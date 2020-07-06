namespace Csissors.Tasks
{
    public interface IDynamicTask : ITask
    {
        ITask ParentTask { get; }
    }
}