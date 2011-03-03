using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using xdc.common;

namespace xdc.Nodes {
	static public class TemplateCache {
		static public List<string> Paths = new List<string>();

		static private Dictionary<string, XmlNode> templates = new Dictionary<string, XmlNode>();

		static public void Clear() {
			templates.Clear();
		}

		static public string FindFile(string file) {
			if(File.Exists(file))
				return file;

			foreach(string path in Paths) {
				string pathedFile = Path.Combine(path, file);

				if(File.Exists(pathedFile))
					return pathedFile;
			}

			throw new FileNotFoundException(file);
		}

		static public XmlNode Get(string file) {
			if(templates.ContainsKey(file))
				return templates[file];

			XmlDocument doc = new XmlDocument();
			doc.Load(FindFile(file));

			templates[file] = doc.DocumentElement;

			return doc.DocumentElement;
		}
	}

	public class TemplateNode : MetaNode {
		public string File {
			get { return Atts["File"]; }
		}

		public TemplateNode(Node parent, Atts atts)
			: base(parent, atts) {
			if(!Atts.ContainsKey("File"))
				throw new ApplicationException("Template must have File");

			XmlNode template = TemplateCache.Get(File);

			if(template == null)
				throw new ApplicationException("Could not load file: " + File);

			foreach(XmlNode child in template.ChildNodes)
				if(child.NodeType == XmlNodeType.Element)
					AddChild(XMLNodeParser.Parse(this, child));
		}
	}
}
