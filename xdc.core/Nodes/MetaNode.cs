using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public abstract class MetaNode : ProxyNode {
		public MetaNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
