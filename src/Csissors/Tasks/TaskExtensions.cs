namespace Csissors.Tasks
{
    public static class TaskExtensions
    {
        public static string GetCanonicalName(this ITask task)
        {
            string taskName = task.Name.Replace(":", "::");
            if (task.ParentTask == null)
            {
                return taskName;
            }
            else
            {
                return $"{task.ParentTask.GetCanonicalName()}:{taskName}";
            }
        }
        public static string GetCanonicalName(this IDynamicTask task)
        {
            return task.Name.Replace(":", "::");
        }
    }
}