using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	static public class TextUtils {
		static public string Indent(string str, int ct) {
			return string.Join(
				"\n" + new string(' ', ct),
				str.Split('\n'));
		}
	}
}
