using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	public class Set<T> : IEnumerable<T> {
		private Dictionary<T, int> dct = new Dictionary<T, int>();

		public Set() {

		}

		public Set(IEnumerable<T> items) {
			AddRange(items);
		}

		public void Add(T item) {
			dct.Add(item, 0);
		}

		public bool TryAdd(T item) {
			try {
				Add(item);
				return true;
			}
			catch(Exception) {
				return false;
			}
		}

		public void AddRange(IEnumerable<T> items) {
			foreach(T item in items)
				Add(item);
		}

		public int Count {
			get { return dct.Count; }
		}

		public bool Contains(T item) {
			return dct.ContainsKey(item);
		}

		public void Remove(T item) {
			dct.Remove(item);
		}

		public IEnumerator<T> GetEnumerator() {
			return dct.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return dct.Keys.GetEnumerator();
		}

		public bool this[T item] {
			get {
				return Contains(item);
			}
			set {
				if(value)
					Add(item);
				else
					Remove(item);
			}
		}					
	}
}
