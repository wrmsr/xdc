using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public abstract class SQLRenderTarget : IWriter {
		public abstract IWriter Writer { get; }

		public abstract bool DeclareVar(string name, string type);
		public abstract void SetVar(string name, string value);

		public abstract void Emit(string sql);
		public abstract void Flush();

		public abstract void EnterObject(ObjectNode objectNode);
		public abstract void LeaveObject(ObjectNode objectNode);

		public void WriteField(FieldNode fieldNode, string value) {
			throw new NotSupportedException();
		}

		public abstract void WriteFieldSQL(FieldNode fieldNode, string sql);

		public abstract void Dispose();
	}
}
