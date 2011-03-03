using System;
using System.Collections.Generic;
using System.Text;

namespace xdc.common {
	public class Atts : DictionaryAccessor<string, string> {
		public Atts()
			: base() {
			dct = new Dictionary<string, string>();
		}

		public Atts(params string[] pairs)
			: base() {
			dct = new Dictionary<string, string>();
			Add(pairs);
		}

		public Atts(IEnumerable<KeyValuePair<string, string>> e)
			: base() {
			dct = new Dictionary<string, string>();
			Add(e);
		}

		public void Add(string key, string value) {
			dct.Add(key, value);
		}

		public void Add(string[] pairs) {
			if(pairs != null)
				for(int i = 1; i < pairs.Length; i++)
					Add(pairs[i - 1], pairs[i]);
		}

		public void Add(IEnumerable<KeyValuePair<string, string>> e) {
			if(e != null)
				foreach(KeyValuePair<string, string> p in e)
					Add(p.Key, p.Value);
		}

		public new string this[string k] {
			get {
				string v;
				if(!dct.TryGetValue(k, out v))
					return null;
				return v;
			}
			//TODO: ReadOnly somehow
			set {
				dct[k] = value;
			}
		}

		public bool GetBool(string name) {
			return (this[name] ?? "false").ToLower() == "true";
		}

		public int GetInt(string name) {
			string v = this[name];
			return string.IsNullOrEmpty(v) ? 0 : Convert.ToInt32(v);
		}
	}
}
