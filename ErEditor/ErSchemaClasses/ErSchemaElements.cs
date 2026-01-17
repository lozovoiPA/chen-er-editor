using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public interface IErElement : IObservable { }
    public abstract class ErElement : IErElement
    {
        protected string name = String.Empty;
        protected readonly ObservableBase observableLogic = new();

        public virtual string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification(this, name)); }
        }

        public bool Subscribe(IObserver observer)
        {
            return observableLogic.Subscribe(observer);
        }
        public bool Unsubscribe(IObserver observer)
        {
            return observableLogic.Unsubscribe(observer);
        }

        public override string ToString()
        {
            return name;
        }
    }

    public abstract class ErElementWithAttributes : ErElement
    {
        public List<ErAttribute> attributes = new();

        public ErAttribute AddAttribute(string name = "")
        {
            ErAttribute newAttribute = new(name);
            attributes.Add(newAttribute);

            observableLogic.Notify(new ObjectAddedToCompositeObject<ErAttribute, ErElementWithAttributes>(newAttribute, this));
            return newAttribute;
        }

        public void AddAttributeRange(IEnumerable<ErAttribute> range)
        {
            foreach(var attr in range)
            {
                this.AddAttribute(attr.Name);
            }
        }
    }

    public class ErEntitySet : ErElementWithAttributes
    {
        public ErEntitySet(string name = "")
        {
            this.name = name;
        }
    }

    public class ErRelationshipSet : ErElementWithAttributes
    {
        public List<ErRole> roles = new();
        public List<ErMapping> mappings = new();

        public ErRelationshipSet(string name = "")
        {
            this.name = name;
        }

        public ErRole AddRole(string name = "")
        {
            ErRole newRole = new(name);
            roles.Add(newRole);
            return newRole;
        }
        public ErMapping AddMapping(string name = "")
        {
            ErMapping newMapping = new(name);
            mappings.Add(newMapping);
            return newMapping;
        }
        public bool RemoveRole(ErRole role)
        {
            if (roles.Contains(role))
            {
                roles.Remove(role);
                return true;
            }
            return false;
        }
    }

    public class ErValueSet : ErElement
    {
        public ErValueSet(string name = "")
        {
            this.name = name;
        }
    }
}
