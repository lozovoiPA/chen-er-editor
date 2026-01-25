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
            this.name = String.Empty;
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

    public class ErRole : ErElement
    {
        private ErEntitySet entitySet;

        private bool isIdDependency = false;
        private bool isKey = false;

        public ErRole(ErEntitySet entitySet, string name = "")
        {
            this.name = name;
            this.entitySet = entitySet;
        }

        // сделать проверку что схема та же
        public ErEntitySet EntitySet
        {
            get {  return entitySet; }
            set { entitySet = value; }
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
    }

    public class ErMapping : ErElement
    {
        private List<ErRole> preImage = new();
        private List<ErRole> image = new();

        // default: 1:M relationship
        private int minCardinalityOfImage = 0;
        private int maxCardinalityOfImage = -1;
        private int minCardinalityOfPreimage = 0;
        private int maxCardinalityOfPreimage = 1;

        public ErMapping() { }
        public ErMapping(string name)
        {
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
                maxCardinalityOfPreimage = value;
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
            observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
        }
        public void AddToImage(ErRole role)
        {
            image.Add(role);
            observers.Notify(new ObjectUpdatedNotification<ErMapping>(this));
        }
    }
}
