using System;
using System.Collections.Generic;


namespace bimsyncManagerAPI.bimsync
{
    public class Project
    {
        public DateTime createdAt { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class Model
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class User
    {
        public DateTime createdAt { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }

    public class Revision
    {
        public string comment { get; set; }
        public DateTime createdAt { get; set; }
        public string id { get; set; }
        public Model model { get; set; }
        public User user { get; set; }
        public int version { get; set; }
    }

    public class Member
    {
        public string role { get; set; }
        public User user { get; set; }
    }

    public class IfcElement
    {
        public string revisionId { get; set; }
        public long objectId { get; set; }
        public string ifcType { get; set; }
        public Attributes attributes { get; set; }
        public object type { get; set; }
        public object propertySets { get; set; }
        public object quantitySets { get; set; }
        public List<object> materials { get; set; }
    }

    public class Attributes
    {
        public GlobalId GlobalId { get; set; }
        public Name Name { get; set; }
        public ObjectType ObjectType { get; set; }
        public Tag Tag { get; set; }
        public PredefinedType PredefinedType { get; set; }
    }

    public class GlobalId
    {
        public string type { get; set; }
        public string ifcType { get; set; }
        public string value { get; set; }
    }

    public class Name
    {
        public string type { get; set; }
        public string ifcType { get; set; }
        public string value { get; set; }
    }

    public class ObjectType
    {
        public string type { get; set; }
        public string ifcType { get; set; }
        public string value { get; set; }
    }

    public class Tag
    {
        public string type { get; set; }
        public string ifcType { get; set; }
        public string value { get; set; }
    }

    public class PredefinedType
    {
        public string type { get; set; }
        public string ifcType { get; set; }
        public string value { get; set; }
    }

    public class PropertySets
    {
        public object PropertySetsList { get; set; }
    }

    public class QuantitySets
    {
        public object QuantitySetsList { get; set; }
    }
}