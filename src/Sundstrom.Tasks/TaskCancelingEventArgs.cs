namespace Sundstrom.Tasks
{
    public class TaskCancelingEventArgs : TaskEventArgsBase
    {
        internal TaskCancelingEventArgs(TaskInfo task, bool cancel)
            : base(task)
        {
            Cancel = cancel;
        }

        /// <summary>
        /// Gets or sets whether the queue should cancel or not.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
