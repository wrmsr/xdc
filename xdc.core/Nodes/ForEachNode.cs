using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class ForEachContext : NodeContext<ForEachNode> {
		private int i;
		private NodeValue val;

		public ForEachContext(NodeContext parent, ForEachNode node)
			: base(parent, node) {
		}

		public override IEnumerable<WeakNodeContext> Children {
			get {
				i = 0;

				foreach(string item in Node.Items)
					foreach(Node child in Node.Children) {
						//Yick, soft clone
						//TODO: use namedValues
						ForEachContext clone = new ForEachContext(Parent, Node);
						clone.i = i;
						clone.val = GetValue(item);
						yield return new WeakNodeContext(clone, child);
						i++;
					}
			}
		}

		public override NodeValue GetSingleValue(string name) {
			switch(StripName(name)) {
				case "i": return new StaticNodeValue(Convert.ToString(i));
				case "val": return val;
			}

			return Node.GetValue(name);
		}
	}

	public class ForEachNode : MetaNode {
		public override Type ContextType {
			get { return typeof(ForEachContext); }
		}

		public IEnumerable<string> Items {
			get {
				foreach(string cur in Atts["In"].Split(','))
					yield return cur.Trim();
			}
		}

		public ForEachNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
			if(!Atts.ContainsKey("In"))
				throw new ApplicationException("Requires In attribute");
		}
	}
}
