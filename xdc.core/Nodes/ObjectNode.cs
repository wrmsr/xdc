using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class ObjectContext : NodeContext<ObjectNode> {
		private List<FieldContext> fields = new List<FieldContext>();

		private List<WeakNodeContext> childObjects = new List<WeakNodeContext>();

		public ObjectClass ObjectClass {
			get { return Node.ObjectClass; }
		}

		public ListAccessor<FieldContext> Fields {
			get { return new ListAccessor<FieldContext>(fields); }
		}

		public IEnumerable<ObjectContext> ChildObjects {
			get {
				foreach(WeakNodeContext childObject in childObjects)
					yield return (ObjectContext)childObject.Context;
			}
		}

		public ObjectContext(NodeContext parent, ObjectNode node)
			: base(parent, node) {
			foreach(WeakNodeContext child in FindDescendents(typeof(FieldNode), typeof(ObjectNode)))
				if(child.Node is FieldNode) {
					fields.RemoveAll(delegate(FieldContext f) {
						return f.ObjectClassField == ((FieldNode)child.Node).ObjectClassField;
					});

					fields.Add((FieldContext)child.Context);
				}
				else if(child.Node is ObjectNode)
					childObjects.Add(child);

			foreach(ObjectClassField objectClassField in ObjectClass.Fields) {
				if(!objectClassField.Atts.GetBool("Required"))
					continue;

				if(!fields.Exists(delegate(FieldContext f) {
					return f.ObjectClassField == objectClassField;
				}))
					throw new ApplicationException("Field required: " + objectClassField.FullName);
			}
		}

		public override NodeValue GetSingleValue(string name) {
			foreach(FieldContext field in fields)
				if(((FieldNode)field.Node).ObjectClassField.IsNamed(name))
					return field.Value;

			return base.GetSingleValue(name);
		}
	}

	public class ObjectNode : Node {
		public override Type ContextType {
			get { return typeof(ObjectContext); }
		}

		static protected new Type[] childTypes = new Type[] { typeof(MetaNode), typeof(ObjectNode), typeof(FieldNode) };
		public override Type[] ChildTypes {
			get { return childTypes; }
		}

		private ObjectClass objectClass = null;

		public ObjectClass ObjectClass {
			get { return objectClass; }
		}

		public override IEnumerable<string> ClassNames {
			get {
				foreach(string cur in ObjectClass.ClassNames)
					yield return cur;
			}
		}

		public ObjectNode(Node parent, Atts atts)
			: base(parent, atts) {
			string className = null;

			if(!atts.TryGetValue("Class", out className) || string.IsNullOrEmpty(className))
				throw new ApplicationException("Object has no class");

			objectClass = ObjectClasses.Get(className);

			if(objectClass == null)
				throw new ApplicationException("Class not found: " + className);

			if(string.IsNullOrEmpty(Name)) {
				int c = 1;

				foreach(Node cur in Parents)
					if(cur.TopClassName == TopClassName)
						c++;

				Atts["Name"] = TopClassName + Convert.ToString(c);
			}

			List<FieldNode> fields = new List<FieldNode>();

			foreach(ObjectClassField f in Enumerations.Reverse(ObjectClass.Fields)) {
				if(!string.IsNullOrEmpty(f.Atts["IsOutput"])) { //IsOutput
					AddChild(new FieldNode(this, new Atts("Name", f.FullName)));
					continue;
				}

				string def = f.Atts["Default"];

				if(!string.IsNullOrEmpty(def)) {
					FieldNode fn = new FieldNode(this, new Atts("Name", f.FullName));
					fn.AddChildren(TextTerminalParser.Parse(fn, def));
					fields.Add(fn);

					continue;
				}
			}

			foreach(KeyValuePair<string, string> att in Atts) {
				ObjectClassField field = ObjectClass.Fields[att.Key];

				if(field != null)
					fields.Add(new FieldNode(this, new Atts("Name", att.Key, "Value", att.Value)));
			}

			//TODO: Sort by inheritence:
			// Highest classes defaults [simples] first
			// Lowest classes ctors [complexes] first
			//TODO: more ingelligent complex sorting - e.g. a RefNode to a Times is still simple
			//TODO: MOVE THIS TO OBJECTCONTEXT - make it get weak FieldNode refs, sort intelligently
			List<Node> complexFields = new List<Node>();
			foreach(FieldNode field in fields)
				if(field.HasChild(typeof(RefNode)))
					complexFields.Add(field);
				else
					AddChild(field);

			AddChildren(complexFields);
		}

		public override string ToStringAnnotation() {
			return TopClassName + ", " + Name;
		}
	}
}
