using System;

namespace Csissors.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CsissorsDynamicTaskAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}