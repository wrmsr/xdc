using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace xdc.common {
	[DebuggerDisplay("Count = {Count}")]
	public class DictionaryAccessor<K, V> : IEnumerable<KeyValuePair<K, V>>, IEnumerable {
		protected Dictionary<K, V> dct;

		protected DictionaryAccessor() {
		}

		public DictionaryAccessor(Dictionary<K, V> _dct) {
			dct = _dct;
		}

		public virtual IEqualityComparer<K> Comparer { get { return dct.Comparer; } }
		public virtual int Count { get { return dct.Count; } }
		public virtual Dictionary<K, V>.KeyCollection Keys { get { return dct.Keys; } }
		public virtual Dictionary<K, V>.ValueCollection Values { get { return dct.Values; } }

		public virtual V this[K key] { get { return TryGetValue(key); } }

		public virtual bool ContainsKey(K key) { return dct.ContainsKey(key); }
		public virtual bool ContainsValue(V value) { return dct.ContainsValue(value); }
		IEnumerator IEnumerable.GetEnumerator() { return dct.GetEnumerator(); }
		IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() { return dct.GetEnumerator(); }
		//public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { return dct.GetObjectData(info, context); }
		public virtual bool TryGetValue(K key, out V value) { return dct.TryGetValue(key, out value); }

		public virtual V TryGetValue(K key) {
			V value;
			TryGetValue(key, out value);
			return value;
		}
	}
}
