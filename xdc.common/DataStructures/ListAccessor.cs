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

		public int Capacity { get { return lst.Capacity; } }
		public int Count { get { return lst.Count; } }

		public T this[int index] { get { return lst[index]; } }

		public ReadOnlyCollection<T> AsReadOnly() { return lst.AsReadOnly(); }
		public int BinarySearch(T item) { return lst.BinarySearch(item); }
		public int BinarySearch(T item, IComparer<T> comparer) { return lst.BinarySearch(item, comparer); }
		public int BinarySearch(int index, int count, T item, IComparer<T> comparer) { return lst.BinarySearch(index, count, item, comparer); }
		public bool Contains(T item) { return lst.Contains(item); }
		public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) { return lst.ConvertAll<TOutput>(converter); }
		public void CopyTo(T[] array) { lst.CopyTo(array); }
		public void CopyTo(T[] array, int arrayIndex) { lst.CopyTo(array, arrayIndex); }
		public void CopyTo(int index, T[] array, int arrayIndex, int count) { lst.CopyTo(index, array, arrayIndex, count); }
		public bool Exists(Predicate<T> match) { return lst.Exists(match); }
		public T Find(Predicate<T> match) { return lst.Find(match); }
		public List<T> FindAll(Predicate<T> match) { return lst.FindAll(match); }
		public int FindIndex(Predicate<T> match) { return lst.FindIndex(match); }
		public int FindIndex(int startIndex, Predicate<T> match) { return lst.FindIndex(startIndex, match); }
		public int FindIndex(int startIndex, int count, Predicate<T> match) { return lst.FindIndex(startIndex, count, match); }
		public T FindLast(Predicate<T> match) { return lst.FindLast(match); }
		public int FindLastIndex(Predicate<T> match) { return lst.FindLastIndex(match); }
		public int FindLastIndex(int startIndex, Predicate<T> match) { return lst.FindLastIndex(startIndex, match); }
		public int FindLastIndex(int startIndex, int count, Predicate<T> match) { return lst.FindLastIndex(startIndex, count, match); }
		public void ForEach(Action<T> action) { lst.ForEach(action); }
		public IEnumerator<T> GetEnumerator() { return lst.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return lst.GetEnumerator(); }
		public List<T> GetRange(int index, int count) { return lst.GetRange(index, count); }
		public int IndexOf(T item) { return lst.IndexOf(item); }
		public int IndexOf(T item, int index) { return lst.IndexOf(item, index); }
		public int IndexOf(T item, int index, int count) { return lst.IndexOf(item, index, count); }
		public int LastIndexOf(T item) { return lst.LastIndexOf(item); }
		public int LastIndexOf(T item, int index) { return lst.LastIndexOf(item, index); }
		public int LastIndexOf(T item, int index, int count) { return lst.LastIndexOf(item, index, count); }
		public T[] ToArray() { return lst.ToArray(); }
		public bool TrueForAll(Predicate<T> match) { return lst.TrueForAll(match); }
	}
}
