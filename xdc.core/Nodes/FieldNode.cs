using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class FieldContext : NodeContext<FieldNode> {
		private bool evaluating = false;

		private NodeValue value = null;

		public ObjectClassField ObjectClassField {
			get { return Node.ObjectClassField; }
		}

		public NodeValue Value {
			get {
				if(value == null) {
					if(evaluating)
						throw new ApplicationException("Circular dependency detected: " + Name);
					
					evaluating = true;
					
					if(!string.IsNullOrEmpty(ObjectClassField.Atts["IsOutput"]))
						value = new DynamicNodeValue(this);
					else
						foreach(NodeContext child in RecursiveChildren)
							if(child is TerminalContext)
								value = NodeValue.Concat(value, ((TerminalContext)child).Value);

					evaluating = false;
				}

				return value;
			}
		}

		public string Name {
			get { return string.Format("{0}_{1}_{2}", CurrentObject.Node.Name, ObjectClassField.Parent.Name, ObjectClassField.Name); }
		}

		public FieldContext(NodeContext parent, FieldNode node)
			: base(parent, node) {
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("@" + Name);

			sb.Append(TextUtils.Indent(Value.ToString(), 2));

			return sb.ToString();
		}
	}

	public class FieldNode : Node {
		public override Type ContextType {
			get { return typeof(FieldContext); }
		}

		static protected new Type[] childTypes = new Type[] { typeof(MetaNode), typeof(TerminalNode) };
		public override Type[] ChildTypes {
			get { return childTypes; }
		}

		private ObjectClassField objectClassField = null;

		public ObjectClassField ObjectClassField {
			get { return objectClassField; }
		}

		public bool ShouldWrite {
			get { return Atts.GetBool("Write") || ObjectClassField.Atts.GetBool("Write"); }
		}

		public FieldNode(Node parent, Atts atts)
			: base(parent, atts) {
			string fieldName = null;

			if(!atts.TryGetValue("Name", out fieldName) || string.IsNullOrEmpty(fieldName))
				throw new ApplicationException("Field has no name");

			ObjectNode parentObject = ParentObject;

			if(parentObject == null)
				throw new ApplicationException("Field has no parent object: " + fieldName);

			objectClassField = ParentObject.ObjectClass.Fields[fieldName];

			if(objectClassField == null)
				throw new ApplicationException("Field not found: " + fieldName);

			string fileValue = null;
			if(Atts.TryGetValue("FileValue", out fileValue) && !string.IsNullOrEmpty(fileValue))
				AddChild(new FileValueNode(this, new Atts("Value", fileValue)));

			string value = null;
			if(Atts.TryGetValue("Value", out value) && !string.IsNullOrEmpty(value))
				AddChildren(TextTerminalParser.Parse(this, value));
		}

		public override string ToStringAnnotation() {
			return ObjectClassField.FullName;
		}
	}
}
