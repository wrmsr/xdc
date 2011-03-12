using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using xdc.common;

namespace xdc.Nodes {
	public class ExecSQLRenderTargetOptions {
		public bool ProgBar = true;
		public TextWriter SQLDump = null;
		public int BufSize = 0x40000;
	}

	public class ExecSQLRenderTarget : SQLRenderTarget {
		private class Var {
			public string DataType = null;
			public object Value = null;
			public bool IsSet = false;
			public bool IsDeclared = false;
		}

		private Dictionary<string, Var> declaredVars = new Dictionary<string, Var>();

		private SqlConnection conn = null;
		private IWriter writer = null;

		private ExecSQLRenderTargetOptions options = new ExecSQLRenderTargetOptions();

		private int execInterval { get { return options.BufSize - 0x1000; } }

		private StringBuilder buf = null;

		private List<SqlError> errors = new List<SqlError>();

		public SqlConnection Conn {
			get { return conn; }
		}

		public override IWriter Writer {
			get { return writer; }
		}

		public ExecSQLRenderTarget(SqlConnection _conn, IWriter _writer, ExecSQLRenderTargetOptions _options) {
			conn = _conn;
			writer = _writer;
			options = _options;

			buf = new StringBuilder(options.BufSize);
		}

		public override void Dispose() {
			Exec();
		}

		private void RawDeclareVar(string name, string type) {
			buf.AppendLine(string.Format("declare @{0} {1};" + Environment.NewLine, name, type));
		}

		private void RawSetVar(string name, string value) {
			buf.AppendLine(string.Format("set @{0} = {1};" + Environment.NewLine, name, value));
		}

		protected void EmitPrefix() {
			buf.AppendLine("set nocount on;" + Environment.NewLine);

			foreach(KeyValuePair<string, Var> cur in declaredVars) {
				RawDeclareVar(cur.Key, cur.Value.DataType);

				if(cur.Value.Value != null)
					RawSetVar(cur.Key, SQLRenderer.DataTypeToStr(cur.Value.Value, cur.Value.DataType));

				cur.Value.IsDeclared = true;
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
			Var var = null;

			if(declaredVars.TryGetValue(name, out var)) {
				if(var.DataType.ToLower() != type.ToLower())
					throw new ApplicationException("Variable already declared with different type: " + name);

				if(!var.IsDeclared) {
					RawDeclareVar(name, type);

					var.IsDeclared = true;
				}

				return true;
			}

			RawDeclareVar(name, type);

			var = new Var();
			var.DataType = type;
			var.IsSet = true;
			var.IsDeclared = true;

			declaredVars[name] = var;

			return false;
		}

		public override void SetVar(string name, string value) {
			RawSetVar(name, value);

			Var var = null;

			if(!declaredVars.TryGetValue(name, out var))
				throw new ApplicationException("Variable not found: " + name);

			var.IsSet = true;
		}

		public override void Emit(string sql) {
			if(buf.Length == 0)
				EmitPrefix();

			buf.AppendLine(sql);
		}

		private void ThrowSQLError(int errNum, int errLineNum, string errMsg) {
			errMsg = Regex.Replace(errMsg, @"^[\+\-\=][^\n]*(\n|$)", string.Empty, RegexOptions.Multiline);

			throw new ApplicationException(string.Format(
				"SQLError: #{0} @{1}: {2}", errNum, errLineNum, errMsg));
		}

		private Queue<Node> writeQueue = new Queue<Node>();

		private int writeCount = 0;

		private int lastWriteReport = 0;

		public void Exec() {
			EmitSuffix();

			Console.Error.WriteLine("Executing SQL Buffer: {0} Bytes, {1} Writes Queued", buf.Length, writeQueue.Count);

			if(options.SQLDump != null) {
				options.SQLDump.WriteLine(buf.ToString());
				options.SQLDump.WriteLine();
				options.SQLDump.WriteLine("GO");
				options.SQLDump.WriteLine(new string('-', 80));
			}

			SqlInfoMessageEventHandler infoMessage = new SqlInfoMessageEventHandler(conn_InfoMessage);

			writeCount = writeQueue.Count;

			if(options.ProgBar)
				TextUtils.DrawTextProgressBar(0, writeQueue.Count);

			lastWriteReport = 0;

			conn.InfoMessage += infoMessage;
			try {
				using(SqlCommand cmd = new SqlCommand(buf.ToString(), conn)) {
					cmd.CommandTimeout = 12 * 60 * 60;

					using(SqlDataReader reader = cmd.ExecuteReader())
					if(reader.Read())
						for(int i = 0; i < reader.FieldCount; i++) {
							Var var = null;

							if(declaredVars.TryGetValue(reader.GetName(i), out var))
								var.Value = reader.GetValue(i);
						}
				}
			}
			catch(SqlException ex) {
				ThrowSQLError(ex.Number, ex.LineNumber, ex.Message);
			}
			finally {
				conn.InfoMessage -= infoMessage;

				if(options.ProgBar)
					TextUtils.ClearLine();
			}

			if(errors.Count > 0)
				ThrowSQLError(errors[0].Number, errors[0].LineNumber, errors[0].Message);

			if(writeQueue.Count != 0)
				throw new ApplicationException("WriteStack mismatch, items left: " + writeQueue.Count);

			writeCount = 0;

			foreach(Var var in declaredVars.Values)
				var.IsDeclared = false;

			buf.Length = 0;
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

			int writesDone = writeCount - writeQueue.Count;

			if(writesDone - lastWriteReport > 100) {
				lastWriteReport = writesDone;

				if(options.ProgBar)
					TextUtils.DrawTextProgressBar(writesDone, writeCount);
			}
		}

		public override void Flush() {
			if(buf.Length > execInterval)
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
			if(sql.ToLower() == "null")
				throw new ApplicationException("Cannot output NULL field value");

			writeQueue.Enqueue(fieldNode);

			Emit(string.Format("print '=' + {0};" + Environment.NewLine, sql));
		}
	}
}
