using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class ExecSQLRenderTarget : SQLRenderTarget {
		private class Var {
			public string DataType = null;
			public object Value = null;
			public bool IsSet = false;
		}

		private Dictionary<string, Var> declaredVars = new Dictionary<string, Var>();

		private SqlConnection conn = null;
		private IWriter writer = null;

		private const int bufSize = 0x40000;
		private const int execInterval = bufSize - 0x1000;
		private StringBuilder sb = new StringBuilder(bufSize);

		private StringWriter sw = null;
		private TextSQLRenderTarget txt = null;

		private List<SqlError> errors = new List<SqlError>();

		public SqlConnection Conn {
			get { return conn; }
		}

		protected TextSQLRenderTarget Txt {
			get {
				if(txt == null) {
					sb.Length = 0;

					sw = new StringWriter(sb);
					txt = new TextSQLRenderTarget(sw, true);

					EmitPrefix();
				}

				return txt;
			}
		}

		public override IWriter Writer {
			get { return writer; }
		}

		public ExecSQLRenderTarget(SqlConnection _conn, IWriter _writer) {
			conn = _conn;
			writer = _writer;
		}

		public override void Dispose() {
			Txt.Flush();

			Exec();
		}

		protected void EmitPrefix() {
			foreach(KeyValuePair<string, Var> cur in declaredVars) {
				Txt.DeclareVar(cur.Key, cur.Value.DataType);

				if(cur.Value.Value != null)
					Txt.SetVar(cur.Key, SQLRenderer.DataTypeToStr(cur.Value.Value, cur.Value.DataType));
			}
		}

		protected void EmitSuffix() {
			StringBuilder sb = new StringBuilder();

			foreach(KeyValuePair<string, Var> cur in declaredVars)
				if(cur.Value.IsSet) {
					if(sb.Length > 0)
						sb.AppendLine(", ");
					else
						sb.AppendLine("select ");

					sb.AppendFormat("\t@{0} '{0}'", cur.Key);

					cur.Value.IsSet = false;
				}

			if(sb.Length > 0) {
				sb.Append(";");

				Emit(sb.ToString());
			}
		}

		public override bool DeclareVar(string name, string type) {
			Txt.DeclareVar(name, type);

			Var existingVar = null;

			if(declaredVars.TryGetValue(name, out existingVar)) {
				if(existingVar.DataType.ToLower() != type.ToLower())
					throw new ApplicationException("Variable already declared with different type: " + name);

				return true;
			}

			Var var = new Var();
			var.DataType = type;
			var.IsSet = true;

			declaredVars[name] = var;

			return false;
		}

		public override void SetVar(string name, string value) {
			Txt.SetVar(name, value);

			Var existingVar = null;

			if(!declaredVars.TryGetValue(name, out existingVar))
				throw new ApplicationException("Variable not found: " + name);

			existingVar.IsSet = true;
		}

		public override void Emit(string sql) {
			Txt.Emit(sql);
		}

		private Queue<Node> writeQueue = new Queue<Node>();

		public void Exec() {
			EmitSuffix();
			Txt.Flush();
			sw.Flush();

			Console.Error.WriteLine("Executing SQL Buffer: {0} Bytes, {1} Writes Queued", sb.Length, writeQueue.Count);

			SqlInfoMessageEventHandler infoMessage = new SqlInfoMessageEventHandler(conn_InfoMessage);

			conn.InfoMessage += infoMessage;
			try {
				using(SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
				using(SqlDataReader reader = cmd.ExecuteReader())
				if(reader.Read())
				for(int i = 0; i < reader.FieldCount; i++) {
					Var var = null;

					if(declaredVars.TryGetValue(reader.GetName(i), out var))
						var.Value = reader.GetValue(i);
				}
			}
			finally {
				conn.InfoMessage -= infoMessage;
			}

			if(errors.Count > 0) {
				SqlError err = errors[0];

				throw new ApplicationException(string.Format(
					"SQLError: #{0} @{1}: {2}", err.Number, err.LineNumber, err.Message));
			}

			if(writeQueue.Count != 0)
				throw new ApplicationException("WriteStack mismatch, items left: " + writeQueue.Count);

			txt = null;
		}

		void conn_InfoMessage(object sender, SqlInfoMessageEventArgs e) {
			foreach(SqlError err in e.Errors) {
				if(err.Message == "+")
					Writer.EnterObject((ObjectNode)writeQueue.Dequeue());
				else if(err.Message == "-")
					Writer.LeaveObject((ObjectNode)writeQueue.Dequeue());
				else if(err.Message.StartsWith("="))
					Writer.WriteField((FieldNode)writeQueue.Dequeue(), err.Message.Substring(1));
				else
					errors.Add(err);
			}
		}

		public override void Flush() {
			Txt.Flush();

			if(sb.Length > execInterval)
				Exec();
		}

		public override void EnterObject(ObjectNode objectNode) {
			writeQueue.Enqueue(objectNode);
			Emit("print '+';" + Environment.NewLine);
		}

		public override void LeaveObject(ObjectNode objectNode) {
			writeQueue.Enqueue(objectNode);
			Emit("print '-';" + Environment.NewLine);
		}

		public override void WriteFieldSQL(FieldNode fieldNode, string sql) {
			writeQueue.Enqueue(fieldNode);
			Emit(string.Format("print '=' + {0};" + Environment.NewLine, sql));
		}
	}
}
