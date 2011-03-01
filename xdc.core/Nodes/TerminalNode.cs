using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public abstract class TerminalContext : NodeContext {
		public TerminalContext(NodeContext parent, TerminalNode node)
			: base(parent, node) {
		}

		public abstract NodeValue Value { get; }
	}

	public abstract class TerminalNode : Node {
		public override Type ContextType {
			get { return typeof(TerminalContext); }
		}

		public TerminalNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}
	}
}