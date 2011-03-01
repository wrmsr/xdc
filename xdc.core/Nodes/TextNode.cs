using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class TextContext : TerminalContext {
		private NodeValue val;

		public TextContext(NodeContext parent, TextNode node)
			: base(parent, node) {
			val = new StaticNodeValue(((TextNode)Node).Value);
		}

		public override NodeValue Value {
			get { return val; }
		}
	}

	public class TextNode : TerminalNode {
		public override Type ContextType {
			get { return typeof(TextContext); }
		}

		public string Value {
			get { return Atts["Value"]; }
		}

		public TextNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
			if(!Atts.ContainsKey("Value"))
				throw new ApplicationException("TextNode must have value");
		}

		public override string ToStringAnnotation() {
			return Value;
		}
	}
}
