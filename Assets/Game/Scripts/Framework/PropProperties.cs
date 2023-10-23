using System.Collections.Generic;
using UnityEngine;

public class PropProperties
{
	private Dictionary<string, Variant> map;

	public delegate void PropertyUpdatedFunc(string property, Variant variant);
	public event PropertyUpdatedFunc onPropertyUpdated;
	
	public int Count
	{
		get
		{
			return map.Count;
		}
	}
	
	public PropProperties()
	{
		this.map = new Dictionary<string, Variant>(32);
	}

	public void Clear()
	{
		map.Clear();
	}
	
	public bool GetBool(BoolProperty property)
	{
		if (ValidateProperty(property))
		{
			return GetBool(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetBool(BoolProperty property, bool val)
	{
		if (ValidateProperty(property))
		{
			SetBool(property.key, val);
		}
	}

	public bool GetBool(string property, bool defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.Bool, out v))
		{
			return v.GetBool();
		}

		return defaultVal;
	}
	
	public void SetBool(string property, bool val)
	{
		SetValue(property, new Variant(val));
	}

	public float GetFloat(FloatProperty property)
	{
		if (ValidateProperty(property))
		{
			return GetFloat(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetFloat(FloatProperty property, float val)
	{
		if (ValidateProperty(property))
		{
			SetFloat(property.key, val);
		}
	}

	public float GetFloat(string property, float defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.Numeric, out v))
		{
			return v.GetFloat();
		}

		return defaultVal;
	}
	
	public void SetFloat(string property, float val)
	{
		SetValue(property, new Variant(val));
	}

	public int GetInt(IntProperty property)
	{
		if (ValidateProperty(property))
		{
			return GetInt(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetInt(IntProperty property, int val)
	{
		if (ValidateProperty(property))
		{
			SetInt(property.key, val);
		}
	}
	
	public int GetInt(string property, int defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.Numeric, out v))
		{
			return v.GetInt();
		}

		return defaultVal;
	}
	
	public void SetInt(string property, int val)
	{
		SetValue(property, new Variant(val));
	}
	
	public string GetString(StringProperty property)
	{
		if (ValidateProperty(property))
		{
			return GetString(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetString(StringProperty property, string val)
	{
		if (ValidateProperty(property))
		{
			SetString(property.key, val);
		}
	}

	public string GetString(string property, string defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.String, out v))
		{
			return v.GetString();
		}

		return defaultVal;
	}
	
	public void SetString(string property, string val)
	{
		SetValue(property, new Variant(val));
	}
	
	public Vector2 GetVector2(Vector2Property property)
	{
		if (ValidateProperty(property))
		{
			return GetVector2(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetVector2(Vector2Property property, Vector2 val)
	{
		if (ValidateProperty(property))
		{
			SetVector2(property.key, val);
		}
	}

	public Vector2 GetVector2(string property, Vector2 defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.Vector2, out v))
		{
			return v.GetVector2();
		}

		return defaultVal;
	}
	
	public void SetVector2(string property, Vector2 val)
	{
		SetValue(property, new Variant(val));
	}

	public Vector3 GetVector3(Vector3Property property)
	{
		if (ValidateProperty(property))
		{
			return GetVector3(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetVector3(Vector3Property property, Vector3 val)
	{
		if (ValidateProperty(property))
		{
			SetVector3(property.key, val);
		}
	}

	public Vector3 GetVector3(string property, Vector3 defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.Vector3, out v))
		{
			return v.GetVector3();
		}

		return defaultVal;
	}
	
	public void SetVector3(string property, Vector3 val)
	{
		SetValue(property, new Variant(val));
	}

	public Color GetColor(ColorProperty property)
	{
		if (ValidateProperty(property))
		{
			return GetColor(property.key, property.defaultValue);
		}

		return property.defaultValue;
	}

	public void SetColor(ColorProperty property, Color val)
	{
		if (ValidateProperty(property))
		{
			SetColor(property.key, val);
		}
	}

	public Color GetColor(string property, Color defaultVal)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.Color, out v))
		{
			return v.GetColor();
		}

		return defaultVal;
	}
	
	public void SetColor(string property, Color val)
	{
		SetValue(property, new Variant(val));
	}

	public PropGuid GetPropGuid(PropGuidProperty property)
	{
		if (ValidateProperty(property))
		{
			return GetPropGuid(property.key);
		}

		return PropGuid.Empty;
	}

	public void SetPropGuid(PropGuidProperty property, PropGuid val)
	{
		if (ValidateProperty(property))
		{
			SetPropGuid(property.key, val);
		}
	}

	public PropGuid GetPropGuid(string property)
	{
		Variant v;

		if (GetValue(property, Variant.VariantType.PropGuid, out v))
		{
			return v.GetPropGuid();
		}

		return PropGuid.Empty;
	}
	
	public void SetPropGuid(string property, PropGuid val)
	{
		SetValue(property, new Variant(val));
	}

	public bool GetValue(string property, Variant.VariantType variantType, out Variant val)
	{
		return (map.TryGetValue(property, out val) && ValidateType(val, variantType));
	}
	
	public void SetValue(string property, Variant val)
	{
		map[property] = val;
		onPropertyUpdated?.Invoke(property, val);
	}

	public Dictionary<string, Variant>.Enumerator GetEnumerator()
	{
		return map.GetEnumerator();
	}

	private bool ValidateType(Variant variant, Variant.VariantType variantType)
	{
		if (variant.variantType != variantType)
		{
			Logger.Instance.ShowDebugError("Expecting type " + variantType + " but found " + variant.variantType);
			
			return false;
		}

		return true;
	}

	private bool ValidateProperty(GenericProperty property)
	{
		if (property == null)
		{
			Logger.Instance.ShowDebugError("Property is null - has it been selected correctly from the editor?");
			
			return false;
		}
		else if (property.key == "")
		{
			Logger.Instance.ShowDebugError("Property key is empty.");

			return false;
		}
		
		return true;
	}
}
