using com.dxfeed.api.data;

namespace dxFeedUISample.Models
{
    public class SubscriptionItem
    {
        public static SubscriptionItem InvalidItem = new SubscriptionItem(string.Empty, EventType.None, string.Empty);

        public SubscriptionItem(string symbol, EventType eventType, string eventSource)
        {
            Symbol = symbol;
            EventType = eventType;
            EventSource = eventSource;
        }

        public string Symbol { get; }
        public EventType EventType { get; }
        public string EventSource { get; }

        public static SubscriptionItem Create(string symbol, object eventType, string eventSource)
        {
            if (symbol.Trim().Length == 0 || !(eventType is EventType))
            {
                return InvalidItem;
            }

            var eventType2 = (EventType) eventType;

            if (eventType2 == EventType.None || eventType2 == EventType.Order && eventSource.Trim().Length == 0)
            {
                return InvalidItem;
            }

            return new SubscriptionItem(symbol.Trim(), eventType2, eventSource.Trim());
        }

        public override string ToString()
        {
            return $"{EventType}{(EventSource.Length == 0 ? "" : "#" + EventSource)}: '{Symbol}'";
        }

        public override int GetHashCode()
        {
            var result = Symbol.GetHashCode();

            result = 31 * result + (int) EventType;
            result = 31 * result + EventSource.GetHashCode();

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is SubscriptionItem subscriptionItem)
            {
                return Symbol.Equals(subscriptionItem.Symbol) && EventType == subscriptionItem.EventType &&
                       EventSource.Equals(subscriptionItem.EventSource);
            }

            return false;
        }
    }
}