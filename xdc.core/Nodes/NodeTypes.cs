using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class NodeTypes {
		static private Dictionary<string, Type> dict = null;

		static public Dictionary<string, Type> Dict {
			get { return dict ?? (dict = Initial()); }
		}

		static public Dictionary<string, Type> Initial() {
			Dictionary<string, Type> dict = new Dictionary<string, Type>();

			dict.Add("Root", typeof(RootNode));
			dict.Add("Object", typeof(ObjectNode));
			dict.Add("Field", typeof(FieldNode));
			dict.Add("Times", typeof(TimesNode));
			dict.Add("Template", typeof(TemplateNode));
			dict.Add("Chance", typeof(ChanceNode));
			dict.Add("Try", typeof(TryNode));
			dict.Add("Case", typeof(CaseNode));
			dict.Add("Text", typeof(TextNode));
			dict.Add("Ref", typeof(RefNode));
			dict.Add("Const", typeof(ConstNode));
			dict.Add("SetConst", typeof(SetConstNode));
			dict.Add("ValueFile", typeof(ValueFileNode));
			dict.Add("FileValue", typeof(FileValueNode));
			dict.Add("Null", typeof(NullNode));
			dict.Add("ForEach", typeof(ForEachNode));

			return dict;
		}
	}
}
