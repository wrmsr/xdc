using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace xdc.common {
	[DebuggerDisplay("Count = {Count}")]
	public class ListAccessor<T> : IEnumerable<T>, IEnumerable {
		private List<T> lst;

		public ListAccessor(List<T> _lst) {
			lst = _lst;
		}

		public virtual int Capacity { get { return lst.Capacity; } }
		public virtual int Count { get { return lst.Count; } }

		public virtual T this[int index] { get { return lst[index]; } }

		public virtual ReadOnlyCollection<T> AsReadOnly() { return lst.AsReadOnly(); }
		public virtual int BinarySearch(T item) { return lst.BinarySearch(item); }
		public virtual int BinarySearch(T item, IComparer<T> comparer) { return lst.BinarySearch(item, comparer); }
		public virtual int BinarySearch(int index, int count, T item, IComparer<T> comparer) { return lst.BinarySearch(index, count, item, comparer); }
		public virtual bool Contains(T item) { return lst.Contains(item); }
		public virtual List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) { return lst.ConvertAll<TOutput>(converter); }
		public virtual void CopyTo(T[] array) { lst.CopyTo(array); }
		public virtual void CopyTo(T[] array, int arrayIndex) { lst.CopyTo(array, arrayIndex); }
		public virtual void CopyTo(int index, T[] array, int arrayIndex, int count) { lst.CopyTo(index, array, arrayIndex, count); }
		public virtual bool Exists(Predicate<T> match) { return lst.Exists(match); }
		public virtual T Find(Predicate<T> match) { return lst.Find(match); }
		public virtual List<T> FindAll(Predicate<T> match) { return lst.FindAll(match); }
		public virtual int FindIndex(Predicate<T> match) { return lst.FindIndex(match); }
		public virtual int FindIndex(int startIndex, Predicate<T> match) { return lst.FindIndex(startIndex, match); }
		public virtual int FindIndex(int startIndex, int count, Predicate<T> match) { return lst.FindIndex(startIndex, count, match); }
		public virtual T FindLast(Predicate<T> match) { return lst.FindLast(match); }
		public virtual int FindLastIndex(Predicate<T> match) { return lst.FindLastIndex(match); }
		public virtual int FindLastIndex(int startIndex, Predicate<T> match) { return lst.FindLastIndex(startIndex, match); }
		public virtual int FindLastIndex(int startIndex, int count, Predicate<T> match) { return lst.FindLastIndex(startIndex, count, match); }
		public virtual void ForEach(Action<T> action) { lst.ForEach(action); }
		public virtual IEnumerator<T> GetEnumerator() { return lst.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return lst.GetEnumerator(); }
		public virtual List<T> GetRange(int index, int count) { return lst.GetRange(index, count); }
		public virtual int IndexOf(T item) { return lst.IndexOf(item); }
		public virtual int IndexOf(T item, int index) { return lst.IndexOf(item, index); }
		public virtual int IndexOf(T item, int index, int count) { return lst.IndexOf(item, index, count); }
		public virtual int LastIndexOf(T item) { return lst.LastIndexOf(item); }
		public virtual int LastIndexOf(T item, int index) { return lst.LastIndexOf(item, index); }
		public virtual int LastIndexOf(T item, int index, int count) { return lst.LastIndexOf(item, index, count); }
		public virtual T[] ToArray() { return lst.ToArray(); }
		public virtual bool TrueForAll(Predicate<T> match) { return lst.TrueForAll(match); }
	}
}
