using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpectrumAnalyzer.Helpers
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public event EventHandler ItemAdding;
        public event EventHandler ItemAdded;
        public event EventHandler ItemRemoving;
        public event EventHandler ItemRemoved;

        public delegate void EventHandler();

        public ObservableDictionary()
        {

        }

        public new void Add(TKey key, TValue value)
        {
            ItemAdding?.Invoke();
            base.Add(key, value);
            ItemAdded?.Invoke();
        }

        public new void Remove(TKey key)
        {
            ItemRemoving?.Invoke();
            base.Remove(key);
            ItemRemoved?.Invoke();
        }
    }
}
