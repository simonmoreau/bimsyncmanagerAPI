using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace bimsyncManagerAPI.Data
{
    public class Property
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public PropertyDefinition PropertyDefinition
        {
            get
            {
                return new PropertyDefinition
                {
                    Unit = this.Unit,
                    Name = this.Name,
                    Type = this.Type
                };
            }
        }
    }

    public class PropertyDefinition
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
    }

    public class Element
    {
        private string separator = ";";
        public string revisionId { get; set; }
        public long objectId { get; set; }
        public string ifcType { get; set; }
        public string type { get; set; }
        public List<Data.Property> properties { get; set; }

        public string ToString(List<Data.PropertyDefinition> propertyDefinitions)
        {
            //private string header = "revisionId;objectId;ifcType;type;";
            string toString = "";

            toString = revisionId + separator
            + objectId + separator
            + ifcType + separator
            + type + separator
            + GetPropertiesAsString(propertyDefinitions);

            return toString;
        }

        private string GetPropertiesAsString(List<Data.PropertyDefinition> propertyDefinitions)
        {
            string toString = "";
            Data.PropertyDefinitionEqualityComparer comparer = new Data.PropertyDefinitionEqualityComparer();

            foreach (PropertyDefinition PropertyDefinition in propertyDefinitions )
            {
                //Get associated properties
                Property property = this.properties.Where(p => comparer.Equals(p.PropertyDefinition,PropertyDefinition)).FirstOrDefault();

                if (property != null)
                {
                    string value = string.IsNullOrEmpty(property.Value) ? "" : property.Value.Replace("\r\n","");
                    toString = toString + value + separator;
                }
                else
                {
                    toString = toString + separator;
                }
            }

            return toString;

        }

        public List<PropertyDefinition> GetPropertyDefinitions()
        {
            List<PropertyDefinition> propertyDefinitions = new List<PropertyDefinition>();

            foreach (Property property in this.properties)
            {
                propertyDefinitions.Add(property.PropertyDefinition);
            }

            return propertyDefinitions;
        }
    }

    public class Task
    {
        public Task<List<bimsync.IfcElement>> AsyncTask { get; set; }
        public string url { get; set; }
    }

    class PropertyDefinitionEqualityComparer : IEqualityComparer<PropertyDefinition>
{
    public bool Equals(PropertyDefinition x, PropertyDefinition y)
    {
        if (x == null || y == null)
            return false;

        return (x.Name == y.Name && x.Type == y.Type && x.Unit == y.Unit);
    }

    public int GetHashCode(PropertyDefinition obj)
    {
        return obj.GetHashCode();
    }
}
}