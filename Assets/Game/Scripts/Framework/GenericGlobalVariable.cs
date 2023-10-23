using System.Collections.Generic;
using UnityEngine;

public abstract class GenericGlobalVariable<T> : ScriptableObject
{
	[SerializeField] private T defaultValue;
	
	private T currentValue;
	
	public event System.Action<T> OnValueChanged;
	
	public T CurrentValue
	{
		get
		{
			return currentValue;
		}

		set
		{
			if (!EqualityComparer<T>.Default.Equals(currentValue, value))
			{
				currentValue = value;
				OnValueChanged?.Invoke(currentValue);
			}
		}
	}

	private void OnEnable()
	{
		currentValue = defaultValue;
	}
}
