using UnityEngine;
using System;

// Like System.Guid except much easier to handle serialisation without
// memory allocation and the 128-bit length is a hard guarantee.
// Also compatible with the Unity inspector.

[System.Serializable]
public struct PropGuid : IEquatable<PropGuid>
{
	public static readonly PropGuid Empty = new PropGuid(0, 0);

	[SerializeField]
	private ulong _a;

	[SerializeField]
	private ulong _b;

	// Allocates, but this is only ever going to be called when we're
	// instantiating new props. We can get around this as soon as we
	// have System.Span<> available in Unity.
	public static PropGuid Create()
	{
		byte [] byteArray = Guid.NewGuid().ToByteArray();

		// Looking at the current implemention of Guid, this seems
		// impossible, but let's grab it anyway so we know if the
		// problem ever pops up for some strange reason.
		if (byteArray.Length != 16)
		{
			throw new InvalidOperationException("Guid.CreateGuid().ToByteArray() returned unexpected length of " + byteArray.Length);
		}

		return FromByteArray(byteArray, 0);
	}

	private PropGuid(ulong _a, ulong _b)
	{
		this._a = _a;
		this._b = _b;
	}

	public bool Equals(PropGuid rhs)
	{
		return (_a == rhs._a) && (_b == rhs._b);
	}

	public override bool Equals(object o)
	{
		return (o is PropGuid) && Equals((PropGuid)o);
	}

	public override int GetHashCode()
	{
		return _a.GetHashCode() ^ _b.GetHashCode();
	}

	public override string ToString()
	{
		return _a.ToString("X16") + _b.ToString("X16");
	}
	
	public static PropGuid Parse(string hexString)
	{
		if (hexString.Length != 32)
		{
			throw new System.FormatException("Invalid hex string length " + hexString.Length + " expected 32.");
		}

		ulong newA = UInt64.Parse(hexString.Substring(0, 16),
								  System.Globalization.NumberStyles.HexNumber);

		ulong newB = UInt64.Parse(hexString.Substring(16, 16),
								  System.Globalization.NumberStyles.HexNumber);

		return new PropGuid(newA, newB);
	}
	
	public static void Push(byte [] data, ref int index, PropGuid p)
	{
		Push(data, ref index, p._b);
		Push(data, ref index, p._a);
	}

	public static PropGuid Pop(byte [] data, ref int index)
	{
		PropGuid p = FromByteArray(data, index);

		index += 16;

		return p;
	}

	private static void Push(byte [] data, ref int index, ulong u)
	{
		data[index++] = (byte)(u & 0xff);
		data[index++] = (byte)((u >> 8) & 0xff);
		data[index++] = (byte)((u >> 16) & 0xff);
		data[index++] = (byte)((u >> 24) & 0xff);
		data[index++] = (byte)((u >> 32) & 0xff);
		data[index++] = (byte)((u >> 40) & 0xff);
		data[index++] = (byte)((u >> 48) & 0xff);
		data[index++] = (byte)((u >> 56) & 0xff);
	}

	private static PropGuid FromByteArray(byte [] byteArray, int index)
	{
		ulong newB = ((ulong)byteArray[index] |
					  ((ulong)byteArray[index + 1] << 8) |
					  ((ulong)byteArray[index + 2] << 16) |
					  ((ulong)byteArray[index + 3] << 24) |
					  ((ulong)byteArray[index + 4] << 32) |
					  ((ulong)byteArray[index + 5] << 40) |
					  ((ulong)byteArray[index + 6] << 48) |
					  ((ulong)byteArray[index + 7] << 56));
		
		ulong newA = ((ulong)byteArray[index + 8] |
					  ((ulong)byteArray[index + 9] << 8) |
					  ((ulong)byteArray[index + 10] << 16) |
					  ((ulong)byteArray[index + 11] << 24) |
					  ((ulong)byteArray[index + 12] << 32) |
					  ((ulong)byteArray[index + 13] << 40) |
					  ((ulong)byteArray[index + 14] << 48) |
					  ((ulong)byteArray[index + 15] << 56));

		return new PropGuid(newA, newB);
	}
	
	public static bool operator==(PropGuid lhs, PropGuid rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator!=(PropGuid lhs, PropGuid rhs)
	{
		return !lhs.Equals(rhs);
	}
}
