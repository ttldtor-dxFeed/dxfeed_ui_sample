using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace dxFeedUISample.Collections
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return;

            _suppressNotification = true;

            var count = 0;

            foreach (var item in enumerable)
            {
                Add(item);
                count++;
            }

            _suppressNotification = false;

            if (count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0 || count <= 0 || index >= Count)
                return;

            _suppressNotification = true;


            if (Count < index + count)
                count = Count - index;

            for (var i = 0; i < count; i++)
            {
                RemoveAt(index);
            }

            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(int count)
        {
            if (Count <= count)
            {
                Clear();
            }

            RemoveRange(Count - count, count);
        }
    }
}