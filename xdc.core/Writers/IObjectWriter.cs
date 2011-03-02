using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	//There simply must be a more elegant way to do this
	//but I'm really pushing the limits of a static language as it is :|
	//Events? that'll kill inline new a(new b(new c())) ...ness
	public interface IObjectWriter {
		void WriteStart();
		void WriteEnd();

		void EnterObject(ObjectNode objectNode);
		void LeaveObject(ObjectNode objectNode);

		void WriteField(FieldNode fieldNode, string value);
	}

	public class MultiObjectWriter : IObjectWriter {
		private IObjectWriter[] writers;

		public MultiObjectWriter(params IObjectWriter[] _writers) {
			writers = _writers;
		}

		public void WriteStart() {
			foreach(IObjectWriter writer in writers)
				writer.WriteStart();
		}

		public void WriteEnd() {
			foreach(IObjectWriter writer in writers)
				writer.WriteEnd();
		}

		public void EnterObject(ObjectNode objectNode) {
			foreach(IObjectWriter writer in writers)
				writer.EnterObject(objectNode);
		}

		public void LeaveObject(ObjectNode objectNode) {
			foreach(IObjectWriter writer in writers)
				writer.LeaveObject(objectNode);
		}

		public void WriteField(FieldNode fieldNode, string value) {
			foreach(IObjectWriter writer in writers)
				writer.WriteField(fieldNode, value);
		}
	}

	public class ObjectWriterWrapper : IDisposable, IObjectWriter {
		private bool disposed = false;

		private IObjectWriter writer;

		public ObjectWriterWrapper(IObjectWriter _writer) {
			writer = _writer;

			writer.WriteStart();
		}

		public void Dispose() {
			if(disposed)
				throw new ObjectDisposedException("ObjectWriter");

			disposed = true;

			writer.WriteEnd();
		}

		public void WriteStart() {
			if(disposed)
				throw new ObjectDisposedException("ObjectWriter");

			writer.WriteStart();
		}

		public void WriteEnd() {
			if(disposed)
				throw new ObjectDisposedException("ObjectWriter");

			writer.WriteEnd();
		}

		public void EnterObject(ObjectNode objectNode) {
			if(disposed)
				throw new ObjectDisposedException("ObjectWriter");

			writer.EnterObject(objectNode);
		}

		public void LeaveObject(ObjectNode objectNode) {
			if(disposed)
				throw new ObjectDisposedException("ObjectWriter");

			writer.LeaveObject(objectNode);
		}

		public void WriteField(FieldNode fieldNode, string value) {
			if(disposed)
				throw new ObjectDisposedException("ObjectWriter");

			writer.WriteField(fieldNode, value);
		}
	}
}
