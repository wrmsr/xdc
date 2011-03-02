using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using xdc.common;

namespace xdc.Nodes {
	public class TextTerminalParser {
		static private Regex escapeRx = new Regex(
			@"(?<=(^|[^\\]))(?<a>(\\\\)*?)\\(?<b>.)"); //replace w/ ${a}${b}

		private const string escapeGuard = @"(?<!(^|[^\\])(\\\\)*?\\)";

		static private Regex chanceRx = new Regex(
			escapeGuard + @"\$");

		static private Regex tryRx = new Regex(
			escapeGuard + @"\|");

		static private Regex terminalRx = new Regex(
			escapeGuard + @"(" +
				@"(\%(?<const>[^\%]+)\%)|" +
				@"(\{(?<ref>[^\{\}]+)\})|" +
				@"(\#(?<fileval>[^\#]+)\#)" +
			@")");

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

			text = escapeRx.Replace(text, "${a}${b}");

			if(!string.IsNullOrEmpty(text))
				yield return new TextNode(parentNode, Node.MakeAtts("Value", text));
		}

		static private IEnumerable<Node> ParseTries(Node parentNode, string text) {
			string[] tries = tryRx.Split(text);

			if(tries.Length < 2)
				return ParseSingle(parentNode, text);

			TryNode tryNode = new TryNode(parentNode, null);

			foreach(string cur in tries) {
				CaseNode caseNode = new CaseNode(tryNode, null);

				tryNode.AddChild(caseNode);

				caseNode.AddChildren(ParseSingle(caseNode, cur));
			}

			return Enumerations.One<Node>(tryNode);
		}

		static private IEnumerable<Node> ParseChances(Node parentNode, string text) {
			string[] chances = chanceRx.Split(text);

			if(chances.Length < 2)
				return ParseTries(parentNode, text);

			ChanceNode chanceNode = new ChanceNode(parentNode, 
				Node.MakeAtts("Type", "Even"));

			foreach(string cur in chances) {
				CaseNode caseNode = new CaseNode(chanceNode, null);

				chanceNode.AddChild(caseNode);

				caseNode.AddChildren(ParseSingle(caseNode, cur));
			}

			return Enumerations.One<Node>(chanceNode);
		}

		static public IEnumerable<Node> Parse(Node parentNode, string text) {
			return ParseChances(parentNode, text);
		}
	}
}
