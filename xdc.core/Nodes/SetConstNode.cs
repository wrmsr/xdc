using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class SetConstContext : NodeContext {
		public SetConstContext(NodeContext parent, SetConstNode node)
			: base(parent, node) {
			Root.GetShared<ConstShared>().Consts[Node.Name] = GetStr(Node.Atts["Value"]);
		}
	}

	public class SetConstNode : MetaNode {
		public override Type ContextType {
			get { return typeof(SetConstContext); }
		}

		//mountain goats reference
		static protected new Type[] childTypes = new Type[] { };
		public override Type[] ChildTypes {
			get { return childTypes; }
		}

		public SetConstNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
