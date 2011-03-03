using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class TryContext : NodeContext {
		public TryContext(NodeContext parent, MetaNode node)
			: base(parent, node) {
		}

		public override IEnumerable<WeakNodeContext> Children {
			get {
				//can't yield return in try, too complex for C# compiler
				CaseNode winner = null;

				foreach(CaseNode caseNode in Node.Children) {
					try {
						NodeContext caseContext = caseNode.CreateContext(this);
						NodeValue val = null;

						foreach(TerminalContext tnc in Enumerations.As<TerminalContext>(caseContext.All))
							val = NodeValue.Concat(val, tnc.Value);

						if(val != null && !(val is NullNodeValue)) {
							winner = caseNode;
							break;
						}
					}
					catch(ApplicationException aex) {
						aex.ToString(); //shut up
					}
				}

				if(winner != null)
					yield return new WeakNodeContext(this, winner);
			}
		}
	}

	public class TryNode : MetaNode {
		public override Type ContextType {
			get { return typeof(TryContext); }
		}

		static protected new Type[] childTypes = new Type[] { typeof(CaseNode) };
		public override Type[] ChildTypes {
			get { return childTypes; }
		}

		public TryNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
