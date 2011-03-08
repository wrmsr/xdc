using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class SQLRenderer : WritingRenderer {
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

				if(field.Node.ShouldWrite)
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
