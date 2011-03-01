using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace xdc.Nodes {
	static public class TemplateCache {
		static private Dictionary<string, XmlNode> templates = new Dictionary<string, XmlNode>();

		static public void Clear() {
			templates.Clear();
		}

		static public XmlNode Get(string file) {
			if(templates.ContainsKey(file))
				return templates[file];

			XmlDocument doc = new XmlDocument();
			doc.Load(file);

			templates[file] = doc.DocumentElement;

			return doc.DocumentElement;
		}
	}

	public class TemplateNode : MetaNode {
		public string File {
			get { return Atts["File"]; }
		}

		public TemplateNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
			if(!Atts.ContainsKey("File"))
				throw new ApplicationException("Template must have File");

			XmlNode template = TemplateCache.Get(File);

			if(template == null)
				throw new ApplicationException("Could not load file: " + File);

			foreach(XmlNode child in template.ChildNodes)
				AddChild(XMLNodeParser.Parse(this, child));
		}
	}
}
