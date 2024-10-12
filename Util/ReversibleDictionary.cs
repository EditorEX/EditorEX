using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorEX.Util
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ReversibleDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> keyToValue;
        private Dictionary<TValue, TKey> valueToKey;

        public ReversibleDictionary()
        {
            keyToValue = new Dictionary<TKey, TValue>();
            valueToKey = new Dictionary<TValue, TKey>();
        }

        public void Add(TKey key, TValue value)
        {
            if (keyToValue.ContainsKey(key) || valueToKey.ContainsKey(value))
                throw new ArgumentException("Key or value already exists in the dictionary.");

            keyToValue.Add(key, value);
            valueToKey.Add(value, key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return keyToValue.TryGetValue(key, out value);
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return valueToKey.TryGetValue(value, out key);
        }

        public TValue this[TKey key]
        {
            get => keyToValue[key];
            set
            {
                if (keyToValue.ContainsKey(key))
                {
                    // Remove the old value from valueToKey
                    var oldValue = keyToValue[key];
                    valueToKey.Remove(oldValue);
                }
                if (valueToKey.ContainsKey(value))
                {
                    // Remove the old key from keyToValue
                    var oldKey = valueToKey[value];
                    keyToValue.Remove(oldKey);
                }

                keyToValue[key] = value;
                valueToKey[value] = key;
            }
        }

        public TKey this[TValue newValue]
        {
            get => valueToKey[newValue];
            set
            {
                if (valueToKey.ContainsKey(newValue))
                {
                    // Remove the old key from keyToValue
                    var oldKey = valueToKey[newValue];
                    keyToValue.Remove(oldKey);
                }
                if (keyToValue.ContainsKey(value))
                {
                    // Remove the old value from valueToKey
                    var oldValue = keyToValue[value];
                    valueToKey.Remove(oldValue);
                }

                valueToKey[newValue] = value;
                keyToValue[value] = newValue;
            }
        }

        public int Count => keyToValue.Count;

        public void Clear()
        {
            keyToValue.Clear();
            valueToKey.Clear();
        }

        public bool Remove(TKey key)
        {
            if (keyToValue.TryGetValue(key, out TValue value))
            {
                keyToValue.Remove(key);
                valueToKey.Remove(value);
                return true;
            }
            return false;
        }

        public bool Remove(TValue value)
        {
            if (valueToKey.TryGetValue(value, out TKey key))
            {
                valueToKey.Remove(value);
                keyToValue.Remove(key);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return keyToValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return keyToValue.GetEnumerator();
        }

        public IEnumerable<TKey> Keys => keyToValue.Keys;
        public IEnumerable<TValue> Values => keyToValue.Values;
    }
}
