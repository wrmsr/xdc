using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class ConsoleObjectWriter : IObjectWriter {
		private int indent = 0;

		public string Indent {
			get { return new string(' ', 2 * indent); }
		}

		public void WriteEnterObject(string name) {
			Console.WriteLine(Indent + "<{0}>", name);
			indent++;
		}

		public void WriteLeaveObject(string name) {
			indent--;
			Console.WriteLine(Indent + "</{0}>", name);
		}

		public void WriteField(string name, string value) {
			Console.WriteLine(Indent + "<{0}>{1}</{0}>", name, value);
		}
	}
}
