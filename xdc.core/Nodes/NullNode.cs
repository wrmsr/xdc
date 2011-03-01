using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class NullContext : TerminalContext {
		public NullContext(NodeContext parent, NullNode node)
			: base(parent, node) {
		}

		public override NodeValue Value {
			get { return new NullNodeValue(); }
		}
	}

	public class NullNode : TerminalNode {
		public override Type ContextType {
			get { return typeof(NullContext); }
		}

		public NullNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}
	}
}
