using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	//There simply must be a more elegant way to do this
	//but I'm really pushing the limits of a static language as it is :|
	//Events? that'll kill inline new a(new b(new c())) ...ness
	public interface IWriter : IDisposable {
		void EnterObject(ObjectNode objectNode);
		void LeaveObject(ObjectNode objectNode);

		void WriteField(FieldNode fieldNode, string value);
	}

	public class MultiWriter : IWriter {
		private List<IWriter> writers = new List<IWriter>();

		public MultiWriter(params IWriter[] _writers) {
			writers.AddRange(_writers);
		}

		public void AddWriter(IWriter writer) {
			writers.Add(writer);
		}

		public void EnterObject(ObjectNode objectNode) {
			foreach(IWriter writer in writers)
				writer.EnterObject(objectNode);
		}

		public void LeaveObject(ObjectNode objectNode) {
			foreach(IWriter writer in writers)
				writer.LeaveObject(objectNode);
		}

		public void WriteField(FieldNode fieldNode, string value) {
			foreach(IWriter writer in writers)
				writer.WriteField(fieldNode, value);
		}

		public void Dispose() {
			//foreach(IWriter writer in writers)
			//	writer.Dispose();
		}
	}
}
