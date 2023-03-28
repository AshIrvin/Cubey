using UnityEngine;

public struct Variant
{
	// The values in this enumeration are used for serialisation - do
	// not change them, add new ones instead! Currently SliceUtil.cs
	// assumes that they can be reliably cast to a byte too.
	public enum VariantType
	{
		Bool = 0,
		Numeric = 1,
		String = 2,
		Vector2 = 3,
		Vector3 = 4,
		Color = 5,
		PropGuid = 6
	}

	public VariantType variantType { get; private set; }

	// This is a little heavyweight, but it keeps the behaviour
	// consistent without trying to do anything clever. If the size of
	// this struct becomes a problem, we can make an unsafe version
	// that implements a C-style union and make absolutely sure that
	// it's not possible to create an instance with an invalid state.
	private double numeric;
	private string str;
	private Vector4 vector;
	private PropGuid propGuid;

	public Variant(bool b)
	{
		this.variantType = VariantType.Bool;
		this.numeric = b ? 1.0 : 0.0;
		this.str = "";
		this.vector = Vector4.zero;
		this.propGuid = PropGuid.Empty;
	}
	
	public Variant(int numeric)
	{
		this.variantType = VariantType.Numeric;
		this.numeric = (double)numeric;
		this.str = "";
		this.vector = Vector4.zero;
		this.propGuid = PropGuid.Empty;
	}
	
	public Variant(double numeric)
	{
		this.variantType = VariantType.Numeric;
		this.numeric = numeric;
		this.str = "";
		this.vector = Vector4.zero;
		this.propGuid = PropGuid.Empty;
	}

	public Variant(string str)
	{
		this.variantType = VariantType.String;
		this.numeric = 0.0;
		this.str = str;
		this.vector = Vector4.zero;
		this.propGuid = PropGuid.Empty;
	}

	public Variant(Vector2 vector2)
	{
		this.variantType = VariantType.Vector2;
		this.numeric = 0.0;
		this.str = "";
		this.vector = vector2;
		this.propGuid = PropGuid.Empty;
	}

	public Variant(Vector3 vector3)
	{
		this.variantType = VariantType.Vector3;
		this.numeric = 0.0;
		this.str = "";
		this.vector = vector3;
		this.propGuid = PropGuid.Empty;
	}
	
	public Variant(Color color)
	{
		this.variantType = VariantType.Color;
		this.numeric = 0.0;
		this.str = "";
		this.vector = color;
		this.propGuid = PropGuid.Empty;
	}

	public Variant(PropGuid propGuid)
	{
		this.variantType = VariantType.PropGuid;
		this.numeric = 0.0;
		this.str = "";
		this.vector = Vector4.zero;
		this.propGuid = propGuid;
	}

	public bool GetBool()
	{
		return System.Math.Abs(numeric) > 0.00001f;
	}

	public int GetInt()
	{
		return (int)numeric;
	}

	public float GetFloat()
	{
		return (float)numeric;
	}

	public double GetDouble()
	{
		return numeric;
	}

	public string GetString()
	{
		return str;
	}

	public Vector2 GetVector2()
	{
		return vector;
	}

	public Vector3 GetVector3()
	{
		return vector;
	}

	public Color GetColor()
	{
		return vector;
	}

	public PropGuid GetPropGuid()
	{
		return propGuid;
	}

	public override string ToString()
	{
		switch (variantType)
		{
		case VariantType.Bool:
			return GetBool() ? "true" : "false";

		case VariantType.Numeric:
			return GetDouble().ToString();

		case VariantType.String:
			return GetString();

		case VariantType.Vector2:
			return GetVector2().ToString();

		case VariantType.Vector3:
			return GetVector3().ToString();

		case VariantType.Color:
			return GetColor().ToString();

		case VariantType.PropGuid:
			return GetPropGuid().ToString();
		}

		return "<Invalid Type>";
	}
}
