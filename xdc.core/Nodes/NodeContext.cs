using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	//*NOT* optional - ALL nodes must be present for ref evaluation
	//deliberately no children tracking, strictly to parent
	//deep-copyable
	public class NodeContext {
		private NodeContext parent;

		private Node node;

		public NodeContext Parent {
			get { return parent; }
		}

		public Node Node {
			get { return node; }
		}

		public T GetAncestor<T>(bool inclusive) where T : NodeContext {
			for(NodeContext cur = inclusive ? this : Parent; cur != null; cur = cur.Parent)
				if(cur is T)
					return cur as T;

			return null;
		}

		public ObjectContext ParentObject {
			get { return GetAncestor<ObjectContext>(false); }
		}

		public ObjectContext CurrentObject {
			get { return GetAncestor<ObjectContext>(true); }
		}

		public RootContext Root {
			get {
				RootContext root = GetAncestor<RootContext>(true);
				if(root != null)
					return root;

				throw new ApplicationException("Node context has no root");
			}
		}

		public virtual IEnumerable<WeakNodeContext> Children {
			get {
				foreach(Node childNode in Node.Children)
					yield return new WeakNodeContext(this, childNode);
			}
		}

		public virtual IEnumerable<NodeContext> RecursiveChildren {
			get {
				foreach(WeakNodeContext child in Children) {
					NodeContext childContext = child.Context;

					yield return childContext;

					//wtb tail recursion
					foreach(NodeContext grandChild in childContext.RecursiveChildren)
						yield return grandChild;
				}
			}
		}

		public virtual IEnumerable<NodeContext> All {
			get {
				yield return this;

				foreach(NodeContext child in RecursiveChildren)
					yield return child;
			}
		}

		public IEnumerable<WeakNodeContext> FindDescendents(params Type[] types) {
			foreach(WeakNodeContext child in Children)
				if(Enumerations.Find(types, delegate(Type t) {
					return ReflUtils.Is(child.Node, t);
				}) != null)
					yield return child;
				else
					foreach(WeakNodeContext grandChild in child.Context.FindDescendents(types))
						yield return grandChild;
		}

		public NodeContext(NodeContext _parent, Node _node) {
			parent = _parent;
			node = _node;
		}

		public virtual NodeValue GetValue(string packed) {
			NodeValue ret = null;

			foreach(Node valueNode in TextTerminalParser.Parse(Node, packed)) {
				NodeContext nc = valueNode.CreateContext(this);
				foreach(TerminalContext tnc in Enumerations.As<TerminalContext>(nc.All))
					ret = NodeValue.Concat(ret, tnc.Value);
			}

			return ret;
		}

		public virtual string GetStr(string packed) {
			NodeValue val = GetValue(packed);

			if(val == null || val is NullNodeValue)
				return null;
			else if(val is StaticNodeValue)
				return ((StaticNodeValue)val).Value;
			else
				throw new ApplicationException("Invalid node value for GetStr");
		}

		/*
		field
		class:field
		name.field
		class/class:field
		*path = take any
		-path = skip instance
		>path = skip object
		^path = skip level
		`path = skip field, ignore name
		*/
		public virtual NodeValue GetRefValue(string path) {
			string[] parts = null;

			if(path.StartsWith("^")) {
				if(Parent == null)
					return null;

				return Parent.GetRefValue(path.Substring(1));
			}
			else if(path.StartsWith(">")) {
				if(Parent == null)
					return null;

				return Parent.GetRefValue(Node is ObjectNode ? path.Substring(1) : path);
			}
			else if((parts = path.Split('/')).Length > 1) {
				if(Parent == null)
					return null;

				return Parent.GetRefValue(Node.IsClass(parts[0]) ? path.Substring(parts[0].Length + 1) : path);
			}

			NodeValue value = null;
			bool named = false;

			string target = path.Trim('-', '*');

			if(named = (parts = target.Split('.')).Length > 1)
				target = Node.Name == parts[0] ? parts[1] : null;
			else if(named = (parts = target.Split(':')).Length > 1)
				target = Node.IsClass(parts[0]) ? target : null;

			if(!string.IsNullOrEmpty(target))
				value = GetSingleValue(target);

			if(Parent != null) {
				if(value == null) {
					if(named || path.StartsWith("*") || !(Node is ObjectNode))
						return Parent.GetRefValue(path);
				}
				else if(path.StartsWith("-"))
					return Parent.GetRefValue(path.Substring(1));
			}

			return value;
		}

		static public string StripName(string name) {
			string[] ret = name.Split('.', ':');
			return ret[ret.Length - 1];
		}

		protected Dictionary<string, NodeValue> namedValues = new Dictionary<string, NodeValue>();

		public DictionaryAccessor<string, NodeValue> NamedValues {
			get { return new DictionaryAccessor<string, NodeValue>(namedValues); }
		}

		public virtual NodeValue GetSingleValue(string name) {
			return NamedValues.TryGetValue(name) ?? Node.GetValue(name);
		}
	}

	public class NodeContext<T> : NodeContext where T : Node {
		public new T Node {
			get { return (T)base.Node; }
		}

		public NodeContext(NodeContext parent, T node)
			: base(parent, node) {
		}
	}

	public class WeakNodeContext {
		private NodeContext parent;

		private Node node;

		public NodeContext Parent {
			get { return parent; }
		}

		public Node Node {
			get { return node; }
		}

		public NodeContext Context {
			get { return Node.CreateContext(Parent); }
		}

		public WeakNodeContext(NodeContext _parent, Node _node) {
			parent = _parent;
			node = _node;
		}
	}

	public class WeakNodeContext<C, N, CN> : WeakNodeContext where C : NodeContext where N : Node where CN : NodeContext {
		public new C Parent {
			get { return (C)base.Parent; }
		}

		public new N Node {
			get { return (N)base.Node; }
		}

		public new CN Context {
			get { return (CN)base.Context; }
		}

		public WeakNodeContext(C parent, N node)
			: base (parent, node) {
		}
	}
}
