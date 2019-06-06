using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SplitMap<K, V> where K : IEquatable<K>
{

    public void Add(K key, V value)
    {
        System.Diagnostics.Debug.Assert(m_Keys.FindIndex((searchKey) => searchKey.Equals(key)) == -1);
        m_Keys.Add(key);
        m_Values.Add(value);
    }

    public bool HasKey(K key)
    {
        return m_Keys.FindIndex((searchKey) => searchKey.Equals(key)) != -1;
    }

    public void Remove(K key)
    {
        int index = m_Keys.FindIndex((searchKey) => searchKey.Equals(key));
        System.Diagnostics.Debug.Assert(index != -1);
        m_Keys.RemoveAt(index);
        m_Values.RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        m_Keys.RemoveAt(index);
        m_Values.RemoveAt(index);
    }

    public List<K> Keys { get => m_Keys; }
    public List<V> Values { get => m_Values; }

    public int Count { get => m_Keys.Count; }

    public V this[K key]
    {
        get
        {
            for(int i = 0; i < m_Keys.Count; i++)
            {
                if(m_Keys[i].Equals(key))
                {
                    return m_Values[i];
                }
            }

             return default(V);
         }

         set
         {
            for(int i = 0; i < m_Keys.Count; i++)
            {
                if(m_Keys[i].Equals(key))
                {
                    m_Values[i] = value;
                    return;
                }
            }

            System.Diagnostics.Debug.Fail("Key not found!");
        }
    }

    [SerializeField]
    private List<K> m_Keys = new List<K>();

    [SerializeField]
    private List<V> m_Values = new List<V>();
}
