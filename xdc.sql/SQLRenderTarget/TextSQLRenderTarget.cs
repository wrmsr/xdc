using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class TextSQLRenderTarget : SQLRenderTarget {
		private Dictionary<string, string> declaredVars = new Dictionary<string, string>();

		private TextWriter dst = null;

		public TextWriter Dst {
			get { return dst; }
		}

		public override IWriter Writer {
			get { return this; }
		}

		public TextSQLRenderTarget(TextWriter _dst) {
			dst = _dst;
		}

		public override void Dispose() {
			Flush();
		}

		public override bool DeclareVar(string name, string type) {
			string existingType = null;

			if(declaredVars.TryGetValue(name, out existingType)) {
				if(existingType.ToLower() != type.ToLower())
					throw new ApplicationException("Variable already declared with different type: " + name);

				return true;
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("declare @{0} {1};", name, type);
			sb.AppendLine();

			declaredVars[name] = type;

			Emit(sb.ToString());

			return false;
		}

		public override void SetVar(string name, string value) {
			Emit(string.Format("set @{0} = {1};" + Environment.NewLine, name, value));
		}

		public override void Emit(string sql) {
			dst.WriteLine(sql);
		}

		public override void Flush() {
			dst.Flush();
		}

		public override void EnterObject(ObjectNode objectNode) {
		}

		public override void LeaveObject(ObjectNode objectNode) {
		}

		public override void WriteFieldSQL(FieldNode fieldNode, string sql) {
		}
	}
}
