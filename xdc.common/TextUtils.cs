using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	static public class TextUtils {
		static public string Indent(string str, int ct) {
			return Indent(str, new string(' ', ct));
		}

		static public string Indent(string str, string i) {
			StringBuilder sb = new StringBuilder();

			int c = 0;
			foreach(string line in str.Split('\n')) {
				if(c++ > 0)
					sb.AppendLine();

				if(!string.IsNullOrEmpty(line))
					sb.Append(i + line);
			}

			return sb.ToString();
		}
	}
}
