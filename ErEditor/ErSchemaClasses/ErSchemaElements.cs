using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public abstract class ErElement : IObservable
    {
        protected string name = String.Empty;
        protected readonly ObservableBase observableLogic = new();

        public virtual string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification<ErElement>(this, name)); }
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
            return newAttribute;
        }
    }

    public class ErEntitySet : ErElementWithAttributes
    {
        public ErEntitySet(string name = "")
        {
            this.name = name;
        }

        public override string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification<ErEntitySet>(this, name)); }
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

        public override string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification<ErRelationshipSet>(this, name)); }
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

        public override string Name
        {
            get { return name; }
            set { name = value; observableLogic.Notify(new ObjectNameChangedNotification<ErValueSet>(this, name)); }
        }
    }
}
