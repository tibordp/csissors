using System;

namespace Csissors.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromTaskDataAttribute : Attribute
    {
        public string? Name { get; set; }
        public bool Optional { get; set; } = false;
    }
}