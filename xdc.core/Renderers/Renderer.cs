using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public interface IRenderer : IDisposable {
		void EnterObject(ObjectNode objectNode);
		void LeaveObject(ObjectNode objectNode);

		void Flush();

		void RenderObjectAs(ObjectContext context, ObjectClass objectClass);
	}

	public abstract class Renderer : IRenderer {
		private int objectCount = 0;

		public bool Report = false;

		public Renderer() {
		}

		public abstract void EnterObject(ObjectNode objectNode);
		public abstract void LeaveObject(ObjectNode objectNode);
		public abstract void Flush();

		public abstract void RenderObjectAs(ObjectContext context, ObjectClass objectClass);

		public virtual int Render(ObjectContext context) {
			foreach(ObjectClass objectClass in Enumerations.Reverse(context.ObjectClass.Bases))
				RenderObjectAs(context, objectClass);
			
			RenderObjectAs(context, context.ObjectClass);

			Flush();

			Render(context.ChildObjects);

			Flush();

			objectCount++;

			if(Report && objectCount % 100 == 0)
				Console.Error.WriteLine("Objects Processed: {0}", objectCount);

			return objectCount;
		}

		public virtual int Render(IEnumerable<ObjectContext> contexts) {
			foreach(ObjectContext context in contexts) {
				EnterObject(context.Node);

				Render(context);

				LeaveObject(context.Node);
			}

			return objectCount;
		}

		public virtual void Dispose() {

		}
	}

	public class MultiRenderer : Renderer {
		private List<IRenderer> renderers = new List<IRenderer>();

		public MultiRenderer(params IRenderer[] _renderers) {
			renderers.AddRange(_renderers);
		}

		public void AddRenderer(IRenderer renderer) {
			renderers.Add(renderer);
		}

		public override void EnterObject(ObjectNode objectNode) {
			foreach(IRenderer renderer in renderers)
				renderer.EnterObject(objectNode);
		}

		public override void LeaveObject(ObjectNode objectNode) {
			foreach(IRenderer renderer in renderers)
				renderer.LeaveObject(objectNode);
		}

		public override void Flush() {
			foreach(IRenderer renderer in renderers)
				renderer.Flush();
		}

		public override void RenderObjectAs(ObjectContext context, ObjectClass objectClass) {
			foreach(IRenderer renderer in renderers)
				renderer.RenderObjectAs(context, objectClass);
		}

		public override void Dispose() {
			//foreach(IRenderer renderer in renderers)
			//	renderer.Dispose();
		}
	}

	public abstract class WritingRenderer : Renderer {
		private IWriter writer;

		public IWriter Writer {
			get { return writer; }
		}

		public WritingRenderer(IWriter _writer) {
			writer = _writer;
		}

		public override void EnterObject(ObjectNode objectNode) {
			Writer.EnterObject(objectNode);
		}

		public override void LeaveObject(ObjectNode objectNode) {
			Writer.LeaveObject(objectNode);
		}
	}
}
