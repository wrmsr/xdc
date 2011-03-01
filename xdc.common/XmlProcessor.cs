using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace xdc.common {
	public abstract class XmlProcessor : IDisposable {
		public abstract XmlNode Process(XmlNode node);

		public virtual void Dispose() {
			
		}
	}

	public abstract class XmlStreamProcessor : XmlProcessor {
		//assumes at unopened start element
		//closes on return
		protected abstract void Process(XmlReader xr, XmlWriter xw);

		public override XmlNode Process(XmlNode node) {
			XmlDocument doc = new XmlDocument();

			using(XmlNodeReader xr = new XmlNodeReader(node))
			using(XmlWriter xw = doc.CreateNavigator().AppendChild()) {
				xw.WriteStartDocument();
				Process(xr, xw);
				xw.WriteEndDocument();
			}

			return doc.DocumentElement;
		}
	}
}
