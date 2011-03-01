/*
ObjectClass Name
 ObjectField Name Type

Object Class
 Field Name  

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace xdc.common {
	public class ObjectClassField : XmlNodeHolder {
		private ObjectClass parent;

		public ObjectClass Parent {
			get { return parent; }
		}

		/*protected*/ public string Name {
			get { return Atts["Name"]; }
		}

		public string FullName {
			get { return string.Format("{0}:{1}", Parent.Name, Name); }
		}

		public bool IsNamed(string name) {
			return Name == name || FullName == name;
		}
		
		public ObjectClassField(ObjectClass _parent, XmlNode _xml)
			: base(_xml) {
			parent = _parent;
		}
	}

	public class ObjectClass : XmlNodeHolder {
		private List<ObjectClassField> localFields = new List<ObjectClassField>();

		private List<ObjectClass> localBases = new List<ObjectClass>();

		public string Name {
			get { return Atts["Name"]; }
		}

		public class LocalFieldsAccessor : ListAccessor<ObjectClassField> {
			public LocalFieldsAccessor(List<ObjectClassField> lst)
				: base(lst) {
			}

			public ObjectClassField this[string name] {
				get { return Find(delegate(ObjectClassField fld) { return fld.IsNamed(name); }); }
			}
		}

		public LocalFieldsAccessor LocalFields {
			get { return new LocalFieldsAccessor(localFields); }
		}

		public class FieldsAccessor : FriendlyEnumerable<ObjectClassField> {
			ObjectClass objectClass;

			public FieldsAccessor(ObjectClass _objectClass) {
				objectClass = _objectClass;
			}

			public ObjectClassField this[string name] {
				get { return Find(delegate(ObjectClassField fld) { return fld.IsNamed(name); }); }
			}

			public override IEnumerator<ObjectClassField> GetEnumerator() {
				foreach(ObjectClassField fld in objectClass.LocalFields)
					yield return fld;

				foreach(ObjectClass b in objectClass.Bases)
					foreach(ObjectClassField fld in b.Fields)
						yield return fld;
			}
		}

		public FieldsAccessor Fields {
			get { return new FieldsAccessor(this); }
		}

		//Local Only
		public ObjectClassField LocalID {
			get {
				return localFields.Find(delegate(ObjectClassField fld) {
					return (fld.Atts["IsID"] ?? string.Empty).ToLower() == "true"; });
			}
		}

		public class LocalBasesAccessor : ListAccessor<ObjectClass> {
			public LocalBasesAccessor(List<ObjectClass> lst)
				: base(lst) {
			}

			public ObjectClass this[string name] {
				get { return Find(delegate(ObjectClass b) { return b.Name == name; }); }
			}
		}

		public LocalBasesAccessor LocalBases {
			get { return new LocalBasesAccessor(localBases); }
		}

		public class BasesAccessor : FriendlyEnumerable<ObjectClass> {
			ObjectClass objectClass;

			public BasesAccessor(ObjectClass _objectClass) {
				objectClass = _objectClass;
			}

			public ObjectClass this[string name] {
				get { return Find(delegate(ObjectClass b) { return b.Name == name; }); }
			}

			public override IEnumerator<ObjectClass> GetEnumerator() {
				foreach(ObjectClass b in objectClass.LocalBases) {
					yield return b;

					foreach(ObjectClass sb in b.Bases)
						yield return sb;
				}
			}
		}

		public BasesAccessor Bases {
			get { return new BasesAccessor(this); }
		}

		//Concat of Name : BaseNames, helps other funcs
		public IEnumerable<string> ClassNames {
			get {
				yield return Name;

				foreach(ObjectClass curBase in Bases)
					foreach(string curName in curBase.ClassNames)
						yield return curName;
			}
		}

		private void LoadFromNode(XmlNode node) {
			foreach(XmlNode n in node.ChildNodes) {
				switch(n.Name) {
					case "ObjectClassField":
						localFields.Add(new ObjectClassField(this, n));
						break;

					case "BaseObjectClass":
						ObjectClass b = ObjectClasses.Get(n.Attributes["Name"].Value);
						if(b != null)
							localBases.Add(b);
						break;

					case "Template":
						LoadFromNode(ObjectClasses.GetTemplate(n.Attributes["Name"].Value));
						break;
				}
			}
		}

		public ObjectClass(XmlNode _xml)
			: base(_xml) {
			LoadFromNode(Xml);
		}
	}

	static public class ObjectClasses {
		static private XmlNode xml = null;

		static private Dictionary<string, ObjectClass> objectClasss = new Dictionary<string, ObjectClass>();

		static private Dictionary<string, XmlNode> objectClassTemplates = new Dictionary<string, XmlNode>();

		static public ObjectClass Get(string name) {
			ObjectClass ret = null;

			if(objectClasss.TryGetValue(name, out ret))
				return ret;

			return null;
		}

		static public XmlNode GetTemplate(string name) {
			XmlNode ret = null;

			if(objectClassTemplates.TryGetValue(name, out ret))
				return ret;

			return null;
		}

		static public void Load() {
			objectClasss.Clear();

			XmlDocument doc = new XmlDocument();
			doc.Load("ObjectClasses.xml");
			xml = doc.SelectSingleNode("ObjectClasses");

			foreach(XmlNode n in xml.ChildNodes) {
				switch(n.Name) {
					case "ObjectClass":
						ObjectClass c = new ObjectClass(n);
						objectClasss.Add(c.Name, c);
						break;

					case "ObjectClassTemplate":
						objectClassTemplates.Add(n.Attributes["Name"].Value, n);
						break;
				}
			}
		}
	}
}
