using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class RenderObject {
		private ObjectNode.Context context;

		private bool expanded = false;

		private List<RenderObjectField> fields = new List<RenderObjectField>();

		private RenderObject parent = null;

		private List<ObjectNode.Context> children = new List<ObjectNode.Context>();

		public ObjectNode.Context Context {
			get { return context; }
		}

		public ObjectNode ObjectNode {
			get { return (ObjectNode)context.Node; }
		}

		public string Name {
			get { return ObjectNode.Name; }
		}

		public ObjectClass ObjectClass {
			get { return ObjectNode.ObjectClass; }
		}

		public RenderObject Parent {
			get { return parent; }
		}

		public ListAccessor<RenderObjectField> Fields {
			get { Expand(); return new ListAccessor<RenderObjectField>(fields); }
		}

		public void AddField(RenderObjectField child) {
			fields.RemoveAll(delegate(RenderObjectField f) {
				return f.ObjectClassField.FullName == child.ObjectClassField.FullName;
			});

			fields.Add(child);
		}

		public IEnumerable<RenderObject> Children {
			get {
				foreach(ObjectNode.Context currentContext in children)
					yield return new RenderObject(currentContext);
			}
		}

		private void ProcessContext(Node.Context currentContext) {
			if(currentContext is ObjectNode.Context) {
				children.Add((ObjectNode.Context)currentContext);
			}
			else {
				if(currentContext is FieldNode.Context)
					fields.Add(((FieldNode.Context)currentContext).RenderObjectField);

				foreach(Node childNode in currentContext.Node.Children)
					ProcessContext(childNode.CreateContext());
			}			
		}

		public RenderObject(ObjectNode.Context _context) {
			context = _context;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			sb.Append(Name);

			foreach(RenderObjectField field in Fields) {
				sb.AppendLine();
				sb.Append(TextUtils.Indent(field.ToString(), 2));
			}

			foreach(RenderObject child in Children) {
				sb.AppendLine();
				sb.Append(TextUtils.Indent(child.ToString(), 2));
			}

			return sb.ToString();
		}
	}

	public class RenderObjectField {
		private RenderObject renderObject = null;

		private ObjectClassField objectClassField = null;

		private NodeValue value = null;

		public RenderObject RenderObject {
			get { return renderObject; }
		}

		public ObjectClassField ObjectClassField {
			get { return objectClassField; }
		}

		public NodeValue Value {
			get { return value; }
		}

		public string Name {
			get { return string.Format("{0}_{1}_{2}", RenderObject.Name, ObjectClassField.Parent.Name, ObjectClassField.Name); }
		}

		public void AddValue(NodeValue _value) {
			value = NodeValue.Concat(value, _value);
		}

		public RenderObjectField(RenderObject _renderObject, ObjectClassField _objectClassField) {
			renderObject = _renderObject;
			objectClassField = _objectClassField;
		}

		public RenderObjectField(RenderObject _renderObject, ObjectClassField _objectClassField, NodeValue _value) {
			renderObject = _renderObject;
			objectClassField = _objectClassField;
			value = _value;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("@" + Name);

			sb.Append(TextUtils.Indent(Value.ToString(), 2));

			return sb.ToString();
		}
	}
}
