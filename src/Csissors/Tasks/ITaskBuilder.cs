using System;

namespace Csissors.Tasks
{
    internal interface ITaskBuilder
    {
        ITask BuildStatic(IServiceProvider serviceProvider);
        IDynamicTask BuildDynamic(IServiceProvider serviceProvider);
    }

}