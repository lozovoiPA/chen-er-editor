using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public class ErAttribute : ErElement
    {
        public double? minValue;
        public double? maxValue;
        public string? allowedValues;
        public bool isKey;

        public List<ErValueSet> valueSets;

        public ErAttribute()
        {
            this.name = string.Empty;
            isKey = false;
            valueSets = new List<ErValueSet>();
        }

        public ErAttribute(string name)
        {
            this.name = name;
            isKey = false;
            valueSets = new List<ErValueSet>();
        }
    }

    public class ErRole 
        : ErElement,
        IObserver
    {
        private ErEntitySet entitySet;

        private bool isIdDependency = false;
        private bool isKey = false;

        private ObserverBase notificationParser;

        public ErRole(ErEntitySet entitySet, string name = "")
        {
            this.name = name;
            this.entitySet = entitySet;

            entitySet.Subscribe(this);
            notificationParser = new(this);
        }

        // сделать проверку что схема та же
        public ErEntitySet EntitySet
        {
            get {  return entitySet; }
            set { 
                if(entitySet != ErEntitySet.Empty)
                {
                    entitySet.Unsubscribe(this);
                }
                entitySet = value;
                if(value != ErEntitySet.Empty)
                {
                    value.Subscribe(this);
                }
                observers.Notify(new ObjectUpdatedNotification<ErRole>(this));
            }
        }
        public bool IsIdDependency
        {
            get { return isIdDependency; }
            set { isIdDependency = value; }
        }
        public bool IsKey
        {
            get { return isKey; }
            set { isKey = value; }
        }

        public void Recieve(Notification notification)
        {
            
        }
    }

    public class ErMapping : 
        ErElement,
        IObserver,
        IVisitor<ObjectNameChangedNotification>
    {
        private List<ErRole> preImage = new();
        private List<ErRole> image = new();

        // default: 1:M relationship
        private int minCardinalityOfImage = 0;
        private int maxCardinalityOfImage = -1;
        private int minCardinalityOfPreimage = 0;
        private int maxCardinalityOfPreimage = 1;

        private ObserverBase notificationParser;

        public override string Name
        {
            get 
            {
                return name;
            }
            set
            {
                base.Name = value;
            }
        }
        public string DefaultName
        {
            get { return this.GetDefaultName(); }
        }

        public ErMapping() {
            notificationParser = new(this);
        }
        public ErMapping(string name)
        {
            notificationParser = new(this);
            this.name = name;
        }

        public ReadOnlyCollection<ErRole> PreImage
        {
            get
            {
                return preImage.AsReadOnly();
            }
        }
        public ReadOnlyCollection<ErRole> Image
        {
            get
            {
                return image.AsReadOnly();
            }
        }
        public int MinCardinalityOfImage
        {
            get { return minCardinalityOfImage; }
            set
            {
                minCardinalityOfImage = value;
                observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
            }
        }
        public int MaxCardinalityOfImage
        {
            get { return maxCardinalityOfImage; }
            set
            {
                maxCardinalityOfImage = value;
                observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
            }
        }
        public int MinCardinalityOfPreimage
        {
            get { return minCardinalityOfPreimage; }
            set
            {
                minCardinalityOfPreimage = value;
                observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
            }
        }
        public int MaxCardinalityOfPreimage
        {
            get { return maxCardinalityOfPreimage; }
            set
            {
                maxCardinalityOfPreimage = value;
                observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
            }
        }

        public void AddToPreImage(ErRole role)
        {
            preImage.Add(role);

            ConsoleLog.Log("\n\nTESTING\n\n");
            role.Subscribe(this);
            observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
        }
        public void AddToImage(ErRole role)
        {
            image.Add(role);
            role.Subscribe(this);
            observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
        }
        // null means either the preimage or image now has no elements, therefore the mapping is invalid
        public bool? Remove(ErRole role)
        {
            bool removed = preImage.Remove(role) | image.Remove(role);
            if (preImage.Count == 0 || image.Count == 0) { 
                role.Unsubscribe(this); 
                return null; 
            }
            if (removed) { 
                role.Unsubscribe(this); 
                observers.Notify(new ObjectUpdatedNotification<ErMapping>(this)); 
            } 
            return removed;
        }
        public string GetDefaultName()
        {
            string output = GetPreImageName();
            output += " -> ";
            output += GetImageName();
            return output;
        }
        public string GetPreImageName()
        {
            string output = string.Empty;
            foreach (var role in preImage)
            {
                output += $"{role.Name} x ";
            }
            if(output.Length >= 3)
            {
                output = output.Remove(output.Length - 3);
            }
            else
            {
                ConsoleLog.Log("\n\n>>>>>>>>>>>>>>>>>>>>>SOMETHING WENT WRONG HERE<<<<<<<<<<<<<<<<<<\n\n");
            }

                return output;
        }
        public string GetImageName()
        {
            string output = string.Empty;
            foreach (var role in image)
            {
                output += $"{role.Name} x ";
            }
            if (output.Length > 3)
            {
                output = output.Remove(output.Length - 3);
            }

            return output;
        }

        public void Recieve(Notification notification)
        {
            //notificationParser.Recieve(notification);
            switch (notification)
            {
                case ObjectNameChangedNotification objectNameChangedNotification:
                    observers.Notify(new ObjectNameChangedNotification(this, this.Name));
                    break;
            }
        }
        public void Visit(ObjectNameChangedNotification notification)
        {
            
        }

        public override string ToString()
        {
            return (name == "" && preImage.Count > 0 && image.Count > 0) ? GetDefaultName() : name; 
        }
    }
}
