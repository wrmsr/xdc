using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class ConstShared {
		private Dictionary<string, string> consts = new Dictionary<string, string>();

		public Dictionary<string, string> Consts {
			get { return consts; }
		}

		public ConstShared() {
			consts.Add("DATE", DateTime.Now.ToString());
		}
	}

	public class ConstContext : TerminalContext {
		private string val = null;

		public ConstContext(NodeContext parent, ConstNode node)
			: base(parent, node) {
			Root.GetShared<ConstShared>().Consts.TryGetValue(Node.Name, out val);
		}

		public override NodeValue Value {
			get { return val == null ? (NodeValue)new NullNodeValue() : new StaticNodeValue(val); }
		}
	}

	public class ConstNode : TerminalNode {
		public override Type ContextType {
			get { return typeof(ConstContext); }
		}

		public ConstNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}

		public override string ToStringAnnotation() {
			return Name;
		}
	}
}
