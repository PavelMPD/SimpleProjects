using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.CR
{
	internal static class EnumerableExtensions
	{
		public static bool HasFirst<T>(this IEnumerable<T> col,int count)
		{
			using (IEnumerator<T> enumerator = col.GetEnumerator())
			{
				enumerator.Reset();
				while (enumerator.MoveNext() && --count > 0) ;
				
				return count == 0;
			}
		}

		public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> collection1, IEnumerable<T2> collection2)
		{
			if (collection1 == null)
				throw new ArgumentNullException("collection1");
			if (collection2 == null)
				throw new ArgumentNullException("collection2");

			using (IEnumerator<T1> enumerator1 = collection1.GetEnumerator())
			using (IEnumerator<T2> enumerator2 = collection2.GetEnumerator())
			{
				while (enumerator1.MoveNext() && enumerator2.MoveNext())
					yield return new Tuple<T1, T2>(enumerator1.Current, enumerator2.Current);
			}
		}

		public static Tuple<IEnumerable<T1>, IEnumerable<T2>> UnZip<T1, T2>(this IEnumerable<Tuple<T1, T2>> zippedCol)
		{
			var collection1 = new List<T1>();
			var collection2 = new List<T2>();

			zippedCol.ForEach(zip =>
			{
				collection1.Add(zip.Item1);
				collection2.Add(zip.Item2);
			});

			return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(collection1, collection2);
		}

		public static void ForEach<T>(this IEnumerable<T> col, Action<T> act)
		{
			foreach (T item in col) act(item);
		}

		public static IEnumerable<T> Distinct<T, TValue>(this IEnumerable<T> col, Func<T, TValue> distinctMember)
		{
			return col.Distinct(new GenericComparer<T, TValue>(distinctMember));
		}


		private class GenericComparer<T, TValue> : IEqualityComparer<T>
		{
			private readonly Func<T, TValue> _objectProvider;

			public GenericComparer(Func<T, TValue> objectProvider)
			{
				_objectProvider = objectProvider;
			}

			public bool Equals(T x, T y)
			{
				return _objectProvider(x).Equals(_objectProvider(y));
			}

			public int GetHashCode(T obj)
			{
				return _objectProvider(obj).GetHashCode();
			}

		}


		public static bool CheckedAny<T>(this IEnumerable<T> col)
		{
			if (col == null) return false;
			return col.Any();
		}

	}
}
