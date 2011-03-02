using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public abstract class Renderer {
		private int objectCount = 0;

		private IObjectWriter writer;

		public IObjectWriter Writer {
			get { return writer; }
		}

		public Renderer(IObjectWriter _writer) {
			writer = _writer;
		}

		public virtual void Flush() {

		}

		public abstract void RenderObjectAs(ObjectContext context, ObjectClass objectClass);

		public void Render(ObjectContext context) {
			Writer.EnterObject(context.Node);

			foreach(ObjectClass objectClass in Enumerations.Reverse(context.ObjectClass.Bases))
				RenderObjectAs(context, objectClass);
			
			RenderObjectAs(context, context.ObjectClass);

			Flush();

			RenderQuiet(context.ChildObjects);

			Flush();

			Writer.LeaveObject(context.Node);

			objectCount++;

			if(objectCount % 100 == 0)
				Console.Error.WriteLine("Objects Processed: {0}", objectCount);
		}

		public void RenderQuiet(IEnumerable<ObjectContext> contexts) {
			foreach(ObjectContext context in contexts)
				Render(context);
		}

		public void Render(IEnumerable<ObjectContext> contexts) {
			RenderQuiet(contexts);
		}
	}
}
