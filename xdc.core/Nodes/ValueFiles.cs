using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace xdc.Nodes {
	public enum ValueFileMode {
		Die = 0,
		End = 1,
		Wrap = 2,
		Random = 3
	}

	public interface IFileValues {
		string Type { get; }
		ValueFileMode Mode { get; set; }
		void Load(TextReader text);
		NodeValue Get(string name);
	}

	public class FileValues {
		static public IFileValues Create(string type) {
			if(type == "Line")
				return new LineFileValues();
			else if(type == "CSV")
				return new CSVFileValues();
			else if(type == "XML")
				return new XMLFileValues();
			else if(type.StartsWith("Set:"))
				return new SetFileValues(type);
			else
				throw new ApplicationException("Uknown FileValues type: " + type);
		}

		static public KeyValuePair<string, string> SplitName(string name) {
			int pos = name.IndexOf(':');

			if(pos >= 0)
				return new KeyValuePair<string, string>(
					name.Substring(0, pos),
					name.Substring(pos + 1));
			else
				return new KeyValuePair<string, string>(
					name,
					string.Empty);
		}
	}

	public abstract class FileValues<T> : IFileValues where T : class {
		private List<T> values = new List<T>();

		private int last = -1;

		private ValueFileMode mode = ValueFileMode.Die;

		public int Last {
			get { return last; }
		}

		public int Count {
			get { return values.Count; }
		}

		public abstract string Type { get; }

		public ValueFileMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		protected void Add(T val) {
			values.Add(val);
		}

		protected T Select() {
			if(mode == ValueFileMode.Die) {
				if(++last >= values.Count)
					throw new Exception("Out of values");
			}
			else if(mode == ValueFileMode.End) {
				if(++last >= values.Count)
					return null;
			}
			else if(mode == ValueFileMode.Wrap) {
				if(++last >= values.Count)
					last = 0;
			}
			else if(mode == ValueFileMode.Random) {
				last = (new Random()).Next(0, values.Count);
			}
			else
				throw new Exception("Unknown mode");

			return values[last];
		}

		public NodeValue Get(string name) {
			T value = Select();

			return value == null ? null : Get(value, name);
		}

		public abstract void Load(TextReader text);

		protected abstract NodeValue Get(T value, string name);
	}

	public class LineFileValues : FileValues<string> {
		public override string Type { get { return "Line"; } }

		public override void Load(TextReader text) {
			for(string line = null; (line = text.ReadLine()) != null; )
				if(!string.IsNullOrEmpty(line))
					Add(line);
		}

		protected override NodeValue Get(string value, string name) {
			return new StaticNodeValue(value);
		}
	}

	public class CSVFileValues : FileValues<string[]> {
		public override string Type { get { return "CSV"; } }

		private Dictionary<string, int> cols = new Dictionary<string, int>();

		public override void Load(TextReader text) {
			if(cols.Count > 0)
				throw new Exception("Columns already loaded");

			for(string line = null; (line = text.ReadLine()) != null; )
				Add(line.Split(','));
		}

		protected override NodeValue Get(string[] value, string name) {
			return null;
		}
	}

	public class XMLFileValues : FileValues<XmlNode> {
		public override string Type { get { return "XML"; } }

		public override void Load(TextReader text) {
			XmlDocument doc = new XmlDocument();
			doc.Load(text);

			foreach(XmlNode cur in doc.DocumentElement.ChildNodes)
				Add(cur);
		}
		
		protected override NodeValue Get(XmlNode value, string name) {
			/*
			KeyValuePair<string, string> splitName = FileValues.SplitName(name);

			if((value = value.SelectSingleNode(splitName.Key)) == null)
				return;

			using(XmlReader xr = new XmlNodeReader(value)) {
				xr.ReadStartElement();
				while(xr.Depth > 0) {
					XmlUtils.WriteShallowNode(xr, xw);
					xr.Read();
				}
				xr.ReadEndElement();
			}
			*/

			return null;
		}
	}

	public class SetFileValues : IFileValues {
		public string Type { get { return "Set:" + childType; } }

		private string childType = string.Empty;

		private Dictionary<string, IFileValues> fileValues = new Dictionary<string, IFileValues>();

		private ValueFileMode mode;

		public ValueFileMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		public SetFileValues(string _type) {
			childType = FileValues.SplitName(_type).Value;
		}

		public void Load(TextReader text) {
			XmlDocument doc = new XmlDocument();
			doc.Load(text);

			foreach(XmlNode cur in doc.DocumentElement.SelectNodes("Item")) {
				string name = cur.Attributes["Name"].Value;
				IFileValues fv = FileValues.Create(childType);

				using(StringReader sr = new StringReader(cur.InnerText))
					fv.Load(sr);

				fileValues[name] = fv;
			}
		}

		public NodeValue Get(string name) {
			KeyValuePair<string, string> splitName = FileValues.SplitName(name);
			IFileValues fv = fileValues[splitName.Key];
			fv.Mode = Mode;
			return fv.Get(splitName.Value);
		}
	}
}
