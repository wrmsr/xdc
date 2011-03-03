using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	//Immutable
	public abstract class NodeValue {
		//Must contain multiple items
		//Must not contain back to back static values
		//  (so you can simply 'is StaticNodeValue')
		//Enforced by NodeValue, this is a dumb container
		protected class CompoundNodeValue : NodeValue {
			private List<TerminalNodeValue> children = new List<TerminalNodeValue>();

			public override IEnumerable<TerminalNodeValue> Terminals {
				get { return children; }
			}

			public CompoundNodeValue(IEnumerable<TerminalNodeValue> _children) {
				children.AddRange(_children);

				if(children.Count < 2)
					throw new ApplicationException("CompoundNodeValue must contain multiple values");
			}

			public override string ToString() {
				StringBuilder sb = new StringBuilder();

				sb.AppendLine("CompoundNodeValue:");

				foreach(TerminalNodeValue cur in Terminals) {
					sb.AppendLine();
					sb.Append("  " + cur.ToString());
				}

				return sb.ToString();
			}
		}

		public virtual IEnumerable<TerminalNodeValue> Terminals {
			get {
				yield return this as TerminalNodeValue;
			}
		}

		public virtual NodeValue Concat(NodeValue other) {
			if(other == null)
				return this;

			return new CompoundNodeValue(Enumerations.Combine(Terminals, other.Terminals));
		}

		public override string ToString() {
			return GetType().Name +
				(string.IsNullOrEmpty(Display) ? string.Empty :
					": " + Display);
		}

		public virtual string Display {
			get { return null; }
		}

		static public NodeValue Concat(NodeValue a, NodeValue b) {
			return
				a == null ? b :
				b is NullNodeValue ? b :
				a.Concat(b);
		}

		static public NodeValue Concat(IEnumerable<NodeValue> values) {
			NodeValue ret = null;

			foreach(NodeValue value in values)
				ret = NodeValue.Concat(ret, value);

			return ret;
		}
	}

	public abstract class TerminalNodeValue : NodeValue {

	}

	public class StaticNodeValue : TerminalNodeValue {
		private string value = null;

		public string Value {
			get { return value; }
		}

		public StaticNodeValue(string _value) {
			value = _value;
		}

		public override NodeValue Concat(NodeValue other) {
			if(other is StaticNodeValue)
				return new StaticNodeValue(Value + ((StaticNodeValue)other).Value);
			else
				return base.Concat(other);
		}

		public override string Display {
			get { return Value; }
		}
	}

	public class DynamicNodeValue : TerminalNodeValue {
		private FieldContext context;

		public FieldContext Context {
			get { return context; }
		}

		public DynamicNodeValue(FieldContext _context) {
			context = _context;
		}

		public override string Display {
			get { return Context.Name; }
		}
	}

	public class NullNodeValue : TerminalNodeValue {
		public override NodeValue Concat(NodeValue other) {
			return this;
		}
	}
}
