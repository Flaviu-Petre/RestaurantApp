using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RestaurantApp.UI.Infrastructure
{
    public interface IMessageBus
    {
        void Subscribe<T>(object subscriber, Action<T> action);
        void Unsubscribe<T>(object subscriber);
        void Publish<T>(T message);
    }

    public class MessageBus : IMessageBus
    {
        private readonly Dictionary<Type, Dictionary<object, List<object>>> _subscriptions =
            new Dictionary<Type, Dictionary<object, List<object>>>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Subscribe<T>(object subscriber, Action<T> action)
        {
            _lock.EnterWriteLock();
            try
            {
                Type messageType = typeof(T);
                if (!_subscriptions.TryGetValue(messageType, out Dictionary<object, List<object>> subscribers))
                {
                    subscribers = new Dictionary<object, List<object>>();
                    _subscriptions[messageType] = subscribers;
                }

                if (!subscribers.TryGetValue(subscriber, out List<object> actions))
                {
                    actions = new List<object>();
                    subscribers[subscriber] = actions;
                }

                actions.Add(action);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Unsubscribe<T>(object subscriber)
        {
            _lock.EnterWriteLock();
            try
            {
                Type messageType = typeof(T);
                if (_subscriptions.TryGetValue(messageType, out Dictionary<object, List<object>> subscribers))
                {
                    if (subscribers.ContainsKey(subscriber))
                    {
                        subscribers.Remove(subscriber);
                    }

                    if (subscribers.Count == 0)
                    {
                        _subscriptions.Remove(messageType);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Publish<T>(T message)
        {
            Dictionary<object, Action<T>> subscribersToNotify = new Dictionary<object, Action<T>>();

            _lock.EnterReadLock();
            try
            {
                Type messageType = typeof(T);
                if (_subscriptions.TryGetValue(messageType, out Dictionary<object, List<object>> subscribers))
                {
                    foreach (var subscriberPair in subscribers)
                    {
                        foreach (var action in subscriberPair.Value.OfType<Action<T>>())
                        {
                            subscribersToNotify[subscriberPair.Key] = action;
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            foreach (var action in subscribersToNotify.Values)
            {
                action(message);
            }
        }
    }

    // Message classes
    public class RefreshDataMessage { }

    public class OrderStatusChangedMessage
    {
        public int OrderId { get; set; }
        public string NewStatus { get; set; }
    }

    public class UserLoggedInMessage
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class UserLoggedOutMessage { }
}