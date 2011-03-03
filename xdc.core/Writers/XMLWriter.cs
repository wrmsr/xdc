using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace xdc.Nodes {
	public class XMLWriter : IWriter {
		private XmlWriter xw;

		public XMLWriter(XmlWriter _xw) {
			xw = _xw;

			xw.WriteStartElement("Objects");
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
			if(fieldNode.ObjectClassField.Parent.Atts.GetBool("Write") && fieldNode.ObjectClassField.Atts.GetBool("Write"))
				xw.WriteElementString(fieldNode.ObjectClassField.Name, value);
		}

		public void Dispose() {
			if(xw.WriteState != WriteState.Closed && xw.WriteState != WriteState.Error)
				xw.WriteEndElement();			
		}
	}
}
