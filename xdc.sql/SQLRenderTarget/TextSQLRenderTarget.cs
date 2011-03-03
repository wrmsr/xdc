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

		private bool silent = false;

		public bool Silent {
			get { return silent; }
		}

		public TextWriter Dst {
			get { return dst; }
		}

		public override IWriter Writer {
			get { return this; }
		}

		public TextSQLRenderTarget(TextWriter _dst) {
			dst = _dst;

			WriteStart();
		}

		public TextSQLRenderTarget(TextWriter _dst, bool _silent) {
			dst = _dst;
			silent = _silent;

			WriteStart();
		}

		public override void Dispose() {
			WriteEnd();
		}

		private void WriteStart() {
			if(!Silent)
				Emit("print '<Objects>';" + Environment.NewLine);
		}

		private void WriteEnd() {
			if(!Silent)
				Emit("print '</Objects>';");
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

		private int objectCount = 0;

		public override void EnterObject(ObjectNode objectNode) {
			if(!Silent) {
				if(++objectCount % 100 == 0)
					Emit(string.Format("print '#{0}';" + Environment.NewLine, objectCount));

				if(objectNode.ObjectClass.Atts.GetBool("Write"))
					Emit(string.Format("print '<{0}>';" + Environment.NewLine, objectNode.ObjectClass.Name));
			}
		}

		public override void LeaveObject(ObjectNode objectNode) {
			if(!Silent)
				if(objectNode.ObjectClass.Atts.GetBool("Write"))
					Emit(string.Format("print '</{0}>';" + Environment.NewLine, objectNode.ObjectClass.Name));
		}

		public override void WriteFieldSQL(FieldNode fieldNode, string sql) {
			if(!Silent)
				Emit(string.Format("print '<{0}>' + {1} + '</{0}>';" + Environment.NewLine, fieldNode.ObjectClassField.Name, sql));
		}
	}
}
