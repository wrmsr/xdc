using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	public class CounterSet<K> {
		private Dictionary<K, int> dct = new Dictionary<K, int>();

		public CounterSet() {
		
		}

		public int this[K key] {
			get {
				int count;
				if(dct.TryGetValue(key, out count))
					return count;
				return 0;
			}
			set {
				dct[key] = value;
			}
		}

		public int Inc(K key) {
			return (this[key] = this[key] + 1);
		}

		public int Dec(K key) {
			return (this[key] = this[key] - 1);
		}

		public void Clear() {
			dct.Clear();
		}

		public int Count() {
			return dct.Count;
		}
	}
}
