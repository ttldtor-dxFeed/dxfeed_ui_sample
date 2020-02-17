using System;
using System.Collections.Generic;
using com.dxfeed.api;
using com.dxfeed.api.connection;
using com.dxfeed.api.data;
using com.dxfeed.api.events;
using com.dxfeed.native;
using dxFeedUISample.Models;

namespace dxFeedUISample.Services
{
    public class ConnectionService : IDisposable
    {
        private IDxConnection? _connection;
        private readonly object _connectionLock = new object();

        private readonly Dictionary<EventType, IDxSubscription> _subscriptions =
            new Dictionary<EventType, IDxSubscription>();

        private readonly object _subscriptionsLock = new object();

        private readonly Dictionary<OrderSource, IDxSubscription> _orderSubscriptions =
            new Dictionary<OrderSource, IDxSubscription>();

        private readonly object _orderSubscriptionsLock = new object();

        private readonly HashSet<SubscriptionItem> _subscriptionItems = new HashSet<SubscriptionItem>();

        private readonly object _subscriptionItemsLock = new object();

        public bool Connected { get; private set; }

        public void Connect(string address, Action<IDxConnection> disconnectListener,
            Action<IDxConnection, ConnectionStatus, ConnectionStatus> connectionStatusListener)
        {
            Connected = false;

            lock (_connectionLock)
            {
                if (_connection != null)
                {
                    return;
                }

                _connection = new NativeConnection(address, disconnectListener, (connection, oldStatus, newStatus) =>
                {
                    Connected = newStatus == ConnectionStatus.Authorized;

                    connectionStatusListener(connection, oldStatus, newStatus);
                });
            }
        }

        public bool Subscribe(SubscriptionItem item, IDxEventListener listener)
        {
            if (!Connected || item.Equals(SubscriptionItem.InvalidItem) || item.EventType == EventType.None)
            {
                return false;
            }

            lock (_subscriptionItemsLock)
            {
                if (_subscriptionItems.Contains(item))
                {
                    return false;
                }
            }

            if (item.EventType == EventType.Order)
            {
                lock (_orderSubscriptionsLock)
                {
                    var source = OrderSource.ValueOf(item.EventSource);

                    if (!_orderSubscriptions.TryGetValue(source, out var subscription))
                    {
                        subscription = _connection.CreateSubscription(item.EventType, listener);
                        subscription.AddSource(source.Name);
                        _orderSubscriptions.Add(source, subscription);
                    }

                    subscription.AddSymbol(item.Symbol);
                }
            }
            else
            {
                lock (_subscriptionsLock)
                {
                    if (!_subscriptions.TryGetValue(item.EventType, out var subscription))
                    {
                        subscription = _connection.CreateSubscription(item.EventType, listener);
                        _subscriptions.Add(item.EventType, subscription);
                    }

                    subscription.AddSymbol(item.Symbol);
                }
            }

            lock (_subscriptionItemsLock)
            {
                _subscriptionItems.Add(item);
            }

            return true;
        }

        public bool Unsubscribe(SubscriptionItem item)
        {
            if (!Connected || item.Equals(SubscriptionItem.InvalidItem) || item.EventType == EventType.None)
            {
                return false;
            }

            lock (_subscriptionItemsLock)
            {
                if (!_subscriptionItems.Contains(item))
                {
                    return false;
                }
            }

            if (item.EventType == EventType.Order)
            {
                lock (_orderSubscriptionsLock)
                {
                    var source = OrderSource.ValueOf(item.EventSource);

                    if (_orderSubscriptions.TryGetValue(source, out var subscription))
                    {
                        subscription.RemoveSymbols(item.Symbol);
                    }
                }
            }
            else
            {
                lock (_subscriptionsLock)
                {
                    if (_subscriptions.TryGetValue(item.EventType, out var subscription))
                    {
                        subscription.RemoveSymbols(item.Symbol);
                    }
                }
            }

            lock (_subscriptionItemsLock)
            {
                _subscriptionItems.Remove(item);
            }

            return true;
        }

        public void Dispose()
        {
            lock (_subscriptionsLock)
            {
                _subscriptions.Clear();
            }

            lock (_orderSubscriptionsLock)
            {
                _orderSubscriptions.Clear();
            }

            lock (_connectionLock)
            {
                _connection?.Dispose();
            }
        }
    }
}