using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace xdc.common {
	public class XmlNodeHolder {
		private XmlNode xml;

		public XmlNode Xml {
			get { return xml; }
		}

		public class AttributeAccessor {
			private XmlNode xml;

			public AttributeAccessor(XmlNode _xml) {
				xml = _xml;
			}

			public string this[string name] {
				get {
					XmlAttribute att = xml.Attributes[name];
					if(att == null)
						return null;
					return att.Value;
				}
				set {
					XmlAttribute att = xml.Attributes[name];
					if(att == null)
						att = xml.Attributes.Append(xml.OwnerDocument.CreateAttribute(name));
					att.Value = value;
				}
			}

			public bool GetBool(string name) {
				return (this[name] ?? "false").ToLower() == "true";
			}
		}

		public AttributeAccessor Atts {
			get { return new AttributeAccessor(xml); }
		}

		public class StringValueAccessor {
			private XmlNode xml;

			public StringValueAccessor(XmlNode _xml) {
				xml = _xml;
			}

			public string this[string name] {
				get {
					XmlNode node = xml.SelectSingleNode(name);
					if(node == null)
						return null;
					return node.InnerText;
				}
				set {
					XmlNode node = xml.SelectSingleNode(name);
					if(node == null)
						node = node.AppendChild(xml.OwnerDocument.CreateElement(name));
					node.InnerText = value;
				}
			}
		}

		public StringValueAccessor Vals {
			get { return new StringValueAccessor(xml); }
		}

		public XmlNodeHolder(XmlNode _xml) {
			xml = _xml;
		}
	}

	static public class XmlUtils {
		static public Dictionary<string, string> ReadAtts(XmlReader xr) {
			Dictionary<string, string> atts = new Dictionary<string, string>();
			
			while(xr.MoveToNextAttribute())
				atts.Add(xr.Name, xr.Value);
			
			return atts;
		}

		static public Dictionary<string, string> ReadAtts(XmlNode node) {
			Dictionary<string, string> atts = new Dictionary<string, string>();

			foreach(XmlAttribute att in node.Attributes)
				atts.Add(att.Name, att.Value);

			return atts;
		}

		static public void WriteAtts(XmlWriter xw, Dictionary<string, string> atts) {
			foreach(KeyValuePair<string, string> att in atts)
				xw.WriteAttributeString(att.Key, att.Value);
		}

		static public void WriteAtts(XmlNode node, Dictionary<string, string> atts) {
			foreach(KeyValuePair<string, string> att in atts)
				node.Attributes.Append(node.OwnerDocument.CreateAttribute(att.Key)).Value = att.Value;
		}

		static public string ReadAtt(XmlNode node, string name) {
			XmlAttribute att = node.Attributes[name];
			
			return att != null ? att.Value : null;
		}

		static public void CopyAtts(XmlNode src, XmlNode dst, bool overwrite) {
			foreach(XmlAttribute srca in src.Attributes) {
				XmlAttribute dsta = dst.Attributes[srca.Name];
				if(dsta != null) {
					if(overwrite)
						dst.Attributes.Remove(dsta);
					else
						continue;
				}
				dsta = dst.OwnerDocument.CreateAttribute(srca.Name);
				dsta.Value = srca.Value;
				dst.Attributes.Append(dsta);
			}
		}

		static public void WriteShallowNode(XmlReader reader, XmlWriter writer) {
			//Console.WriteLine("{0}: {1}", reader.NodeType, reader.Name);

			switch(reader.NodeType) {
				case XmlNodeType.Element:
					writer.WriteStartElement(reader.Prefix	, reader.LocalName, reader.NamespaceURI);
					writer.WriteAttributes(reader, true);
					if(reader.IsEmptyElement)
						writer.WriteEndElement();
					break;
				case XmlNodeType.Text:
					writer.WriteString(reader.Value);
					break;
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					writer.WriteWhitespace(reader.Value);
					break;
				case XmlNodeType.CDATA:
					writer.WriteCData(reader.Value);
					break;
				case XmlNodeType.EntityReference:
					writer.WriteEntityRef(reader.Name);
					break;
				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction(reader.Name, reader.Value);
					break;
				case XmlNodeType.DocumentType:
					writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
					break;
				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;
				case XmlNodeType.EndElement:
					writer.WriteFullEndElement();
					break;
			}
		}

		static public XmlAttribute AddAtt(XmlNode node, string name, string value) {
			XmlAttribute att = node.Attributes.Append(node.OwnerDocument.CreateAttribute(name));
			att.Value = value;
			return att;
		}
		
		static public XmlNode MakeElement(XmlDocument doc, string name, string[] atts, string value) {
			XmlNode node = doc.CreateElement(name);

			if(atts != null)
				for(int i = 1; i < atts.Length; i += 2)
					//name and value may not be null, name may not be empty
					if(!string.IsNullOrEmpty(atts[i - 1]) && atts[i] != null)
						AddAtt(node, atts[i - 1], atts[i]);

			if(value != null)
				node.InnerText = value;

			return node;
		}
		
		static public XmlNode MakeElement(XmlDocument doc, string name, string[] atts) {
			return MakeElement(doc, name, atts, null);
		}

		static public XmlNode WriteElement(XmlNode parent, string name, string[] atts, string value) {
			return parent.AppendChild(MakeElement(parent.OwnerDocument, name, atts, value));
		}

		static public void WriteElement(XmlNode parent, string name, string[] atts) {
			WriteElement(parent, name, atts, null);
		}





		static public XmlNode MakeElement(XmlDocument doc, string name, Dictionary<string, string> atts, string value) {
			XmlNode node = doc.CreateElement(name);

			if(atts != null)
				foreach(KeyValuePair<string, string> att in atts)
					AddAtt(node, att.Key, att.Value);

			if(value != null)
				node.InnerText = value;

			return node;
		}

		static public XmlNode MakeElement(XmlDocument doc, string name, Dictionary<string, string> atts) {
			return MakeElement(doc, name, atts, null);
		}

		static public XmlNode WriteElement(XmlNode parent, string name, Dictionary<string, string> atts, string value) {
			return parent.AppendChild(MakeElement(parent.OwnerDocument, name, atts, value));
		}

		static public void WriteElement(XmlNode parent, string name, Dictionary<string, string> atts) {
			WriteElement(parent, name, atts, null);
		}


		
		static public void WriteElement(XmlWriter xw, string name, string[] atts, string value) {
			xw.WriteStartElement(name);

			if(atts != null)
				for(int i = 1; i < atts.Length; i += 2)
					//name and value may not be null, name may not be empty
					if(!string.IsNullOrEmpty(atts[i - 1]) && atts[i] != null)
						xw.WriteAttributeString(atts[i - 1], atts[i]);

			xw.WriteString(value);

			xw.WriteEndElement();
		}

		static public void WriteElement(XmlWriter xw, string name, Dictionary<string, string> atts, string value) {
			xw.WriteStartElement(name);

			if(atts != null)
				foreach(KeyValuePair<string, string> att in atts)
					xw.WriteAttributeString(att.Key, att.Value);

			xw.WriteString(value);

			xw.WriteEndElement();
		}

		static public void WriteChildren(XmlWriter xw, XmlReader xr) {
			int orgDepth = xr.Depth;

			while(xr.Depth >= orgDepth) {
				XmlUtils.WriteShallowNode(xr, xw);
				
				xr.Read();
			}
		}

		static public void WriteElement(XmlWriter xw, string name, Dictionary<string, string> atts, XmlReader xr) {
			xw.WriteStartElement(name);

			if(atts != null)
				foreach(KeyValuePair<string, string> att in atts)
					xw.WriteAttributeString(att.Key, att.Value);

			WriteChildren(xw, xr);

			xw.WriteEndElement();
		}

		static public void WriteElement(XmlWriter xw, string name, string value) {
			WriteElement(xw, name, (string[])null, value);
		}

		static public void WriteElement(XmlWriter xw, string name, string[] atts) {
			WriteElement(xw, name, atts, null);
		}

		static public void WriteElement(XmlWriter xw, string name) {
			WriteElement(xw, name, (string[])null, null);
		}

		static public void WriteElement(XmlWriter xw, string name, Dictionary<string, string> atts) {
			WriteElement(xw, name, atts, (string)null);
		}
	}
}
