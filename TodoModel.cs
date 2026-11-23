using System;

namespace TodoList
{
    public class TodoModel
    {
        private string _title = "Новая задача";
        private string _description = string.Empty;
        private DateTime _createdAt = DateTime.Now;
        private TaskPriority _priority = TaskPriority.Medium;
        private TaskStatus _status = TaskStatus.NotStarted;

        public string Title
        {
            get { return _title; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Название задачи не может быть пустым.", nameof(value));
                }

                _title = value.Trim();
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value ?? string.Empty; }
        }

        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set
            {
                if (value == default(DateTime))
                {
                    throw new ArgumentException("Дата создания должна быть указана.", nameof(value));
                }

                _createdAt = value;
            }
        }

        public DateTime? DueDate { get; set; }

        public TaskPriority Priority
        {
            get { return _priority; }
            set
            {
                if (!Enum.IsDefined(typeof(TaskPriority), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Неизвестный приоритет задачи.");
                }

                _priority = value;
            }
        }

        public TaskStatus Status
        {
            get { return _status; }
            set
            {
                if (!Enum.IsDefined(typeof(TaskStatus), value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Неизвестный статус задачи.");
                }

                _status = value;
            }
        }
    }
}