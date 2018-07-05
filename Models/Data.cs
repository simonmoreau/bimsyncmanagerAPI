using System;
using System.Collections.Generic;


namespace bimsyncManagerAPI.Data
{
    public class Property
    {
        public string Name {get;set;}
        public string Unit {get;set;}
        public string Type {get;set;}
        public string Value {get;set;}
    }

        public class Element
    {
        public string revisionId { get; set; }
        public long objectId { get; set; }
        public string ifcType { get; set; }
        public string type { get; set; }
        public List<Data.Property> properties {get;set;}
    }
}