using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class RefContext : TerminalContext {
		public RefContext(NodeContext parent, RefNode node)
			: base(parent, node) {
		}

		public override NodeValue Value {
			get { return GetRefValue(((RefNode)Node).Value) ?? new NullNodeValue(); }
		}
	}

	public class RefNode : TerminalNode {
		public override Type ContextType {
			get { return typeof(RefContext); }
		}

		public string Value {
			get { return Atts["Value"]; }
		}

		public RefNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
			if(!Atts.ContainsKey("Value"))
				throw new ApplicationException("RefNode must have value");
		}

		public override string ToStringAnnotation() {
			return Value;
		}
	}
}
