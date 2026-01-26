using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.Infrastructure
{
    public interface IObservable
    {
        public bool Subscribe(IObserver observer);
        public bool Unsubscribe(IObserver observer);
    }
    public interface IObserver
    {
        public void Recieve(Notification notification);
    }

    public class ObservableBase : IObservable
    {
        protected List<IObserver> observers = new();

        public virtual bool Subscribe(IObserver observer)
        {
            ConsoleLog.Log("Observable Base received observer " + observer.ToString() + ", trying to subscribe");
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                ConsoleLog.Log("The observer was added.");
                return true;
            }
            ConsoleLog.Log("The observer wasn't added.");
            return false;
        }
        public virtual bool Unsubscribe(IObserver observer)
        {
            ConsoleLog.Log("An observer is unsubscribed.");
            if (observers.Contains(observer))
            {
                observers.Remove(observer);
                return true;
            }
            return false;
        }
        public virtual void Notify(Notification notification)
        {
            ConsoleLog.Log("I am GOING to notify observers, but they might not exist");
            ConsoleLog.Log("Total observers: " + observers.Count);
            foreach (var observer in observers)
            {
                ConsoleLog.Log("I am notifying observer " + observer.ToString());
                observer.Recieve(notification);
            }
        }
    }

    public class ObserverBase : IObserver
    {
        private NotificationVisitor notificationVisitorLogic;

        public ObserverBase(object wrappableVisitor)
        {
            notificationVisitorLogic = new(wrappableVisitor);
        }
        public void Recieve(Notification notification)
        {
            notification.Accept(notificationVisitorLogic);
        }
    }
}
