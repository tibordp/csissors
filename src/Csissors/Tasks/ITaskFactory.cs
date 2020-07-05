using System;

namespace Csissors.Tasks
{
    internal interface ITaskFactory
    {
        ICsissorsTask Build(IServiceProvider serviceProvider);
    }

}