using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using xdc.common;

namespace xdc.Nodes {
	public class TextTerminalParser {
		static private Regex escapeRx = new Regex(
			@"\\."); //replace w/ ${a}${b}

		public const string EscapeGuard = @"(?<!(^|[^\\])(\\\\)*?\\)";

		static private Regex chanceRx = new Regex(
			EscapeGuard + @"\$");

		static private Regex tryRx = new Regex(
			EscapeGuard + @"\|");

		static private Regex terminalRx = new Regex(
			EscapeGuard + @"(" +				
				@"(\%(?<const>[^\%]+)\%)|" +
				@"(\{(?<ref>[^\{\}]+)\})|" +
				@"(\#(?<fileval>[^\#]+)\#)" +
			@")");

		public delegate IEnumerable<Node> TextHandler(Node parentNode, string text);
		static public TextHandler RootTexthandler = new TextHandler(ParseText);
		static private IEnumerable<Node> ParseText(Node parentNode, string text) {
			if(!string.IsNullOrEmpty(text))
				yield return new TextNode(parentNode, new Atts("Value", text));
		}

		static private IEnumerable<Node> ParseSingle(Node parentNode, string text) {
			for(Match m = null; (m = terminalRx.Match(text)) != null && m.Success; ) {
				if(m.Index > 0)
					foreach(Node t in RootTexthandler(parentNode, text.Substring(0, m.Index)))
						yield return t;

				if(m.Groups["const"].Success)
					yield return new ConstNode(parentNode, new Atts("Name", m.Groups["const"].Value));
				else if(m.Groups["fileval"].Success)
					yield return  new FileValueNode(parentNode, new Atts("Name", m.Groups["fileval"].Value));
				else if(m.Groups["ref"].Success)
					yield return new RefNode(parentNode, new Atts("Value", m.Groups["ref"].Value));

				text = text.Substring(m.Index + m.Length);
			}

			for(Match m = null; (m = escapeRx.Match(text)) != null && m.Success; ) {
				if(m.Index > 0)
					foreach(Node t in RootTexthandler(parentNode, text.Substring(0, m.Index)))
						yield return t;

				switch(m.Value[1]) {
					case '0':
						yield return new NullNode(parentNode, new Atts());
						break;

					case '\\':
					case '$': case '|':
					case '%': case '{': case '}': case '#':
						yield return new TextNode(parentNode, new Atts("Value", m.Value.Substring(1)));
						break;

					default:
						foreach(Node t in RootTexthandler(parentNode, m.Value))
							yield return t;
						break;
				}

				text = text.Substring(m.Index + m.Length);
			}

			foreach(Node t in RootTexthandler(parentNode, text))
				yield return t;
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
				new Atts("Type", "Even"));

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
