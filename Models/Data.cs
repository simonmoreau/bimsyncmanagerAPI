using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;

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

    public class SortedIfcElement
    {
        private string separator = ";";
        public string revisionId { get; set; }
        public long objectId { get; set; }
        public string ifcType { get; set; }
        public string type { get; set; }

        private bimsync.IfcElement IfcElement;

        List<Data.Property> dataProperties = new List<Property>();

        public SortedIfcElement(bimsync.IfcElement ifcElement)
        {
            this.IfcElement = ifcElement;
            this.ifcType = ifcElement.ifcType;
            this.revisionId = ifcElement.revisionId;
            this.objectId = ifcElement.objectId;
            this.type = "";

            if (ifcElement.type != null)
            {
                this.type = ifcElement.type.objectId.ToString();
            }

            dataProperties = GetIfcElementProperties();

            //GetIfcElementProperties(ifcElement);
            //propertyDefinitions = UpdatePropertyDefinitions(element.GetPropertyDefinitions(), propertyDefinitions);
        }

        public void AddToDataTable(DataTable metadataTable)
        {
            //Create a new row
            DataRow workRow = metadataTable.NewRow();
            //Add the properties to the row
            AddToRow(metadataTable, workRow, "ifcType", this.ifcType);
            AddToRow(metadataTable, workRow, "revisionId", this.revisionId);
            AddToRow(metadataTable, workRow, "objectId", this.objectId.ToString());
            AddToRow(metadataTable, workRow, "type", this.type);
            
            foreach (Data.Property property in dataProperties)
            {
                AddToRow(metadataTable, workRow, property.Name + " (" + property.Unit + ")", property.Value);
            }

            metadataTable.Rows.Add(workRow);
        }

        private List<Data.Property> GetIfcElementProperties()
        {
            List<Data.Property> dataProperties = new List<Data.Property>();

            //Loop on property sets
            JObject propertySets = IfcElement.propertySets as JObject;
            if (propertySets != null)
            {
                if (propertySets.Children().Count() != 0)
                {
                    dataProperties.AddRange(GetProperties(propertySets));
                }
            }

            //Loop on quantity sets
            JObject quantitySets = IfcElement.quantitySets as JObject;
            if (quantitySets != null)
            {
                if (quantitySets.Children().Count() != 0)
                {
                    dataProperties.AddRange(GetProperties(quantitySets));
                }
            }

            //Loop on attributes
            JObject attributes = IfcElement.attributes as JObject;
            if (attributes != null)
            {
                foreach (JToken jTokenattribute in attributes.Children())
                {
                    bimsync.AttributeProperty property = jTokenattribute.Children().First().ToObject<bimsync.AttributeProperty>();

                    property.Name = jTokenattribute.Path;

                    //Create a new data property
                    Data.Property dataProperty = new Data.Property();
                    dataProperty.Name = property.Name;
                    dataProperty.Unit = "string";
                    dataProperty.Value = (property.value == null) ? "" : property.value.ToString();
                    dataProperty.Type = property.ifcType;

                    dataProperties.Add(dataProperty);
                }
            }

            return dataProperties;
        }

        private List<Data.Property> GetProperties(JObject sets)
        {
            List<Data.Property> dataProperties = new List<Data.Property>();

            foreach (JToken jToken in sets.Children())
            {
                bimsync.Set set = jToken.Children().First().ToObject<bimsync.Set>();
                set.Name = jToken.Path;

                JObject properties = set.properties as JObject;
                if (properties == null) { properties = set.quantities as JObject; }

                if (properties != null)
                {
                    //Loop on properties
                    foreach (JToken jTokenProperty in properties.Children())
                    {
                        bimsync.Property property = jTokenProperty.Children().First().ToObject<bimsync.Property>();

                        property.Name = jTokenProperty.Path;

                        //Create a new data property
                        Data.Property dataProperty = new Data.Property();
                        dataProperty.Name = set.Name + "." + property.Name;
                        dataProperty.Type = property.ifcType;

                        bimsync.Value value = null;
                        if (property.value != null)
                        {
                            value = property.value;
                        }
                        else if (property.nominalValue != null)
                        {
                            value = property.nominalValue;
                        }

                        if (value != null)
                        {
                            dataProperty.Unit = value.unit;
                            dataProperty.Value = value.value;
                        }


                        dataProperties.Add(dataProperty);
                    }

                }
            }

            return dataProperties;
        }

        private void AddToRow(DataTable metadataTable, DataRow workRow, string property, string value)
        {
            if (metadataTable.Columns.Contains(property))
            {
                workRow[property] = value;
            }
            else
            {
                DataColumn workCol = metadataTable.Columns.Add(property, typeof(String));
                workRow[property] = value;
            }
        }

        private static DateTime ParseRequestDate(string value)
        {
            // https://stackoverflow.com/questions/2883576/how-do-you-convert-epoch-time-in-c

            CultureInfo provider = CultureInfo.InvariantCulture;
            string test = "2018:08:07 12:36:22+02:00" + "2018:07:20 17:04:57";
            string format1 = "yyyy:MM:dd HH:mm:ssK";
            string format2 = "yyyy:MM:dd HH:mm:ss";
            string format3 = "yyyy:MM:dd HH:mm:ss.ffffff";
            string format4 = "yyyy:MM:dd";

            DateTime date = new DateTime();

            // Scenario #1
            if (DateTime.TryParseExact(value, format1, provider, DateTimeStyles.None, out date))
                return date;

            // Scenario #2
            if (DateTime.TryParseExact(value, format2, provider, DateTimeStyles.None, out date))
                return date;

            // Scenario #3
            if (DateTime.TryParseExact(value, format3, provider, DateTimeStyles.None, out date))
                return date;

            // Scenario #4
            if (DateTime.TryParseExact(value, format4, provider, DateTimeStyles.None, out date))
                return date;

            if (DateTime.TryParse(value, out date))
                return date;

            throw new Exception("Don't know how to parse...");
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