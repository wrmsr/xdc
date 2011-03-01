using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	public class CounterSet {
		private Dictionary<string, int> dct = new Dictionary<string, int>();

		public CounterSet() {
		
		}

		public int this[string name] {
			get {
				int count;
				if(dct.TryGetValue(name, out count))
					return count;
				return 0;
			}
			set {
				dct[name] = value;
			}
		}

		public int Inc(string name) {
			return (this[name] = this[name] + 1);
		}

		public int Dec(string name) {
			return (this[name] = this[name] - 1);
		}

		public void Clear() {
			dct.Clear();
		}

		public int Count() {
			return dct.Count;
		}
	}
}
