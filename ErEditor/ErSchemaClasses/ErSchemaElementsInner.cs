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
        public ErEntitySet? entitySet;
        public int? minCardinalityWhenImage;
        public int? maxCardinalityWhenImage;
        public int? minCardinalityWhenPreimage;
        public int? maxCardinalityWhenPreimage;

        public bool isIdDependency;
        public bool isKey;

        public ErRole(string name = "", ErEntitySet? entitySet = null)
        {
            this.name = name;
            isIdDependency = false;
            isKey = false;
            this.entitySet = entitySet;
        }

        public void AddEntitySet(ErEntitySet _entitySet)
        {
            entitySet = _entitySet;
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
