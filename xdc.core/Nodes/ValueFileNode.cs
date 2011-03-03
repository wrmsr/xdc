using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using xdc.common;

namespace xdc.Nodes {
	public class ValueFileContext : NodeContext {
		public ValueFileContext(NodeContext parent, ValueFileNode node)
			: base(parent, node) {
			string type = Node.Atts.ContainsKey("Type") ? Node.Atts["Type"] : "Line";
			ValueFileMode mode = (ValueFileMode)Enum.Parse(typeof(ValueFileMode), Node.Atts.ContainsKey("Mode") ? Node.Atts["Mode"] : "Die");

			IFileValues fileValues = null;

			if(Root.GetShared<FileValueShared>().FileValues.TryGetValue(Node.Name, out fileValues)) {
				if(fileValues.Type != type)
					throw new ApplicationException("ValueFile aready exists but is of different type: " + type);
			}
			else {
				fileValues = FileValues.Create(type);
				Root.GetShared<FileValueShared>().FileValues.Add(Node.Name, fileValues);
			}

			fileValues.Mode = mode;

			string file = GetStr(Node.Atts["File"]);

			using(StreamReader sr = new StreamReader(file))
				fileValues.Load(sr);
		}
	}

	public class ValueFileNode : MetaNode {
		public override Type ContextType {
			get { return typeof(ValueFileContext); }
		}

		public ValueFileNode(Node parent, Atts atts)
			: base(parent, atts) {
			if(!atts.ContainsKey("Name"))
				throw new ApplicationException("Name required");
		}
	}
}
