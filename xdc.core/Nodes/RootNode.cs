using System;
using System.Collections.Generic;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public class RootContext : NodeContext {
		private Dictionary<Type, object> shared = new Dictionary<Type, object>();

		/*
		private isDummyRun = false;
		public bool IsDummyRun {
			get { return isDummyRun; }
		}
		*/

		public DateTime Now = DateTime.Now;

		public Random Rand = new Random();

		public object GetShared(Type sharedType) {
			if(sharedType == null)
				return null;

			object value;

			if(!shared.TryGetValue(sharedType, out value))
				shared.Add(sharedType, value = Activator.CreateInstance(sharedType));

			return value;
		}

		public T GetShared<T>() {
			return (T)GetShared(typeof(T));
		}

		public RootContext(NodeContext parent, RootNode node)
			: base(parent, node) {
			if(parent != null)
				throw new ArgumentOutOfRangeException("parent", "RootNode context may not have parent");
		}

		public IEnumerable<ObjectContext> Objects {
			get {
				foreach(WeakNodeContext context in FindDescendents(typeof(ObjectNode)))
					yield return (ObjectContext)context.Context;
			}				
		}
	}

	public class RootNode : Node {
		public override Type ContextType {
			get { return typeof(RootContext); }
		}

		public RootContext CreateContext() {
			return (RootContext)CreateContext(null);
		}

		static protected new Type[] childTypes = new Type[] { typeof(MetaNode), typeof(ObjectNode) };
		public override Type[] ChildTypes {
			get { return childTypes; }
		}

		public RootNode()
			: base(null, new Dictionary<string, string>()) {
		}

		public RootNode(Dictionary<string, string> atts)
			: base(null, atts) {
		}

		public RootNode(Node _parent, Dictionary<string, string> atts)
			: base(null, atts) {
			if(_parent != null)
				throw new ArgumentOutOfRangeException("_parent", "RootNode may not have parent");
		}
	}
}
