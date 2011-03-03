using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using xdc.common;

namespace xdc.Nodes {
	public class DateContext : TerminalContext {
		private StaticNodeValue val = null;

		public DateContext(NodeContext parent, TerminalNode node)
			: base(parent, node) {
			DateTime to = !string.IsNullOrEmpty(Node.Atts["To"]) ? DateTime.Parse(Node.Atts["To"]) : Root.Now;
			DateTime from  = !string.IsNullOrEmpty(Node.Atts["To"]) ? DateTime.Parse(Node.Atts["To"]) : Root.Now;
			
			TimeSpan diff = to.Subtract(from);
			DateTime dt = to.AddMilliseconds(Root.Rand.Next(diff.Milliseconds));

			val = new StaticNodeValue(dt.ToString(Node.Atts["Fmt"] ?? string.Empty));
		}

		public override NodeValue Value {
			get { return val; }
		}
	}

	public class DateNode : TerminalNode {
		public override Type ContextType {
			get { return typeof(DateContext); }
		}

		public DateNode(Node parent, Atts atts)
			: base(parent, atts) {
		}
	}
}
