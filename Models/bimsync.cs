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
        public object attributes { get; set; }
        public IfcElement type { get; set; }
        public object propertySets { get; set; }
        public object quantitySets { get; set; }
        public List<object> materials { get; set; }
    }

    public class Set
    {
        public string Name { get; set; }
        public string revisionId { get; set; }
        public long? objectId { get; set; }
        public string ifcType { get; set; }
        public object attributes { get; set; }
        public object properties { get; set; }
        public object quantities { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string description { get; set; }
        public string ifcType { get; set; }
        public Value value { get; set; }
        public Value nominalValue { get; set; }
    }

        public class AttributeProperty
    {
        public string Name { get; set; }
        public string type { get; set; }
        public string ifcType { get; set; }
        public object value { get; set; }
    }

    public class Value
    {
        public string type { get; set; }
        public string ifcType { get; set; }
        public string value { get; set; }
        public string unit { get; set; }
    }

}