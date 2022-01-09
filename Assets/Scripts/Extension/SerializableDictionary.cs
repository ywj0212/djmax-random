using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public TValue this[TKey key] {
            get {
                int i = keys.IndexOf(key);
                return values[i];
            }
        }
    }
}
