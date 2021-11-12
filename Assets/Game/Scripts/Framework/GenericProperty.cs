using UnityEngine;

public abstract class GenericProperty : ScriptableObject
{
	[SerializeField]
	private string _key;

	public string key
	{
		get
		{
			return _key;
		}
	}
}

public abstract class GenericProperty<T> : GenericProperty
{
	[SerializeField]
	private T _defaultValue;
	
	public T defaultValue
	{
		get
		{
			return _defaultValue;
		}
	}
}
