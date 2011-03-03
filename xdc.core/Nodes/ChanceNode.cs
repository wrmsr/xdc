using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class ChanceContext : NodeContext {
		public ChanceContext(NodeContext parent, ChanceNode node)
			: base(parent, node) {
		}

		public override IEnumerable<WeakNodeContext> Children {
			get {
				if(Node.Atts["Type"] == "Even") {
					int idx = Root.Rand.Next(Node.Children.Count);

					yield return new WeakNodeContext(this, Node.Children[idx]);
				}
				else {
					double num = Root.Rand.NextDouble();
					double cumulative = 0;

					foreach(Node child in Node.Children) {
						string strn = child.Atts["n"];
						double n = Convert.ToDouble(strn);

						if(string.IsNullOrEmpty(strn) || (n + cumulative) >= num) {
							yield return new WeakNodeContext(this, child);
							yield break;
						}

						cumulative += n;
					}
				}
			}
		}
	}

	public class ChanceNode : MetaNode {
		public override Type ContextType {
			get { return typeof(ChanceContext); }
		}

		static protected new Type[] childTypes = new Type[] { typeof(CaseNode) };
		public override Type[] ChildTypes {
			get { return childTypes; }
		}

		public ChanceNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}
	}
}
