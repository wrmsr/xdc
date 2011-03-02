using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace xdc.Nodes {
	public class XMLObjectWriter : IObjectWriter {
		private XmlWriter xw;

		public XMLObjectWriter(XmlWriter _xw) {
			xw = _xw;
		}

		public void WriteStart() {
			xw.WriteStartElement("Objects");
		}

		public void WriteEnd() {
			xw.WriteEndElement();
		}

		public void EnterObject(ObjectNode objectNode) {
			if(objectNode.ObjectClass.Atts.GetBool("Write"))
				xw.WriteStartElement(objectNode.ObjectClass.Name);
		}

		public void LeaveObject(ObjectNode objectNode) {
			if(objectNode.ObjectClass.Atts.GetBool("Write"))
				xw.WriteEndElement();
		}

		public void WriteField(FieldNode fieldNode, string value) {
			xw.WriteElementString(fieldNode.ObjectClassField.Name, value);
		}
	}
}
