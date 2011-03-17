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
					if(child.Node.Atts.GetBool("Weak")) {
						if(fields.Find(delegate(FieldContext f) {
							return f.ObjectClassField == ((FieldNode)child.Node).ObjectClassField;
						}) != null)
							continue;
					}
					else
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
			get {
				if(objectClass == null) {
					string className = null;

					if(!Atts.TryGetValue("Class", out className) || string.IsNullOrEmpty(className))
						throw new ApplicationException("Object has no class");

					objectClass = ObjectClasses.Get(className);

					if(objectClass == null)
						throw new ApplicationException("Class not found: " + className);
				}

				return objectClass;
			}
		}

		public override IEnumerable<string> ClassNames {
			get {
				foreach(string cur in ObjectClass.ClassNames)
					yield return cur;
			}
		}

		public bool ShouldWrite {
			get { return Atts.GetBool("Write") || ObjectClass.Atts.GetBool("Write"); }
		}

		private string name = null;
		public override string Name {
			get {
				if(name == null) {
					name = Atts.TryGetValue("Name");

					if(string.IsNullOrEmpty(name)) {
						int c = 1;

						foreach(Node cur in Parents)
							if(cur.TopClassName == TopClassName)
								c++;

						name = TopClassName + Convert.ToString(c);
					}
				}
				
				return name;
			}
		}

		public override int ObjectCount {
			get { return 1 + base.ObjectCount; }
		}

		public ObjectNode(Node parent, Atts atts)
			: base(parent, atts) {
			foreach(ObjectClassField f in Enumerations.Reverse(ObjectClass.Fields)) {
				if(!string.IsNullOrEmpty(f.Atts["IsOutput"])) { //IsOutput
					AddChild(new FieldNode(this, new Atts("Name", f.FullName)));
					continue;
				}

				string def = f.Atts["Default"];

				if(!string.IsNullOrEmpty(def)) {
					FieldNode fn = new FieldNode(this, new Atts("Name", f.FullName));
					fn.AddChildren(TextTerminalParser.Parse(fn, def));
					AddChild(fn);

					continue;
				}
			}

			foreach(KeyValuePair<string, string> att in Atts) {
				string key = att.Key.TrimEnd('.');

				ObjectClassField field = ObjectClass.Fields[key];

				if(field != null) {
					Dictionary<string, string> fieldAtts = new Dictionary<string,string>();
					fieldAtts["Name"] = key;
					fieldAtts["Value"] = att.Value;

					if(att.Key.EndsWith("."))
						fieldAtts["Write"] = "True";

					FieldNode fieldNode = new FieldNode(this, new Atts(fieldAtts));
					AddChild(fieldNode);
				}
			}
		}

		public override string ToStringAnnotation() {
			return TopClassName + ", " + Name;
		}
	}
}
