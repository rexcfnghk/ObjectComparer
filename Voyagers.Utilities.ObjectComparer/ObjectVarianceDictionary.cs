using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Voyagers.Utilities.ObjectComparer
{
    public class ObjectVarianceDictionary : IDictionary<string, Tuple<object, object>>
    {
        private readonly IDictionary<string, Tuple<object, object>> _objectVarianceDictionary;

        public ObjectVarianceDictionary(IEnumerable<IObjectVariance> objectVariances)
        {
            _objectVarianceDictionary = objectVariances.ToDictionary(v => v.PropertyName, v => Tuple.Create(v.PropertyValue1, v.PropertyValue2));
        }

        public bool ContainsKey(string key)
        {
            return _objectVarianceDictionary.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _objectVarianceDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out Tuple<object, object> value)
        {
            return _objectVarianceDictionary.TryGetValue(key, out value);
        }

        public Tuple<object, object> this[string key]
        {
            get { return _objectVarianceDictionary[key]; }
            set { throw new NotSupportedException(); }
        }

        public ICollection<string> Keys
        {
            get { return _objectVarianceDictionary.Keys; }
        }

        public ICollection<Tuple<object, object>> Values
        {
            get { return _objectVarianceDictionary.Values; }
        }

        public IEnumerator<KeyValuePair<string, Tuple<object, object>>> GetEnumerator()
        {
            return _objectVarianceDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_objectVarianceDictionary).GetEnumerator();
        }

        public void Add(string propertyName, Tuple<object, object> propertyValueTuple)
        {
            Add(new KeyValuePair<string, Tuple<object, object>>(propertyName, propertyValueTuple));
        }

        public void Add(string propertyName, object propertyValue1, object propertyValue2)
        {
            Add(propertyName, Tuple.Create(propertyValue1, propertyValue2));
        }

        public void Add(KeyValuePair<string, Tuple<object, object>> item)
        {
            _objectVarianceDictionary.Add(item);
        }

        public void Clear()
        {
            _objectVarianceDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, Tuple<object, object>> item)
        {
            return _objectVarianceDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, Tuple<object, object>>[] array, int arrayIndex)
        {
            _objectVarianceDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, Tuple<object, object>> item)
        {
            return _objectVarianceDictionary.Remove(item);
        }

        public int Count
        {
            get { return _objectVarianceDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }
    }
}
