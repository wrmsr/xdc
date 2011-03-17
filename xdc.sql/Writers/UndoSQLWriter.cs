using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	/*
	2005 only:

	begin transaction;
	begin try;
	...
	commit transaction;
	end try
	begin catch;
		declare @errsev int; set @errsev = error_severity();
		declare @errnum int; set @errnum = error_number();
		declare @errmsg nvarchar(4000); set @errmsg = error_message();
		declare @errst int; set @errst = error_state();
		if @errst = 0 set @errst = 1;
		raiserror(@errmsg, @errsev, @errst, @errnum);
		if xact_state() < 0 rollback transaction;
	end catch
	close o;
	deallocate o;
	drop table #objects;
	*/

	public class SQLUndoWriter : IWriter {
		private bool escape = false;

		private TextWriter tw;

		private Set<ObjectClassField> idFields = new Set<ObjectClassField>();

		public SQLUndoWriter(TextWriter _tw) {
			tw = _tw;

			WriteStart();
		}

		public SQLUndoWriter(TextWriter _tw, bool _escape) {
			tw = _tw;
			escape = _escape;

			WriteStart();
		}

		public void Dispose() {
			WriteEnd();
		}

		private void Emit() {
			Emit(string.Empty);
		}

		private void Emit(string str) {
			if(escape)
				str = str.Replace("'", "''");

			tw.WriteLine(str);
		}

		public void WriteStart() {
			Emit("set nocount on;");
			Emit();

			Emit("begin transaction;");
			Emit();

			Emit("create table #objects (pk int identity(1, 1) primary key, fk int not null, class varchar(50));");
			Emit();
		}

		public void WriteEnd() {
			Emit();

			Emit("declare @fk int;");
			Emit("declare @class varchar(50);");
			Emit();
			Emit("declare o cursor for select fk, class from #objects order by pk desc;");
			Emit("open o;");
			Emit();
			Emit("fetch next from o into @fk, @class;");
			Emit("while @@fetch_status = 0");
			Emit("begin;");
			Emit("\tprint @class + ': ' + cast(@fk as varchar(50));");
			Emit();

			int i = 0;
			foreach(ObjectClassField id in idFields) {
				StringBuilder sb = new StringBuilder();

				sb.Append("\t");

				if(i++ > 0)
					sb.Append("else ");

				sb.AppendFormat("if @class = '{0}'" + Environment.NewLine,
					id.Parent.Name);

				sb.AppendFormat("\t\tdelete from {0} where {1} = @fk;" + Environment.NewLine,
					id.Parent.Atts["Table"],
					id.Atts["Column"]);

				Emit(sb.ToString());
			}

			Emit("\telse");
			Emit("\tbegin;");
			Emit("\t\tdeclare @errtxt varchar(50);");
			Emit("\t\tset @errtxt = 'Unknown object type: ' + cast(@class as varchar(50));");
			Emit();
			Emit("\t\traiserror(@errtxt, 18, -1);");
			Emit("\tend;");

			Emit();
			Emit("\tif @@rowcount <> 1");
			Emit("\t\tprint 'Object not found: ' + cast(@class as varchar(50)) + ' ' + cast(@fk as varchar(50));");
			
			Emit();
			Emit("\tfetch next from o into @fk, @class;");
			Emit("end;");
			Emit();
			Emit("close o;");
			Emit("deallocate o;");
			Emit();
			Emit("drop table #objects;");

			Emit();
			Emit("commit transaction;");
		}

		public void EnterObject(ObjectNode objectNode) {
			
		}

		public void LeaveObject(ObjectNode objectNode) {
			
		}

		public void WriteIDField(FieldNode fieldNode, string valueSQL) {
			string fmt = "insert into #objects(fk, class) values ({0}, '{1}');";

			if(escape)
				fmt = fmt.Replace("'", "''");

			tw.WriteLine(fmt, valueSQL, fieldNode.ObjectClassField.Parent.Name);

			idFields.TryAdd(fieldNode.ObjectClassField);
		}

		public void WriteField(FieldNode fieldNode, string value) {
			if(fieldNode.ObjectClassField.Atts.GetBool("IsID")
				&& !string.IsNullOrEmpty(fieldNode.ObjectClassField.Atts["Column"])
				&& !string.IsNullOrEmpty(fieldNode.ObjectClassField.Parent.Atts["Table"]))
				WriteIDField(fieldNode, Convert.ToString(Convert.ToInt32(value)));
		}
	}
}
