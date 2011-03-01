using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using xdc.common;

namespace xdc.Nodes {
	public class TextTerminalParser {
		static private Regex tryRx = new Regex(
			@"^(?<try>([^\|]|(\|\|))+)\|([^\|]|$)");

		static private Regex terminalRx = new Regex(
			@"(\%(?<const>[^\%]+)\%)|" +
			@"(\{(?<ref>[^\{\}]+)\})|" +
			@"(\#(?<fileval>[^\#]+)\#)"
			);

		static private IEnumerable<Node> ParseSingle(Node parentNode, string text) {
			for(Match m = null; (m = terminalRx.Match(text)) != null && m.Success; ) {
				if(m.Index > 0)
					yield return new TextNode(parentNode, Node.MakeAtts("Value", text.Substring(0, m.Index)));

				if(m.Groups["const"].Success)
					yield return new ConstNode(parentNode, Node.MakeAtts("Name", m.Groups["const"].Value));
				else if(m.Groups["fileval"].Success)
					yield return  new FileValueNode(parentNode, Node.MakeAtts("Name", m.Groups["fileval"].Value));
				else if(m.Groups["ref"].Success)
					yield return new RefNode(parentNode, Node.MakeAtts("Value", m.Groups["ref"].Value));

				text = text.Substring(m.Index + m.Length);
			}

			if(!string.IsNullOrEmpty(text))
				yield return new TextNode(parentNode, Node.MakeAtts("Value", text));
		}

		static public IEnumerable<Node> Parse(Node parentNode, string text) {
			List<string> tries = new List<string>();

			for(Match m = null; (m = tryRx.Match(text)) != null && m.Success; ) {
				tries.Add(m.Groups["try"].Value);

				text = text.Substring(m.Length - 1);
			}

			if(tries.Count < 1)
				return ParseSingle(parentNode, text.Replace("||", "|"));

			tries.Add(text);

			TryNode tryNode = new TryNode(parentNode, null);

			foreach(string cur in tries) {
				CaseNode caseNode = new CaseNode(tryNode, null);
				tryNode.AddChild(caseNode);

				caseNode.AddChildren(ParseSingle(caseNode, cur.Replace("||", "|")));
			}

			return Enumerations.One<Node>(tryNode);
		}
	}
}
