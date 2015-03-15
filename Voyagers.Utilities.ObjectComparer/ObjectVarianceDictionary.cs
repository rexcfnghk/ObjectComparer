using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Voyagers.Utilities.ObjectComparer
{
    public class ObjectVarianceDictionary
    {
        private readonly IDictionary<string, Tuple<object, object>> _objectVarianceDictionary;

        public ObjectVarianceDictionary(IEnumerable<IObjectVariance> objectVariances)
        {
            _objectVarianceDictionary = objectVariances.ToDictionary(v => v.PropertyName, v => Tuple.Create(v.PropertyValue1, v.PropertyValue2));
        }

        public IEnumerable<KeyValuePair<string, Tuple<object, object>>> Results
        {
            get { return _objectVarianceDictionary.ToList().AsReadOnly(); }
        }

        public void Add(KeyValuePair<string, Tuple<object, object>> keyValuePair)
        {
            _objectVarianceDictionary.Add(keyValuePair);
        }
    }
}
