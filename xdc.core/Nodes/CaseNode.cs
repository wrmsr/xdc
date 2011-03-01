using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class CaseNode : ProxyNode {
		public override Type[] ChildTypes {
			get { return Parent.Parent.ChildTypes; }
		}

		public CaseNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}
	}
}
