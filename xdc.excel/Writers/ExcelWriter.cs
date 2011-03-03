using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class ExcelWriter : IWriter, IDisposable {
		private string fileName = null;

		private ADODB.Connection conn = null;

		private bool tableCreated = false;

		private CounterSet<string> colNameCounts = new CounterSet<string>();

		private Dictionary<FieldNode, string> colNames = new Dictionary<FieldNode, string>();

		private Stack<Dictionary<string, string>> vals = new Stack<Dictionary<string, string>>();

		private StreamWriter sw = null; //new StreamWriter("excel.sql");

		public string FileName {
			get { return fileName; }
		}

		public ExcelWriter(string _fileName) {
			fileName = _fileName;

			File.Delete(fileName);

            string connStr = string.Format(
                "Driver={{Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)}}; DBQ={0}; ReadOnly=False;",
                FileName);

			conn = new ADODB.Connection();
			conn.Open(connStr, null, null, 0);

			if(conn.State != 1)
				throw new ApplicationException("Could not connect to Excel Driver: " + FileName);

			Console.Error.WriteLine("Connected to Excel Driver: " + FileName);
		}

		private void Exec(string txt) {
			//sw.WriteLine(txt);

			object ra;
			conn.Execute(txt, out ra, 0);
		}

		public void EnterObject(ObjectNode objectNode) {
			vals.Push(new Dictionary<string, string>());
		}

		public void LeaveObject(ObjectNode objectNode) {
			List<KeyValuePair<string, string>> objVals = new List<KeyValuePair<string, string>>();

			foreach(Dictionary<string, string> o in vals)
				objVals.AddRange(o);

			vals.Pop();

			if(objVals.Count > 0) {
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
			}
		}

		public void WriteField(FieldNode fieldNode, string value) {
			string colName = null;

			if(!colNames.TryGetValue(fieldNode, out colName)) {
				string fieldName = fieldNode.ObjectClassField.Name;

				colNames[fieldNode] = colName = string.Format("{0}{1}", fieldName, colNameCounts[fieldName]);				

				colNameCounts.Inc(fieldName);

				if(!tableCreated) {
					Exec(string.Format("create table [t] ({0} text);", colName));

					tableCreated = true;
				}
				else
					Exec(string.Format("alter table [t] add column {0} text;", colName));
			}

			vals.Peek()[colName] = value;

			throw new Exception("The method or operation is not implemented.");
		}

		public void Dispose() {
			//sw.Dispose();

			conn.Close();

			Console.Error.WriteLine("Disonnected from Excel Driver: " + FileName);
		}
	}
}
