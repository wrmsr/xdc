using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class AnnotatedTextSQLRenderTarget : TextSQLRenderTarget {
		private StringBuilder undosb = new StringBuilder();
		private StringWriter undosw = null;
		private SQLUndoWriter undo = null;

		private int objectCount = 0;

		public AnnotatedTextSQLRenderTarget(TextWriter dst)
			: base(dst) {
			WriteStart();

			undosw = new StringWriter(undosb);
			undo = new SQLUndoWriter(undosw, true);
		}

		public override void Dispose() {
			base.Dispose();

			undo.Dispose();
			undosw.Dispose();

			WriteEnd();

			Flush();
		}

		public override void Flush() {
			base.Flush();

			FlushUndo();
		}

		private void FlushUndo() {
			undosw.Flush();

			using(StringReader sr = new StringReader(undosb.ToString()))
			for(string line; (line = sr.ReadLine()) != null;)
				Emit(string.Format(
					"print '!{0}';" + Environment.NewLine, line));

			undosb.Length = 0;
		}

		private void WriteStart() {
			Emit("begin transaction;" + Environment.NewLine);

			Emit("print '<Objects>';" + Environment.NewLine);
		}

		private void WriteEnd() {
			Emit("print '</Objects>';" + Environment.NewLine);

			Emit("commit transaction;" + Environment.NewLine);
		}

		public override void EnterObject(ObjectNode objectNode) {
			base.EnterObject(objectNode);

			if(++objectCount % 100 == 0)
				Emit(string.Format("print '#{0}';" + Environment.NewLine, objectCount));

			if(objectNode.ObjectClass.Atts.GetBool("Write"))
				Emit(string.Format("print '<{0}>';" + Environment.NewLine, objectNode.ObjectClass.Name));
		}

		public override void LeaveObject(ObjectNode objectNode) {
			base.LeaveObject(objectNode);

			if(objectNode.ObjectClass.Atts.GetBool("Write"))
				Emit(string.Format("print '</{0}>';" + Environment.NewLine, objectNode.ObjectClass.Name));
		}

		public override void WriteFieldSQL(FieldNode fieldNode, string sql) {
			base.WriteFieldSQL(fieldNode, sql);

			Emit(string.Format("print '<{0}>' + {1} + '</{0}>';" + Environment.NewLine, fieldNode.ObjectClassField.Name, sql));

			if(fieldNode.ObjectClassField.Atts.GetBool("IsID")
				&& !string.IsNullOrEmpty(fieldNode.ObjectClassField.Atts["Column"])
				&& !string.IsNullOrEmpty(fieldNode.ObjectClassField.Parent.Atts["Table"])) {
				undo.WriteIDField(fieldNode, string.Format("' + {0} + '", sql));
				FlushUndo();
			}
		}
	}
}
