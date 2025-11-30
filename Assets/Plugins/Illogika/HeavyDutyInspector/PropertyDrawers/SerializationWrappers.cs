//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace HeavyDutyInspector
{

	[System.Serializable]
	public class UInt16S : System.Object
	{

		[SerializeField]
		private byte[] _bytes;

		[SerializeField]
		private bool _endianness;

		public UInt16S()
		{
			_bytes = BitConverter.GetBytes((UInt16)0);
			_endianness = BitConverter.IsLittleEndian;
		}

		public UInt16S(UInt16 value)
		{
			_bytes = BitConverter.GetBytes(value);
			_endianness = BitConverter.IsLittleEndian;
		}

		public static implicit operator UInt16(UInt16S value)
		{
			return BitConverter.ToUInt16(BitConverter.IsLittleEndian == value._endianness ? value._bytes : value._bytes.Reverse().ToArray(), 0);
		}

		public static implicit operator UInt16S(UInt16 value)
		{
			return new UInt16S(value);
		}
	}

	[System.Serializable]
	public class UInt32S : System.Object
	{

		[SerializeField]
		private byte[] _bytes;

		[SerializeField]
		private bool _endianness;

		public UInt32S()
		{
			_bytes = BitConverter.GetBytes(0U);
			_endianness = BitConverter.IsLittleEndian;
		}

		public UInt32S(UInt32 value)
		{
			_bytes = BitConverter.GetBytes(value);
			_endianness = BitConverter.IsLittleEndian;
		}

		public static implicit operator UInt32(UInt32S value)
		{
			return BitConverter.ToUInt32(BitConverter.IsLittleEndian == value._endianness ? value._bytes : value._bytes.Reverse().ToArray(), 0);
		}

		public static implicit operator UInt32S(UInt32 value)
		{
			return new UInt32S(value);
		}
	}

	[System.Serializable]
	public class UInt64S : System.Object
	{

		[SerializeField]
		private byte[] _bytes;

		[SerializeField]
		private bool _endianness;

		public UInt64S()
		{
			_bytes = BitConverter.GetBytes(0UL);
			_endianness = BitConverter.IsLittleEndian;
		}

		public UInt64S(UInt64 value)
		{
			_bytes = BitConverter.GetBytes(value);
			_endianness = BitConverter.IsLittleEndian;
		}

		public static implicit operator UInt64(UInt64S value)
		{
			return BitConverter.ToUInt64(BitConverter.IsLittleEndian == value._endianness ? value._bytes : value._bytes.Reverse().ToArray(), 0);
		}

		public static implicit operator UInt64S(UInt64 value)
		{
			return new UInt64S(value);
		}
	}

	[System.Serializable]
	public class Int16S : System.Object
	{

		[SerializeField]
		private byte[] _bytes;

		[SerializeField]
		private bool _endianness;

		public Int16S()
		{
			_bytes = BitConverter.GetBytes((Int16)0);
			_endianness = BitConverter.IsLittleEndian;
		}

		public Int16S(Int16 value)
		{
			_bytes = BitConverter.GetBytes(value);
			_endianness = BitConverter.IsLittleEndian;
		}

		public static implicit operator Int16(Int16S value)
		{
			return BitConverter.ToInt16(BitConverter.IsLittleEndian == value._endianness ? value._bytes : value._bytes.Reverse().ToArray(), 0);
		}

		public static implicit operator Int16S(Int16 value)
		{
			return new Int16S(value);
		}
	}

	[System.Serializable]
	public class Int64S : System.Object
	{

		[SerializeField]
		private byte[] _bytes;

		[SerializeField]
		private bool _endianness;

		public Int64S()
		{
			_bytes = BitConverter.GetBytes(0L);
			_endianness = BitConverter.IsLittleEndian;
		}

		public Int64S(Int64 value)
		{
			_bytes = BitConverter.GetBytes(value);
			_endianness = BitConverter.IsLittleEndian;
		}

		public static implicit operator Int64(Int64S value)
		{
			return BitConverter.ToInt64(BitConverter.IsLittleEndian == value._endianness ? value._bytes : value._bytes.Reverse().ToArray(), 0);
		}

		public static implicit operator Int64S(Int64 value)
		{
			return new Int64S(value);
		}
	}

}
