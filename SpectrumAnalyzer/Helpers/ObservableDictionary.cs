using System.Collections.Generic;

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
