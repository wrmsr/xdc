using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.Nodes {
	public class FileValueShared {
		private Dictionary<string, IFileValues> fileValues = new Dictionary<string, IFileValues>();

		public Dictionary<string, IFileValues> FileValues {
			get { return fileValues; }
		}

		public FileValueShared() {
		}
	}

	public class FileValueContext : TerminalContext {
		private NodeValue val = null;

		public FileValueContext(NodeContext parent, FileValueNode node)
			: base(parent, node) {
			KeyValuePair<string, string> splitName = FileValues.SplitName(((FileValueNode)Node).Value);

			IFileValues fileValues = null;
			if(!Root.GetShared<FileValueShared>().FileValues.TryGetValue(splitName.Key, out fileValues))
				throw new ApplicationException("FileValues not found: " + splitName.Key);

			val = fileValues.Get(splitName.Value) ?? new NullNodeValue();
		}

		public override NodeValue Value {
			get { return val; }
		}
	}

	public class FileValueNode : TerminalNode {
		public override Type ContextType {
			get { return typeof(FileValueContext); }
		}

		public string Value {
			get { return Atts["Value"]; }
		}

		public FileValueNode(Node parent, Dictionary<string, string> atts)
			: base(parent, atts) {
		}

		public override string ToStringAnnotation() {
			return Atts["Value"];
		}
	}
}
