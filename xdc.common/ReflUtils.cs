using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace xdc.common {
	static public class ReflUtils {
		static public object CopyByName(object dst, object src) {
			foreach(FieldInfo df in dst.GetType().GetFields()) {
				FieldInfo sf = src.GetType().GetField(df.Name);
				if(sf != null) df.SetValue(dst, sf.GetValue(src));
				PropertyInfo sp = src.GetType().GetProperty(df.Name);
				if(sp != null) df.SetValue(dst, sp.GetValue(src, null));
			}

			foreach(PropertyInfo dp in dst.GetType().GetProperties()) {
				FieldInfo sf = src.GetType().GetField(dp.Name);
				if(sf != null) dp.SetValue(dst, sf.GetValue(src), null);
				PropertyInfo sp = src.GetType().GetProperty(dp.Name);
				if(sp != null) dp.SetValue(dst, sp.GetValue(src, null), null);
			}

			return dst;
		}

		static public bool Is(object obj, Type t) {
			return obj.GetType() == t || obj.GetType().IsSubclassOf(t);
		}
	}
}
