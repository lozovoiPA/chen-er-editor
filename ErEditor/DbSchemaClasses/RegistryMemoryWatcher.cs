using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{
    // For Registry only 
    // converts notifications into three types registry accepts
    // Watcher может быть на несколько разных типов объектов, но каждый тип объекта - к своему реестру
    // шлюз эдакий для реестра
    public abstract class RegistryMemoryWatcher : IObserver, IObservable
    {
        protected ObserverBase observerLogic;
        protected ObservableBase observableLogic = new();

        public RegistryMemoryWatcher()
        {
            observerLogic = new(this);
        }

        public abstract void Recieve(Notification notification);

        public bool Subscribe(IObserver observer)
        {
            return observableLogic.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observableLogic.Unsubscribe(observer);
        }
    }
}
