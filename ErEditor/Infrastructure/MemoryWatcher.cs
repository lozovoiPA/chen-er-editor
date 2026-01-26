using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.Infrastructure
{
    // For Registry only 
    // converts notifications into three types registry accepts
    // Watcher может быть на несколько разных типов объектов, но каждый тип объекта - к своему реестру
    // шлюз эдакий для реестра
    public abstract class MemoryWatcher : IObserver, IObservable
    {
        protected ObserverBase notificationProcessor;
        protected ObservableBase observers = new();
        public bool BlockNotifying { get => observers.BlockNotifying; set => observers.BlockNotifying = value; }

        public MemoryWatcher()
        {
            notificationProcessor = new(this);
        }

        public virtual void Recieve(Notification notification) 
        {
            notificationProcessor.Recieve(notification);
        }

        public bool Subscribe(IObserver observer)
        {
            return observers.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observers.Unsubscribe(observer);
        }
    }

    public abstract class CollectionWatcher<TObject> : MemoryWatcher
        where TObject : notnull
    {
        public virtual void Visit(ObjectCreatedNotification<TObject> notif)
        {
            ConsoleLog.Log($"New {notif.Object.GetType()} was added", this, "INFO");
            observers.Notify(notif);
        }
        public virtual void Visit(ObjectDeletedNotification<TObject> notif)
        {
            ConsoleLog.Log($"{notif.Object.GetType()} was deleted", this, "INFO");
            observers.Notify(notif);
        }
    }
}
