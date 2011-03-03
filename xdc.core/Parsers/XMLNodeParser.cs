using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using xdc.common;

namespace xdc.Nodes {
	public class XMLNodeParser {
		static private Type GetNodeType(string name, Dictionary<string, string> atts, Node parentNode) {
			if(parentNode == null)
				return typeof(RootNode);

			//Fields before Objects for, among other things, 'Barcode' fields
			//if you want a barcode object, do <Object Class="Barcode"> :|
			if(parentNode != null) {
				ObjectNode currentObject = parentNode.CurrentObject;

				if(currentObject != null) {
					ObjectClassField objectClassField = currentObject.ObjectClass.Fields[name];

					if(objectClassField != null) {
						atts["Name"] = name;

						return typeof(FieldNode);
					}
				}
			}

			//Nodes before ObjectClasses, so no accidental overriding
			Type nodeType = null;
			if(NodeTypes.Dict.TryGetValue(name, out nodeType) && nodeType != null)
				return nodeType;

			ObjectClass objectClass = ObjectClasses.Get(name);

			if(objectClass != null) {
				atts["Class"] = name;

				return typeof(ObjectNode);
			}

			throw new ApplicationException("Unknown node type: " + name);
		}

		static public Node Parse(Node parentNode, XmlReader xr) {
			if(!xr.Read() || !xr.IsStartElement())
				throw new ApplicationException("Invalid xml reader state");

			Dictionary<string, string> atts = XmlUtils.ReadAtts(xr);

			xr.MoveToElement();

			Type nodeType = GetNodeType(xr.Name, atts, parentNode);

			Node node = Node.Create(nodeType, parentNode, atts );

			while(xr.Read()) {
				if(xr.IsStartElement()) {
					using(XmlReader xcr = xr.ReadSubtree())
						node.AddChild(Parse(node, xcr));
				}
				else if(xr.NodeType == XmlNodeType.Text)
					node.AddChildren(TextTerminalParser.Parse(node, xr.ReadString()));
			}

			return node;
		}

		static public RootNode Parse(XmlReader xr) {
			return (RootNode)Parse(null, xr);
		}

		static public Node Parse(Node parentNode, XmlNode node) {
			using(XmlReader xr = new XmlNodeReader(node))
				return Parse(parentNode, xr);
		}

		static public RootNode Parse(XmlNode node) {
			using(XmlReader xr = new XmlNodeReader(node))
				return (RootNode)Parse(null, xr);
		}
	}
}
