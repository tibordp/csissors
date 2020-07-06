using System;

namespace Csissors.Tasks
{
    internal interface ITaskBuilder
    {
        ITask Build(IServiceProvider serviceProvider);
    }

}