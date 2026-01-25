using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.Infrastructure
{
    public interface INamedObject
    {
        string Name { get; set; }
    }

    public class NamedObjectCollection<TNamedObject> : ICollection<TNamedObject>
        where TNamedObject : INamedObject, new()
    {
        protected List<TNamedObject> elements = new();
        public int Count => elements.Count;

        public virtual TNamedObject Add(string name = "")
        {
            TNamedObject element = new();
            element.Name = name;
            this.Add(element);
            return element;
        }
        protected virtual void Add(TNamedObject item)
        {
            elements.Add(item);
        }
        public virtual bool Remove(TNamedObject item)
        {
            bool removed = elements.Remove(item);
            return removed;
        }
        public virtual void Clear()
        {
            elements.Clear();
        }
        public bool Contains(TNamedObject item)
        {
            return elements.Contains(item);
        }

        public IEnumerator<TNamedObject> GetEnumerator()
        {
            return elements.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)elements).GetEnumerator();
        }
        void ICollection<TNamedObject>.CopyTo(TNamedObject[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        void ICollection<TNamedObject>.Add(TNamedObject item) { Add(item); }
        bool ICollection<TNamedObject>.IsReadOnly => false;
    }
}
