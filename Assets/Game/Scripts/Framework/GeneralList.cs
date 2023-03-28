using System.Collections.Generic;
using UnityEngine;

public abstract class GenericList<T> : ScriptableObject
{
    [SerializeField] private List<T> list;
    
    public T this[int index]
    {
        get
        {
            return list[index];
        }
    }

    public int Count
    {
        get
        {
            return list.Count;
        }
    }
	
    public List<T>.Enumerator GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public void UnityEditorPopulate(List<T> newList)
    {
        list.Clear();
        list.AddRange(newList);
    }
    
    
    
}