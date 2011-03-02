using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public abstract class SQLRenderTarget : IObjectWriter {
		public abstract IObjectWriter Writer { get; }

		public abstract bool DeclareVar(string name, string type);
		public abstract void SetVar(string name, string value);

		public abstract void Emit(string sql);
		public abstract void Flush();

		public abstract void WriteStart();
		public abstract void WriteEnd();

		public abstract void EnterObject(ObjectNode objectNode);
		public abstract void LeaveObject(ObjectNode objectNode);
		
		public void WriteField(FieldNode fieldNode, string value) {
			throw new NotSupportedException();
		}

		public abstract void WriteFieldSQL(FieldNode fieldNode, string sql);
	}

	public class TextSQLRenderTarget : SQLRenderTarget {
		private Dictionary<string, string> declaredVars = new Dictionary<string, string>();

		private TextWriter dst = null;

		public TextWriter Dst {
			get { return dst; }
		}

		public override IObjectWriter Writer {
			get { return this; }
		}

		public TextSQLRenderTarget(TextWriter _dst) {
			dst = _dst;
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

		public override void WriteStart() {
			Emit("print '<Objects>';");
		}

		public override void WriteEnd() {
			Emit("print '</Objects>';");
		}

		private int objectCount = 0;

		public override void EnterObject(ObjectNode objectNode) {
			if(++objectCount % 100 == 0)
				Emit(string.Format("print '#{0}';" + Environment.NewLine, objectCount));

			if(objectNode.ObjectClass.Atts.GetBool("Write"))
				Emit(string.Format("print '<{0}>';" + Environment.NewLine, objectNode.ObjectClass.Name));
		}

		public override void LeaveObject(ObjectNode objectNode) {
			if(objectNode.ObjectClass.Atts.GetBool("Write"))
				Emit(string.Format("print '</{0}>';" + Environment.NewLine, objectNode.ObjectClass.Name));
		}

		public override void WriteFieldSQL(FieldNode fieldNode, string sql) {
			Emit(string.Format("print '<{0}>' + {1} + '</{0}>';" + Environment.NewLine, fieldNode.ObjectClassField.Name, sql));
		}
	}

	public class ExecSQLRenderTarget : SQLRenderTarget {
		private class Var {
			public string DataType = null;
			public object Value = null;
			public bool IsSet = false;
		}

		private Dictionary<string, Var> declaredVars = new Dictionary<string, Var>();

		private SqlConnection conn = null;
		private IObjectWriter writer = null;

		private const int bufSize = 0x40000;
		private const int execInterval = bufSize - 0x1000;
		private StringBuilder sb = new StringBuilder(bufSize);

		private StringWriter sw = null;
		private TextSQLRenderTarget txt = null;

		public SqlConnection Conn {
			get { return conn; }
		}

		protected TextSQLRenderTarget Txt {
			get {
				if(txt == null) {
					sb.Length = 0;

					sw = new StringWriter(sb);
					txt = new TextSQLRenderTarget(sw);

					EmitPrefix();
				}

				return txt;
			}
		}

		public override IObjectWriter Writer {
			get { return writer; }
		}

		public ExecSQLRenderTarget(SqlConnection _conn, IObjectWriter _writer) {
			conn = _conn;
			writer = _writer;
		}

		public override void WriteStart() {
			throw new NotImplementedException();
		}

		public override void WriteEnd() {
			throw new NotImplementedException();
		}

		protected void EmitPrefix() {
			foreach(KeyValuePair<string, Var> cur in declaredVars) {
				Txt.DeclareVar(cur.Key, cur.Value.DataType);

				if(cur.Value.Value != null)
					Txt.SetVar(cur.Key, SQLRenderer.DataTypeToStr(cur.Value.Value, cur.Value.DataType));
			}
		}

		protected void EmitSuffix() {
			foreach(KeyValuePair<string, Var> cur in declaredVars)
				if(cur.Value.IsSet) {
					Emit(string.Format("select @{0} '{0}';" + Environment.NewLine, cur.Key));
					cur.Value.IsSet = false;
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
				while(reader.Read()) {
					Var var = null;

					if(declaredVars.TryGetValue(reader.GetName(0), out var))
						var.Value = reader.GetValue(0);

					reader.NextResult();
				}
			}
			finally {
				conn.InfoMessage -= infoMessage;
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
					Console.Error.WriteLine(err.ToString());
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

	public class SQLRenderer : Renderer {
		private SQLRenderTarget target;

		protected SQLRenderTarget Target {
			get { return target; }
		}

		public SQLRenderer(SQLRenderTarget _target)
			: base(_target) {
			target = _target;
		}

		public override void Flush() {
			target.Flush();
		}

		static public string QuoteStr(string str) {
			return string.Format("'{0}'", str.Replace("'", "''"));
		}

		static public string DataTypeToStr(object val, string type) {
			if(val == null || val is DBNull)
				return "NULL";
			else if(type == "int")
				return Convert.ToString(Convert.ToInt32(val));
			else
				return QuoteStr(Convert.ToString(val));
		}

		public virtual string TransformValue(FieldContext field, StaticNodeValue sv) {
			return QuoteStr(sv.Value);
		}

		protected virtual string RenderField(StringBuilder sb, FieldContext field) {
			if(field.Value == null)
				throw new ApplicationException("Field has no value: " + field.Name);

			List<TerminalNodeValue> vals = new List<TerminalNodeValue>(field.Value.Terminals);

			if(vals.Count == 0)
				throw new ApplicationException("Field has no value: " + field.Name);

			if(vals.Count == 1 && vals[0] is StaticNodeValue)
				return TransformValue(field, (StaticNodeValue)vals[0]);

			StringBuilder valStr = new StringBuilder();

			int c = 0;
			foreach(TerminalNodeValue tsv in vals) {
				if(tsv is NullNodeValue)
					return "NULL";

				else if(field.ObjectClassField.Atts["DataType"] != "string") {
					if(c > 0)
						throw new ApplicationException("Cannot have compound values for datatype: " + field.ObjectClassField.Atts["DataType"]);

					if(tsv is StaticNodeValue) {
						if(field.ObjectClassField.Atts["DataType"] == "int")
							valStr.Append(Convert.ToString(Convert.ToInt32(((StaticNodeValue)tsv).Value)));
						else
							valStr.Append(QuoteStr(((StaticNodeValue)tsv).Value));
					}
					else if(tsv is DynamicNodeValue)
						valStr.Append("@" + ((DynamicNodeValue)tsv).Context.Name);
				}
				else {
					if(c++ > 0)
						valStr.Append(" + ");

					if(tsv is StaticNodeValue)
						valStr.Append(QuoteStr(((StaticNodeValue)tsv).Value));
					else if(tsv is DynamicNodeValue)
						valStr.AppendFormat("cast(@{0} as varchar)", ((DynamicNodeValue)tsv).Context.Name);
				}
			}

			return valStr.ToString();
		}

		public virtual void RenderTableObjectAs(ObjectContext context, ObjectClass objectClass) {
			StringBuilder sb = new StringBuilder();

			bool writeObject = objectClass.Atts.GetBool("Write");

			sb.AppendFormat("insert into {0} (", objectClass.Atts["Table"]);
			sb.AppendLine();
			sb.Append("\t");

			List<FieldContext> fieldParams = new List<FieldContext>(
				context.Fields.FindAll(delegate(FieldContext fld) {
				return fld.ObjectClassField.Parent.Name == objectClass.Name &&
					!fld.ObjectClassField.Atts.GetBool("IsOutput") &&
					!string.IsNullOrEmpty(fld.ObjectClassField.Atts["Column"]);
			}));

			int c = 0;
			foreach(FieldContext field in fieldParams) {
				if(c++ > 0)
					sb.Append(", ");

				sb.Append(field.ObjectClassField.Atts["Column"]);
			}

			sb.AppendLine();
			sb.AppendLine(") values (");
			sb.Append("\t");

			c = 0;
			foreach(FieldContext field in fieldParams) {
				if(c++ > 0)
					sb.Append(", ");

				string rendered = RenderField(sb, field);

				sb.Append(rendered);

				if(writeObject && field.ObjectClassField.Atts.GetBool("Write"))
					Target.WriteFieldSQL(field.Node, rendered);
			}

			sb.AppendLine();
			sb.AppendLine(");");

			Target.Emit(sb.ToString());

			FieldContext id = context.Fields.Find(delegate(FieldContext fld) {
				return fld.ObjectClassField.Parent.Name == objectClass.Name &&
					fld.ObjectClassField.Atts.GetBool("IsID");
			});

			if(id != null) {
				Target.DeclareVar(id.Name, "int");
				Target.SetVar(id.Name, "scope_identity()");

				if(writeObject && id.ObjectClassField.Atts.GetBool("Write"))
					Target.WriteFieldSQL(id.Node, "cast(scope_identity() as varchar(500))");
			}
		}

		public virtual void RenderProcObjectAs(ObjectContext context, ObjectClass objectClass) {
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("exec {0}", objectClass.Atts["Proc"]);
			sb.AppendLine();

			int c = 0;
			foreach(FieldContext field in context.Fields) {
				if(c++ > 0)
					sb.AppendLine(", ");

				sb.AppendFormat("\t{0} = {1}",
					field.ObjectClassField.Atts["Param"],
					RenderField(sb, field));

				if(field.ObjectClassField.Atts.GetBool("IsOutput")) {
					Target.DeclareVar(field.Name, "int");

					sb.Append(" out");
				}
			}

			sb.AppendLine(";");

			Target.Emit(sb.ToString());
		}

		public override void RenderObjectAs(ObjectContext context, ObjectClass objectClass) {
			if(!string.IsNullOrEmpty(objectClass.Atts["Table"]))
				RenderTableObjectAs(context, objectClass);
			else if(!string.IsNullOrEmpty(objectClass.Atts["Proc"]))
				RenderProcObjectAs(context, objectClass);
		}
	}
}
