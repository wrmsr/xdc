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
			if(objectNode.ShouldWrite)
				xw.WriteStartElement(objectNode.ObjectClass.Name);
		}

		public void LeaveObject(ObjectNode objectNode) {
			if(objectNode.ShouldWrite)
				xw.WriteEndElement();
		}

		public void WriteField(FieldNode fieldNode, string value) {
			if(fieldNode.ShouldWrite && fieldNode.CurrentObject.ShouldWrite)
				xw.WriteElementString(fieldNode.ObjectClassField.Name, value);
		}

		public void Dispose() {
			if(xw.WriteState != WriteState.Closed && xw.WriteState != WriteState.Error)
				xw.WriteEndElement();			
		}
	}
}
