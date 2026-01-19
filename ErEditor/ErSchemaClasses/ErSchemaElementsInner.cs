using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
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
        public ErMapping(string name)
        {
            this.name = name;
        }
    }
}
