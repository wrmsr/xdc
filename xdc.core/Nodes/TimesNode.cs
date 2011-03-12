using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class TimesContext : NodeContext<TimesNode> {
		private int i = 0;

		public TimesContext(NodeContext parent, TimesNode node)
			: base(parent, node) {
		}

		public override IEnumerable<WeakNodeContext> Children {
			get {
				int to = Node.GetTimes(Root.Rand);

				for(i = 0; i < to; i++)
					foreach(Node child in Node.Children) {
						//Yick, soft clone
						//TODO: use namedValues
						TimesContext clone = new TimesContext(Parent, Node);
						clone.i = i;
						yield return new WeakNodeContext(clone, child);
					}
			}
		}

		public override NodeValue GetSingleValue(string name) {
			switch(StripName(name)) {
				case "i": return new StaticNodeValue(Convert.ToString(i));
			}

			return Node.GetValue(name);
		}
	}

	public class TimesNode : MetaNode {
		public override Type ContextType {
			get { return typeof(TimesContext); }
		}

		public override int ObjectCount {
			get {
				return GetTimes(new Random()) * base.ObjectCount;
			}
		}

		public int GetTimes(Random rand) {
			int to = 0;

			if(Atts.ContainsKey("Rand")) {
				string[] randParts = Atts["Rand"].Split('-');
				to = rand.Next(Convert.ToInt32(randParts[0]), Convert.ToInt32(randParts[1]) + 1);
			}
			else if(Atts.ContainsKey("To"))
				to = Convert.ToInt32(Atts["To"]);

			return to;
		}

		public TimesNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
