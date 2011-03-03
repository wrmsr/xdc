using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class SQLUndoWriter : IWriter {
		private TextWriter tw;

		private Set<ObjectClassField> idFields = new Set<ObjectClassField>();

		public SQLUndoWriter(TextWriter _tw) {
			tw = _tw;

			WriteStart();
		}

		public void Dispose() {
			WriteEnd();
		}

		public void WriteStart() {
			tw.WriteLine("begin transaction;");
			tw.WriteLine();

			tw.WriteLine("create table #objects (pk int identity(1, 1) primary key, fk int not null, class varchar(50));");
			tw.WriteLine();
		}

		public void WriteEnd() {
			tw.WriteLine();

			tw.WriteLine("declare @fk int;");
			tw.WriteLine("declare @class varchar(50);");
			tw.WriteLine();
			tw.WriteLine("declare o cursor for select fk, class from #objects order by pk desc;");
			tw.WriteLine("open o;");
			tw.WriteLine();
			tw.WriteLine("fetch next from o into @fk, @class;");
			tw.WriteLine("while @@fetch_status = 0");
			tw.WriteLine("begin;");
			tw.WriteLine("\tprint @class + ': ' + cast(@fk as varchar(50));");
			tw.WriteLine();

			int i = 0;
			foreach(ObjectClassField id in idFields) {
				tw.Write("\t");

				if(i++ > 0)
					tw.Write("else ");

				tw.WriteLine("if @class = '{0}'",
					id.Parent.Name);

				tw.WriteLine("\t\tdelete from {0} where {1} = @fk;",
					id.Parent.Atts["Table"],
					id.Atts["Column"]);
			}

			tw.WriteLine("\telse");
			tw.WriteLine("\t\traiseerror('omfg');");

			tw.WriteLine();
			tw.WriteLine("\tfetch next from o into @fk, @class;");
			tw.WriteLine("end;");
			tw.WriteLine();
			tw.WriteLine("close o;");
			tw.WriteLine("deallocate o;");
			tw.WriteLine();
			tw.WriteLine("drop table #objects;");
		}

		public void EnterObject(ObjectNode objectNode) {
			
		}

		public void LeaveObject(ObjectNode objectNode) {
			
		}

		public void WriteField(FieldNode fieldNode, string value) {
			if(fieldNode.ObjectClassField.Atts.GetBool("IsID")
				&& !string.IsNullOrEmpty(fieldNode.ObjectClassField.Atts["Column"])
				&& !string.IsNullOrEmpty(fieldNode.ObjectClassField.Parent.Atts["Table"])) {
				tw.WriteLine("insert into #objects(fk, class) values ({0}, '{1}');",
					Convert.ToInt32(value),
					fieldNode.ObjectClassField.Parent.Name);

				idFields.TryAdd(fieldNode.ObjectClassField);
			}
		}
	}
}
