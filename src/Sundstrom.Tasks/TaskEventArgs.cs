namespace Sundstrom.Tasks
{
    public class TaskEventArgs : TaskEventArgsBase
    {
        internal TaskEventArgs(TaskInfo task)
            : base(task)
        {
        }
    }
}
