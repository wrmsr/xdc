using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class CaseNode : ProxyNode {
		public override Type[] ChildTypes {
			get { return Parent.Parent.ChildTypes; }
		}

		public CaseNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
