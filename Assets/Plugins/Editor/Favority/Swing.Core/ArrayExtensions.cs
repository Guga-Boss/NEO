using System;
using System.Collections.Generic;

namespace Swing
{
	public static class ArrayExtensions
	{
		public static bool isNullOrEmpty( this Array _this )
		{
			return _this == null || _this.Length == 0;
		}
		public static bool isNullOrEmpty<T>( this List<T> _this )
		{
			return _this == null || _this.Count == 0;
		}

		public static int indexOf( this Array _this, object _value )
		{
			return Array.IndexOf(_this, _value);
		}
		public static int indexOf( this Array _this, object _value, int _startIndex )
		{
			return Array.IndexOf(_this, _value, _startIndex);
		}
		public static int indexOf( this Array _this, object _value, int _startIndex, int _count )
		{
			return Array.IndexOf(_this, _value, _startIndex, _count);
		}

		public static int indexOf<T>( this T[] _this, T _value )
		{
			return Array.IndexOf(_this, _value);
		}
		public static int indexOf<T>( this T[] _this, T _value, int _startIndex )
		{
			return Array.IndexOf(_this, _value, _startIndex);
		}
		public static int indexOf<T>( this T[] _this, T _value, int _startIndex, int _count )
		{
			return Array.IndexOf(_this, _value, _startIndex, _count);
		}

		public static bool contains( this Array _this, object _value )
		{
			return Array.IndexOf(_this, _value) != -1;
		}
		public static bool contains<T>( this T[] _this, T _value )
		{
			return Array.IndexOf(_this, _value) != -1;
		}

		public static T find<T>( this T[] _this, Predicate<T> _match )
		{
			return Array.Find(_this, _match);
		}
		public static T[] findAll<T>( this T[] _this, Predicate<T> _match )
		{
			return Array.FindAll(_this, _match);
		}
		public static int findIndex<T>( this T[] _this, Predicate<T> _match )
		{
			return Array.FindIndex(_this, _match);
		}
		public static bool exists<T>( this T[] _this, Predicate<T> _match )
		{
			return Array.Exists(_this, _match);
		}
		public static List<T> toList<T>( this T[] _this )
		{
			return new List<T>(_this);
		}

		public static TOutput[] convertAll<TInput, TOutput>( this TInput[] _this, Converter<TInput, TOutput> _converter )
		{
			return Array.ConvertAll(_this, _converter);
		}

		public static void add<T>( ref T[] _array, T _item )
		{
			Array.Resize(ref _array, _array.Length + 1);
			_array[_array.Length - 1] = _item;
		}

		/// <summary>
		/// Eg [0, 1, 2, 3] becomes [1, 2, 3, 0]
		/// </summary>
		public static void shiftLeft<T>( this T[] _array )
		{
			T first = _array[0];
			for (int i = 0; i < _array.Length - 1; i++)
			{
				_array[i] = _array[i + 1];
			}
			_array[_array.Length - 1] = first;
		}

		/// <summary>
		/// Eg [0, 1, 2, 3] becomes [3, 0 ,1, 2]
		/// </summary>
		public static void shiftRight<T>( this T[] _array )
		{
			T last = _array[_array.Length - 1];
			for (int i = _array.Length - 1; i > 0; i--)
			{
				_array[i] = _array[i - 1];
			}
			_array[0] = last;
		}

		/// <summary>
		/// Compare contents, assuming the length is the same.
		/// </summary>
		public static bool isContentEqual<T>( T[] _a, T[] _b, int _length )
		{
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			for (int i = 0; i < _length; i++)
			{
				if (!comparer.Equals(_a[i], _b[i]))
				{
					return false;
				}
			}

			return true;
		}
		/// <summary>
		/// Compare contents, assuming the length is the same.
		/// </summary>
		public static bool isContentEqual( int[] _a, int[] _b, int _length )
		{
			for (int i = 0; i < _length; i++)
			{
				if (_a[i] != _b[i])
				{
					return false;
				}
			}

			return true;
		}
		/// <summary>
		/// Compare contents, assuming the length is the same.
		/// </summary>
		public static bool isContentEqual( float[] _a, float[] _b, int _length )
		{
			for (int i = 0; i < _length; i++)
			{
				if (_a[i] != _b[i])
				{
					return false;
				}
			}

			return true;
		}

		public static T last<T>( this T[] _this )
		{
			return _this[_this.Length - 1];
		}
		public static int count<T>( this T[] _this, Predicate<T> _match )
		{
			int matchCount = 0;

			int length = _this.Length;
			for (int i = 0; i < length; i++)
			{
				if (_match(_this[i]))
				{
					matchCount++;
				}
			}

			return matchCount;
		}

		#region stolen from UnityEditor.ArrayUtility

		public static void remove<T>( ref T[] _array, T _item )
		{
			List<T> list = new List<T>(_array);
			list.Remove(_item);
			_array = list.ToArray();
		}
		public static void removeAt<T>( ref T[] _array, int _index )
		{
			List<T> list = new List<T>(_array);
			list.RemoveAt(_index);
			_array = list.ToArray();
		}

		#endregion
	}
}
