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

		public override int ObjectCount {
			get { return 0; }
		}

		public TerminalNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
