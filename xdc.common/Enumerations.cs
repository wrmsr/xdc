using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	static public class Enumerations {
		static public IEnumerable<T> None<T>() {
			yield break;
		}

		static public IEnumerable<T> One<T>(T v) {
			yield return v;
		}

		static public IEnumerable<T> From<T>(params T[] vs) {
			foreach(T v in vs)
				yield return v;
		}

		static public IEnumerable<T> Combine<T>(params IEnumerable<T>[] es) {
			foreach(IEnumerable<T> e in es)
				foreach(T v in e)
					yield return v;
		}

		static public IEnumerable<T> Skip<T>(IEnumerable<T> vs, int c) {
			foreach(T v in vs)
				if(c > 0)
					c--;
				else
					yield return v;
		}

		static public IEnumerable<X> ChangeType<T, X>(IEnumerable<T> e) {
			foreach(T v in e)
				yield return (X)Convert.ChangeType(v, typeof(X));
		}

		static public IEnumerable<X> As<T, X>(IEnumerable<T> e) where X : class {
			foreach(T v in e) {
				X x = v as X;
				if(x != null)
					yield return x;
			}
		}

		static public IEnumerable<X> ChangeType<X>(IEnumerable e) {
			foreach(object v in e)
				yield return (X)Convert.ChangeType(v, typeof(X));
		}

		static public IEnumerable<X> As<X>(IEnumerable e) where X : class {
			foreach(object v in e) {
				X x = v as X;
				if(x != null)
					yield return x;
			}
		}

		public delegate bool Transformer<T, X>(T src, ref X dst);
		static public IEnumerable<X> Transform<T, X>(IEnumerable<T> e, Transformer<T, X> f) {
			foreach(T v in e) {
				X x = default(X);
				if(f(v, ref x))
					yield return x;
			}
		}		

		static public bool Exists<T>(IEnumerable<T> e, Predicate<T> match) {
			foreach(T v in e)
				if(match(v))
					return true;

			return false;
		}
		
		static public T Find<T>(IEnumerable<T> e, Predicate<T> match) {
			foreach(T v in e)
				if(match(v))
					return v;

			return default(T);
		}
		
		static public IEnumerable<T> FindAll<T>(IEnumerable<T> e, Predicate<T> match) {
			foreach(T v in e)
				if(match(v))
					yield return v;
		}

		static public void ForEach<T>(IEnumerable<T> e, Action<T> action) {
			foreach(T v in e)
				action(v);
		}

		static public IEnumerable<T> Reverse<T>(IEnumerable<T> e) {
			List<T> vs = new List<T>(e);
			vs.Reverse();
			foreach(T v in vs)
				yield return v;
		}
	}

	public abstract class FriendlyEnumerable<T> : IEnumerable<T> {
		public bool Exists(Predicate<T> match) {
			return Enumerations.Exists(this, match);
		}

		public T Find(Predicate<T> match) {
			return Enumerations.Find(this, match);
		}

		public IEnumerable FindAll(Predicate<T> match) {
			return Enumerations.FindAll(this, match);
		}

		public void ForEach(Action<T> action) {
			Enumerations.ForEach(this, action);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public abstract IEnumerator<T> GetEnumerator();
	}
}
