using System;

namespace Csissors.Tasks
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CsissorsTaskAttribute : Attribute
    {
        public string Name { get; set; }
        public string Schedule { get; set; }
        public string TimeZone { get; set; }
        public FailureMode FailureMode { get; set; }
        public ExecutionMode ExecutionMode { get; set; }
        public int Seconds { get; set; }
        public int Minutes { get; set; }
        public int Hours { get; set; }
        public int Days { get; set; }
        public bool Dynamic { get; set; }
        public bool FastForward { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromTaskDataAttribute : Attribute
    {
        public string Name { get; }
        public FromTaskDataAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}