using Csissors.Tasks;
using System;

namespace Csissors.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CsissorsTaskAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Schedule { get; set; }
        public string? TimeZone { get; set; }
        public FailureMode FailureMode { get; set; }
        public ExecutionMode ExecutionMode { get; set; }
        public int Seconds { get; set; }
        public int Minutes { get; set; }
        public int Hours { get; set; }
        public int Days { get; set; }
        public bool FastForward { get; set; }
    }
}