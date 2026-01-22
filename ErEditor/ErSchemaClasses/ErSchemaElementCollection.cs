using ErEditor.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public class ErElementCollection<TErElement> : NamedObjectCollection<TErElement>
        where TErElement : class, IObservable, INamedObject, new()
    {
        public readonly ErElementWatcher<TErElement> Watcher;

        public ErElementCollection(ErElementWatcher<TErElement> watcher)
        {
            this.Watcher = watcher;
        }

        public override TErElement Add(string name = "")
        {
            TErElement element = new();
            element.Name = name;
            this.Add(element);
            return element;
        }
        protected override void Add(TErElement item)
        {
            elements.Add(item);
            Watcher.Visit(new ObjectCreatedNotification<TErElement>(item));
        }
        public override bool Remove(TErElement item)
        {
            bool removed = elements.Remove(item);
            Watcher.Visit(new ObjectDeletedNotification<TErElement>(item));
            return removed;
        }
    }
}
