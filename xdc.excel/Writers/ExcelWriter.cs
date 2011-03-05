using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class ExcelWriter : IWriter, IDisposable {
		private object sync = new object();

		private string fileName = null;

		//http://stackoverflow.com/questions/158706/how-to-properly-clean-up-excel-interop-objects-in-c
		private Microsoft.Office.Interop.Excel.Application app;
		private Microsoft.Office.Interop.Excel.Workbooks wbs;
		private Microsoft.Office.Interop.Excel.Workbook wb;
		private Microsoft.Office.Interop.Excel.Sheets wss;
		private Microsoft.Office.Interop.Excel.Worksheet ws;

		private CounterSet<string> colNameCounts = new CounterSet<string>();

		private Dictionary<FieldNode, int> colIdxs = new Dictionary<FieldNode, int>();

		private Stack<Dictionary<int, string>> vals = new Stack<Dictionary<int, string>>();

		public string FilePath {
			get {
				FileInfo fi = new FileInfo(fileName);
				
				return fi.FullName;
			}
		}

		public ExcelWriter(string _fileName) {
			fileName = _fileName;

			if(File.Exists(FilePath))
				throw new ApplicationException("File already exists: " + FilePath);

			File.Create(FilePath);

			app = new Microsoft.Office.Interop.Excel.Application();

			Console.Error.WriteLine("Connected to Excel");

			wbs = app.Workbooks;

			wb = wbs.Add(1);

			wb.Activate();

			wss = wb.Sheets;

			ws = (Microsoft.Office.Interop.Excel.Worksheet)wss.get_Item(1);

			Console.Error.WriteLine("Excel Worksheet Initialized");
		}

		public void EnterObject(ObjectNode objectNode) {
			vals.Push(new Dictionary<int, string>());
		}

		public void LeaveObject(ObjectNode objectNode) {
			List<KeyValuePair<int, string>> objVals = new List<KeyValuePair<int, string>>();

			foreach(Dictionary<int, string> o in vals)
				objVals.AddRange(o);

			vals.Pop();

			if(objVals.Count > 0) {
				lock(sync) {
					/*
					StringBuilder sb = new StringBuilder();

					sb.Append("insert into [t$] (");

					int i = 0;
					foreach(KeyValuePair<string, string> val in objVals) {
						if(i++ > 0)
							sb.Append(", ");

						sb.Append(val.Key);
					}

					sb.Append(") values (");

					i = 0;
					foreach(KeyValuePair<string, string> val in objVals) {
						if(i++ > 0)
							sb.Append(", ");

						sb.AppendFormat("'{0}'", val.Value);
					}

					sb.Append(");");

					Exec(sb.ToString());
					*/
				}
			}
		}

		public void WriteField(FieldNode fieldNode, string value) {
			int colIdx = 0;

			if(!colIdxs.TryGetValue(fieldNode, out colIdx)) {
				string fieldName = fieldNode.ObjectClassField.Name;

				string colName = string.Format("{0}{1}", fieldName, colNameCounts[fieldName]);
				
				//ws.Columns.get_Item(.... Name = colName

				colNameCounts.Inc(fieldName);
			}

			vals.Peek()[colIdx] = value;

			throw new Exception("The method or operation is not implemented.");
		}

		private void Release(object o) {
			while(Marshal.FinalReleaseComObject(o) != 0) { }
		}

		private void CleanUp() {
			lock(sync) {
				if(wb != null) {
					wb.Close(Type.Missing, Type.Missing, Type.Missing);
					Release(wb);
					wb = null;
				}

				if(wbs != null) { Release(wbs); wbs = null; }
				if(ws != null) { Release(ws); ws = null; }
				if(wss != null) { Release(wss); wss = null; }

				if(app != null) {
					app.Quit();
					Release(app);
					app = null;
				}
			}
		}

		~ExcelWriter() {
			Console.Error.WriteLine("ExcelWriter Finalizing");

			CleanUp();

			Console.Error.WriteLine("ExcelWriter Finalized");
		}

		public void Dispose() {
			Console.Error.WriteLine("Saving Excel Worksheet: " + FilePath);

			File.Delete(FilePath);

			lock(sync)
				wb.SaveAs(FilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
					Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
					Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

			Console.Error.WriteLine("Disonnecting from Excel");

			//http://support.microsoft.com/kb/306022/en-us

			GC.Collect();
			GC.WaitForPendingFinalizers();

			CleanUp();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Console.Error.WriteLine("Disonnected from Excel");
		}
	}
}
