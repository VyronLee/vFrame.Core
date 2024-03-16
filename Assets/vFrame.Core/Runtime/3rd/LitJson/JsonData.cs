﻿#region Header

/**
 * JsonData.cs
 *   Generic type to hold JSON data (objects, arrays, and so on). This is
 *   the default type returned by JsonMapper.ToObject().
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace vFrame.Core.ThirdParty.LitJson
{
    public class JsonData : IJsonWrapper, IEquatable<JsonData>
    {
        public bool Equals(JsonData x) {
            if (x == null)
                return false;

            if (x.type != type) // further check to see if this is a long to int comparison
                if (x.type != JsonType.Int && x.type != JsonType.Long
                    || type != JsonType.Int && type != JsonType.Long)
                    return false;

            switch (type) {
                case JsonType.None:
                    return true;

                case JsonType.Object:
                    return inst_object.Equals(x.inst_object);

                case JsonType.Array:
                    return inst_array.Equals(x.inst_array);

                case JsonType.String:
                    return inst_string.Equals(x.inst_string);

                case JsonType.Int: {
                    if (x.IsLong) {
                        if (x.inst_long < int.MinValue || x.inst_long > int.MaxValue)
                            return false;
                        return inst_int.Equals((int) x.inst_long);
                    }

                    return inst_int.Equals(x.inst_int);
                }

                case JsonType.Long: {
                    if (x.IsInt) {
                        if (inst_long < int.MinValue || inst_long > int.MaxValue)
                            return false;
                        return x.inst_int.Equals((int) inst_long);
                    }

                    return inst_long.Equals(x.inst_long);
                }

                case JsonType.Double:
                    return inst_double.Equals(x.inst_double);

                case JsonType.Boolean:
                    return inst_boolean.Equals(x.inst_boolean);
            }

            return false;
        }


        #region IDictionary Indexer

        object IDictionary.this[object key] {
            get => EnsureDictionary()[key];

            set {
                if (!(key is string))
                    throw new ArgumentException(
                        "The key has to be a string");

                var data = ToJsonData(value);

                this[(string) key] = data;
            }
        }

        #endregion


        #region IOrderedDictionary Indexer

        object IOrderedDictionary.this[int idx] {
            get {
                EnsureDictionary();
                return object_list[idx].Value;
            }

            set {
                EnsureDictionary();
                var data = ToJsonData(value);

                var old_entry = object_list[idx];

                inst_object[old_entry.Key] = data;

                var entry =
                    new KeyValuePair<string, JsonData>(old_entry.Key, data);

                object_list[idx] = entry;
            }
        }

        #endregion


        #region IList Indexer

        object IList.this[int index] {
            get => EnsureList()[index];

            set {
                EnsureList();
                var data = ToJsonData(value);

                this[index] = data;
            }
        }

        #endregion


        #region ICollection Methods

        void ICollection.CopyTo(Array array, int index) {
            EnsureCollection().CopyTo(array, index);
        }

        #endregion


        #region IEnumerable Methods

        IEnumerator IEnumerable.GetEnumerator() {
            return EnsureCollection().GetEnumerator();
        }

        #endregion

        public JsonType GetJsonType() {
            return type;
        }

        public void SetJsonType(JsonType type) {
            if (this.type == type)
                return;

            switch (type) {
                case JsonType.None:
                    break;

                case JsonType.Object:
                    inst_object = new Dictionary<string, JsonData>();
                    object_list = new List<KeyValuePair<string, JsonData>>();
                    break;

                case JsonType.Array:
                    inst_array = new List<JsonData>();
                    break;

                case JsonType.String:
                    inst_string = default;
                    break;

                case JsonType.Int:
                    inst_int = default;
                    break;

                case JsonType.Long:
                    inst_long = default;
                    break;

                case JsonType.Double:
                    inst_double = default;
                    break;

                case JsonType.Boolean:
                    inst_boolean = default;
                    break;
            }

            this.type = type;
        }


        public int Add(object value) {
            var data = ToJsonData(value);

            json = null;

            return EnsureList().Add(data);
        }

        public bool Remove(object obj) {
            json = null;
            if (IsObject) {
                JsonData value = null;
                if (inst_object.TryGetValue((string) obj, out value))
                    return inst_object.Remove((string) obj) &&
                           object_list.Remove(new KeyValuePair<string, JsonData>((string) obj, value));
                throw new KeyNotFoundException("The specified key was not found in the JsonData object.");
            }

            if (IsArray) return inst_array.Remove(ToJsonData(obj));

            throw new InvalidOperationException(
                "Instance of JsonData is not an object or a list.");
        }

        public void Clear() {
            if (IsObject) {
                ((IDictionary) this).Clear();
                return;
            }

            if (IsArray) ((IList) this).Clear();
        }

        public string ToJson() {
            if (json != null)
                return json;

            var sw = new StringWriter();
            var writer = new JsonWriter(sw);
            writer.Validate = false;

            WriteJson(this, writer);
            json = sw.ToString();

            return json;
        }

        public void ToJson(JsonWriter writer) {
            var old_validate = writer.Validate;

            writer.Validate = false;

            WriteJson(this, writer);

            writer.Validate = old_validate;
        }

        public override string ToString() {
            switch (type) {
                case JsonType.Array:
                    return "JsonData array";

                case JsonType.Boolean:
                    return inst_boolean.ToString();

                case JsonType.Double:
                    return inst_double.ToString();

                case JsonType.Int:
                    return inst_int.ToString();

                case JsonType.Long:
                    return inst_long.ToString();

                case JsonType.Object:
                    return "JsonData object";

                case JsonType.String:
                    return inst_string;
            }

            return "Uninitialized JsonData";
        }

        #region Fields

        private IList<JsonData> inst_array;
        private bool inst_boolean;
        private double inst_double;
        private int inst_int;
        private long inst_long;
        private IDictionary<string, JsonData> inst_object;
        private string inst_string;
        private string json;
        private JsonType type;

        // Used to implement the IOrderedDictionary interface
        private IList<KeyValuePair<string, JsonData>> object_list;

        #endregion


        #region Properties

        public int Count => EnsureCollection().Count;

        public bool IsArray => type == JsonType.Array;

        public bool IsBoolean => type == JsonType.Boolean;

        public bool IsDouble => type == JsonType.Double;

        public bool IsInt => type == JsonType.Int;

        public bool IsLong => type == JsonType.Long;

        public bool IsObject => type == JsonType.Object;

        public bool IsString => type == JsonType.String;

        public ICollection<string> Keys {
            get {
                EnsureDictionary();
                return inst_object.Keys;
            }
        }

        /// <summary>
        ///     Determines whether the json contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the json.</param>
        /// <returns>true if the json contains an element that has the specified key; otherwise, false.</returns>
        public bool ContainsKey(string key) {
            EnsureDictionary();
            return inst_object.Keys.Contains(key);
        }

        #endregion


        #region ICollection Properties

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => EnsureCollection().IsSynchronized;

        object ICollection.SyncRoot => EnsureCollection().SyncRoot;

        #endregion


        #region IDictionary Properties

        bool IDictionary.IsFixedSize => EnsureDictionary().IsFixedSize;

        bool IDictionary.IsReadOnly => EnsureDictionary().IsReadOnly;

        ICollection IDictionary.Keys {
            get {
                EnsureDictionary();
                IList<string> keys = new List<string>();

                foreach (var entry in
                    object_list)
                    keys.Add(entry.Key);

                return (ICollection) keys;
            }
        }

        ICollection IDictionary.Values {
            get {
                EnsureDictionary();
                IList<JsonData> values = new List<JsonData>();

                foreach (var entry in
                    object_list)
                    values.Add(entry.Value);

                return (ICollection) values;
            }
        }

        #endregion


        #region IJsonWrapper Properties

        bool IJsonWrapper.IsArray => IsArray;

        bool IJsonWrapper.IsBoolean => IsBoolean;

        bool IJsonWrapper.IsDouble => IsDouble;

        bool IJsonWrapper.IsInt => IsInt;

        bool IJsonWrapper.IsLong => IsLong;

        bool IJsonWrapper.IsObject => IsObject;

        bool IJsonWrapper.IsString => IsString;

        #endregion


        #region IList Properties

        bool IList.IsFixedSize => EnsureList().IsFixedSize;

        bool IList.IsReadOnly => EnsureList().IsReadOnly;

        #endregion


        #region Public Indexers

        public JsonData this[string prop_name] {
            get {
                EnsureDictionary();
                return inst_object[prop_name];
            }

            set {
                EnsureDictionary();

                var entry =
                    new KeyValuePair<string, JsonData>(prop_name, value);

                if (inst_object.ContainsKey(prop_name)) {
                    for (var i = 0; i < object_list.Count; i++)
                        if (object_list[i].Key == prop_name) {
                            object_list[i] = entry;
                            break;
                        }
                }
                else {
                    object_list.Add(entry);
                }

                inst_object[prop_name] = value;

                json = null;
            }
        }

        public JsonData this[int index] {
            get {
                EnsureCollection();

                if (type == JsonType.Array)
                    return inst_array[index];

                return object_list[index].Value;
            }

            set {
                EnsureCollection();

                if (type == JsonType.Array) {
                    inst_array[index] = value;
                }
                else {
                    var entry = object_list[index];
                    var new_entry =
                        new KeyValuePair<string, JsonData>(entry.Key, value);

                    object_list[index] = new_entry;
                    inst_object[entry.Key] = value;
                }

                json = null;
            }
        }

        #endregion


        #region Constructors

        public JsonData() {
        }

        public JsonData(bool boolean) {
            type = JsonType.Boolean;
            inst_boolean = boolean;
        }

        public JsonData(double number) {
            type = JsonType.Double;
            inst_double = number;
        }

        public JsonData(int number) {
            type = JsonType.Int;
            inst_int = number;
        }

        public JsonData(long number) {
            type = JsonType.Long;
            inst_long = number;
        }

        public JsonData(object obj) {
            if (obj is bool) {
                type = JsonType.Boolean;
                inst_boolean = (bool) obj;
                return;
            }

            if (obj is double) {
                type = JsonType.Double;
                inst_double = (double) obj;
                return;
            }

            if (obj is int) {
                type = JsonType.Int;
                inst_int = (int) obj;
                return;
            }

            if (obj is long) {
                type = JsonType.Long;
                inst_long = (long) obj;
                return;
            }

            if (obj is string) {
                type = JsonType.String;
                inst_string = (string) obj;
                return;
            }

            throw new ArgumentException(
                "Unable to wrap the given object with JsonData");
        }

        public JsonData(string str) {
            type = JsonType.String;
            inst_string = str;
        }

        #endregion


        #region Implicit Conversions

        public static implicit operator JsonData(bool data) {
            return new JsonData(data);
        }

        public static implicit operator JsonData(double data) {
            return new JsonData(data);
        }

        public static implicit operator JsonData(int data) {
            return new JsonData(data);
        }

        public static implicit operator JsonData(long data) {
            return new JsonData(data);
        }

        public static implicit operator JsonData(string data) {
            return new JsonData(data);
        }

        #endregion


        #region Explicit Conversions

        public static explicit operator bool(JsonData data) {
            if (data.type != JsonType.Boolean)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");

            return data.inst_boolean;
        }

        public static explicit operator double(JsonData data) {
            if (data.type != JsonType.Double)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");

            return data.inst_double;
        }

        public static explicit operator int(JsonData data) {
            if (data.type != JsonType.Int && data.type != JsonType.Long)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold an int");

            // cast may truncate data... but that's up to the user to consider
            return data.type == JsonType.Int ? data.inst_int : (int) data.inst_long;
        }

        public static explicit operator long(JsonData data) {
            if (data.type != JsonType.Long && data.type != JsonType.Int)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a long");

            return data.type == JsonType.Long ? data.inst_long : data.inst_int;
        }

        public static explicit operator string(JsonData data) {
            if (data.type != JsonType.String)
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a string");

            return data.inst_string;
        }

        #endregion


        #region IDictionary Methods

        void IDictionary.Add(object key, object value) {
            var data = ToJsonData(value);

            EnsureDictionary().Add(key, data);

            var entry =
                new KeyValuePair<string, JsonData>((string) key, data);
            object_list.Add(entry);

            json = null;
        }

        void IDictionary.Clear() {
            EnsureDictionary().Clear();
            object_list.Clear();
            json = null;
        }

        bool IDictionary.Contains(object key) {
            return EnsureDictionary().Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return ((IOrderedDictionary) this).GetEnumerator();
        }

        void IDictionary.Remove(object key) {
            EnsureDictionary().Remove(key);

            for (var i = 0; i < object_list.Count; i++)
                if (object_list[i].Key == (string) key) {
                    object_list.RemoveAt(i);
                    break;
                }

            json = null;
        }

        #endregion


        #region IJsonWrapper Methods

        bool IJsonWrapper.GetBoolean() {
            if (type != JsonType.Boolean)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a boolean");

            return inst_boolean;
        }

        double IJsonWrapper.GetDouble() {
            if (type != JsonType.Double)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a double");

            return inst_double;
        }

        int IJsonWrapper.GetInt() {
            if (type != JsonType.Int)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold an int");

            return inst_int;
        }

        long IJsonWrapper.GetLong() {
            if (type != JsonType.Long)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a long");

            return inst_long;
        }

        string IJsonWrapper.GetString() {
            if (type != JsonType.String)
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a string");

            return inst_string;
        }

        void IJsonWrapper.SetBoolean(bool val) {
            type = JsonType.Boolean;
            inst_boolean = val;
            json = null;
        }

        void IJsonWrapper.SetDouble(double val) {
            type = JsonType.Double;
            inst_double = val;
            json = null;
        }

        void IJsonWrapper.SetInt(int val) {
            type = JsonType.Int;
            inst_int = val;
            json = null;
        }

        void IJsonWrapper.SetLong(long val) {
            type = JsonType.Long;
            inst_long = val;
            json = null;
        }

        void IJsonWrapper.SetString(string val) {
            type = JsonType.String;
            inst_string = val;
            json = null;
        }

        string IJsonWrapper.ToJson() {
            return ToJson();
        }

        void IJsonWrapper.ToJson(JsonWriter writer) {
            ToJson(writer);
        }

        #endregion


        #region IList Methods

        int IList.Add(object value) {
            return Add(value);
        }

        void IList.Clear() {
            EnsureList().Clear();
            json = null;
        }

        bool IList.Contains(object value) {
            return EnsureList().Contains(value);
        }

        int IList.IndexOf(object value) {
            return EnsureList().IndexOf(value);
        }

        void IList.Insert(int index, object value) {
            EnsureList().Insert(index, value);
            json = null;
        }

        void IList.Remove(object value) {
            EnsureList().Remove(value);
            json = null;
        }

        void IList.RemoveAt(int index) {
            EnsureList().RemoveAt(index);
            json = null;
        }

        #endregion


        #region IOrderedDictionary Methods

        IDictionaryEnumerator IOrderedDictionary.GetEnumerator() {
            EnsureDictionary();

            return new OrderedDictionaryEnumerator(
                object_list.GetEnumerator());
        }

        void IOrderedDictionary.Insert(int idx, object key, object value) {
            var property = (string) key;
            var data = ToJsonData(value);

            this[property] = data;

            var entry =
                new KeyValuePair<string, JsonData>(property, data);

            object_list.Insert(idx, entry);
        }

        void IOrderedDictionary.RemoveAt(int idx) {
            EnsureDictionary();

            inst_object.Remove(object_list[idx].Key);
            object_list.RemoveAt(idx);
        }

        #endregion


        #region Private Methods

        private ICollection EnsureCollection() {
            if (type == JsonType.Array)
                return (ICollection) inst_array;

            if (type == JsonType.Object)
                return (ICollection) inst_object;

            throw new InvalidOperationException(
                "The JsonData instance has to be initialized first");
        }

        private IDictionary EnsureDictionary() {
            if (type == JsonType.Object)
                return (IDictionary) inst_object;

            if (type != JsonType.None)
                throw new InvalidOperationException(
                    "Instance of JsonData is not a dictionary");

            type = JsonType.Object;
            inst_object = new Dictionary<string, JsonData>();
            object_list = new List<KeyValuePair<string, JsonData>>();

            return (IDictionary) inst_object;
        }

        private IList EnsureList() {
            if (type == JsonType.Array)
                return (IList) inst_array;

            if (type != JsonType.None)
                throw new InvalidOperationException(
                    "Instance of JsonData is not a list");

            type = JsonType.Array;
            inst_array = new List<JsonData>();

            return (IList) inst_array;
        }

        private JsonData ToJsonData(object obj) {
            if (obj == null)
                return null;

            if (obj is JsonData)
                return (JsonData) obj;

            return new JsonData(obj);
        }

        private static void WriteJson(IJsonWrapper obj, JsonWriter writer) {
            if (obj == null) {
                writer.Write(null);
                return;
            }

            if (obj.IsString) {
                writer.Write(obj.GetString());
                return;
            }

            if (obj.IsBoolean) {
                writer.Write(obj.GetBoolean());
                return;
            }

            if (obj.IsDouble) {
                writer.Write(obj.GetDouble());
                return;
            }

            if (obj.IsInt) {
                writer.Write(obj.GetInt());
                return;
            }

            if (obj.IsLong) {
                writer.Write(obj.GetLong());
                return;
            }

            if (obj.IsArray) {
                writer.WriteArrayStart();
                foreach (var elem in (IList) obj)
                    WriteJson((JsonData) elem, writer);
                writer.WriteArrayEnd();

                return;
            }

            if (obj.IsObject) {
                writer.WriteObjectStart();

                foreach (DictionaryEntry entry in (IDictionary) obj) {
                    writer.WritePropertyName((string) entry.Key);
                    WriteJson((JsonData) entry.Value, writer);
                }

                writer.WriteObjectEnd();
            }
        }

        #endregion
    }


    internal class OrderedDictionaryEnumerator : IDictionaryEnumerator
    {
        private readonly IEnumerator<KeyValuePair<string, JsonData>> list_enumerator;


        public OrderedDictionaryEnumerator(
            IEnumerator<KeyValuePair<string, JsonData>> enumerator) {
            list_enumerator = enumerator;
        }


        public object Current => Entry;

        public DictionaryEntry Entry {
            get {
                var curr = list_enumerator.Current;
                return new DictionaryEntry(curr.Key, curr.Value);
            }
        }

        public object Key => list_enumerator.Current.Key;

        public object Value => list_enumerator.Current.Value;


        public bool MoveNext() {
            return list_enumerator.MoveNext();
        }

        public void Reset() {
            list_enumerator.Reset();
        }
    }
}