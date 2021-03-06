using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using xdc.common;

namespace xdc.Nodes {
	public abstract class Node {
		#region Family

		private List<Node> children = new List<Node>();

		public ListAccessor<Node> Children {
			get { return new ListAccessor<Node>(children); }
		}

		public IEnumerable<Node> RecursiveChildren {
			get {
				foreach(Node child in Children) {
					yield return child;

					foreach(Node grandChild in child.RecursiveChildren)
						yield return grandChild;
				}
			}
		}

		public bool HasChild(Type type) {
			foreach(Node child in RecursiveChildren)
				if(ReflUtils.Is(child, type))
					return true;

			return false;
		}

		public void AddChild(Node child) {
			if(!IsChildSupported(child))
				throw new ApplicationException(string.Format("Parent does not support child: {0} {1}", GetType().Name, child.GetType().Name));

			children.Add(child);
		}

		public void AddChildren(IEnumerable<Node> _children) {
			foreach(Node child in _children)
				AddChild(child);
		}

		static protected Type[] childTypes = new Type[] { };
		public virtual Type[] ChildTypes {
			get { return childTypes; }
		}

		public bool IsChildSupported(Type type) {
			foreach(Type childType in ChildTypes)
				if(childType.IsAssignableFrom(type))
					return true;

			return false;
		}

		public bool IsChildSupported(Node node) {
			return IsChildSupported(node.GetType());
		}

		private Node parent = null;

		public Node Parent {
			get { return parent; }
		}

		public IEnumerable<Node> Parents {
			get {
				for(Node cur = Parent; cur != null; cur = cur.Parent)
					yield return cur;
			}
		}

		public T GetAncestor<T>(bool inclusive) where T : Node {
			if(inclusive && this is T)
				return this as T;

			foreach(Node cur in Parents)
				if(cur is T)
					return cur as T;

			return null;
		}

		public ObjectNode ParentObject {
			get { return GetAncestor<ObjectNode>(false); }
		}

		public ObjectNode CurrentObject {
			get { return GetAncestor<ObjectNode>(true); }
		}

		public RootNode Root {
			get {
				RootNode root = GetAncestor<RootNode>(true);
				if(root != null)
					return root;

				throw new ApplicationException("Node has no root");
			}
		}

		public virtual int ObjectCount {
			get {
				//zipWith lol
				int ret = 0;
				foreach(Node child in Children)
					ret += child.ObjectCount;
				return ret;
			}
		}

		#endregion

		#region Atts

		protected Atts atts = new Atts();

		public Atts Atts {
			get { return atts; }
		}

		public virtual string Name {
			get { return Atts.TryGetValue("Name"); }
		}

		#endregion

		#region Class Names

		public virtual IEnumerable<string> ClassNames {
			get {
				string name = GetType().Name;
				if(name.EndsWith("Node"))
					name = name.Substring(0, name.Length - 4);

				yield return name;
			}
		}

		public string TopClassName {
			get {
				foreach(string className in ClassNames)
					return className;

				throw new ApplicationException("Node has no top class name");
			}
		}

		public bool IsClass(string className) {
			foreach(string cur in ClassNames)
				if(cur == className)
					return true;

			return false;
		}

		#endregion

		#region Ctor

		public Node(Node _parent, Atts _atts) {
			parent = _parent;
			atts.Add(_atts);

			if(_parent != null) {

			}
			else if(!(this is RootNode))
				throw new ArgumentNullException("_parent");

			if(!string.IsNullOrEmpty(atts["Template"]))
				AddChild(new TemplateNode(this, new Atts("File", atts["Template"])));
		}

		#endregion

		#region ToString

		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			sb.Append(GetType().Name);

			string annotation = ToStringAnnotation();
			if(!string.IsNullOrEmpty(annotation))
				sb.Append(": " + annotation);

			foreach(Node child in Children) {
				sb.AppendLine();
				sb.Append(TextUtils.Indent(child.ToString(), 2));
			}

			return sb.ToString();
		}

		public virtual string ToStringAnnotation() {
			return null;
		}

		#endregion
		
		public virtual Type ContextType {
			get { return typeof(NodeContext); }
		}

		static public Node Create(Type nodeType, Node parentNode, Atts atts) {
			return (Node)Activator.CreateInstance(nodeType, new object[] { parentNode, atts });
		}

		public virtual NodeContext CreateContext(NodeContext parent) {
			try {
				NodeContext ret = (NodeContext)Activator.CreateInstance(ContextType, new object[] { parent, this });

				if(parent != null && parent.Root != null)
					parent.Root.NodeCounts.Inc(this);

				return ret;
			}
			catch(TargetInvocationException tie) {
				throw tie.InnerException ?? tie;
			}
		}

		public virtual NodeValue GetValue(string name) {
			switch(name) {
				case "Name": return new StaticNodeValue(Name);
			}

			string attVal = Atts.TryGetValue(name);
			if(attVal != null)
				return new StaticNodeValue(attVal);

			return null;
		}

		public class NodeException : ApplicationException {
			private Node node;

			public Node Node {
				get { return node; }
			}

			public NodeException(Node _node)
				: base() {
				node = _node;
			}

			public NodeException(Node _node, string message)
				: base(message) {
				node = _node;
			}

			public NodeException(Node _node, string message, Exception innerException)
				: base(message, innerException) {
				node = _node;
			}
		}
	}
}
