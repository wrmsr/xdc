using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public interface IObjectWriter {
		void WriteEnterObject(string name);
		void WriteLeaveObject(string name);
		void WriteField(string name, string value);
	}
}
