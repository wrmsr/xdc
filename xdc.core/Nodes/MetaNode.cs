using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public abstract class MetaNode : ProxyNode {
		public MetaNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}
	}
}
