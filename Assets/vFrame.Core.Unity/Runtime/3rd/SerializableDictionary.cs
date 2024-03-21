// From https://raw.githubusercontent.com/TheOddler/unity-helpers/master/SerializableDictionary.cs
//

using System.Collections.Generic;
using UnityEngine;

//
// Unity doesn't know how to serialize a Dictionary
// So this is a simple extension of a dictionary that saves as two lists.
// By Pablo Bollansée

//
// Usage is a little strange though, for some reason you can't use it directly in unity.
// You have to make a non-generic instance of it, and then use it. This is luckily quite easy:
//
// [System.Serializable]
// class MyDictionary : SerializableDictionary<KeyType, ValueType> {}
//
// Then make an instance of this like this:
//
// [SerializeField]
// private MyDictionary _dictionary = new MyDictionary();
//
// Now you can use it in exactly the same way as a normal Dictionary. Everything just works.

namespace vFrame.Core.Unity
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // We save the keys and values in two lists because Unity does understand those.
        [SerializeField, HideInInspector] private List<TKey> _keys;
        [SerializeField, HideInInspector] private List<TValue> _values;

        // Before the serialization we fill these lists
        public void OnBeforeSerialize() {
            _keys = new List<TKey>(Count);
            foreach (var kvp in this) {
                _keys.Add(kvp.Key);
            }
            _keys.Sort();

            _values = new List<TValue>(Count);
            foreach (var key in _keys) {
                _values.Add(this[key]);
            }
        }

        // After the serialization we create the dictionary from the two lists
        public void OnAfterDeserialize() {
            Clear();
            for (var i = 0; i != Mathf.Min(_keys.Count, _values.Count); i++) {
                Add(_keys[i], _values[i]);
            }
        }

        public SerializableDictionary() {

        }

        public SerializableDictionary(SerializableDictionary<TKey, TValue> other) {
            foreach (var item in other) {
                Add(item.Key, item.Value);
            }
        }
    }
}