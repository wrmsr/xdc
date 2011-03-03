using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	//PassThrough node but NOT a MetaNode
	//metanodes go anywhere, not all proxies do (for ex. CaseNode)
	public abstract class ProxyNode : Node {
		public override Type[] ChildTypes {
			get { return Parent.ChildTypes; }
		}

		public ProxyNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
