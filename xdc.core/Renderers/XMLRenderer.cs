using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using xdc.common;

namespace xdc.Nodes {
	public class XMLRenderer : Renderer {
		private XmlWriter xw;

		public XMLRenderer(XmlWriter _xw) {
			xw = _xw;

			xw.WriteStartElement("Objects");
		}

		public override void Dispose() {
			if(xw.WriteState != WriteState.Closed && xw.WriteState != WriteState.Error)
				xw.WriteEndElement();
		}

		public override void EnterObject(ObjectNode objectNode) {
			xw.WriteStartElement(objectNode.ObjectClass.Name);
		}

		public override void LeaveObject(ObjectNode objectNode) {
			xw.WriteEndElement();
		}

		public override void Flush() {
			xw.Flush();
		}

		public override void RenderObjectAs(ObjectContext context, ObjectClass objectClass) {
			if(context.ObjectClass != objectClass)
				return;

			foreach(FieldContext field in context.Fields) {
				xw.WriteStartElement(field.ObjectClassField.Name);

				foreach(TerminalNodeValue t in field.Value.Terminals)
					xw.WriteElementString(t.GetType().Name, t.Display);

				xw.WriteEndElement();
			}
		}
	}
}
