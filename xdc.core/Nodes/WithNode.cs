using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class WithContext : NodeContext<WithNode> {
		public WithContext(NodeContext parent, WithNode node)
			: base(parent, node) {
		}

		public override NodeValue GetSingleValue(string name) {
			if(!string.IsNullOrEmpty(Node.Atts[name]))
				return GetValue(Node.Atts[name]);

			return base.GetSingleValue(name);
		}
	}

	public class WithNode : MetaNode {
		public override Type ContextType {
			get { return typeof(WithContext); }
		}

		public WithNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}
	}
}
