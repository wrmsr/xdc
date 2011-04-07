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
				Pair<int> range = Node.GetRange(Root.Rand);

				for(i = range.a; i < range.b; i++)
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
				Pair<int> range = GetRange(new Random());
				return (range.b - range.a) * base.ObjectCount;
			}
		}

		public Pair<int> GetRange(Random rand) {
			Pair<int> ret = new Pair<int>(0, 0);

			if(Atts.ContainsKey("Rand")) {
				string[] randParts = Atts["Rand"].Split('-');
				ret.b = rand.Next(Convert.ToInt32(randParts[0]), Convert.ToInt32(randParts[1]) + 1);
			}
			else if(Atts.ContainsKey("To"))
				ret.b = Convert.ToInt32(Atts["To"]);

			if(Atts.ContainsKey("From"))
				ret.a = Convert.ToInt32(atts["From"]);

			return ret;
		}

		public TimesNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
