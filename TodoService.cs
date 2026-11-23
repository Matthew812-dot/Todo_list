using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TodoList
{
    public class TodoService
    {
        private readonly List<TodoModel> _tasks = new List<TodoModel>();

        public ReadOnlyCollection<TodoModel> Tasks { get; }

        public TodoService()
        {
            Tasks = _tasks.AsReadOnly();
        }

        public TodoModel AddTask(
            string title,
            string description,
            DateTime? dueDate,
            TaskPriority priority)
        {
            TodoModel task = new TodoModel
            {
                Title = title,
                Description = description,
                DueDate = dueDate,
                Priority = priority
            };

            _tasks.Add(task);
            return task;
        }

        public void EditTask(
            TodoModel task,
            string title,
            string description,
            DateTime? dueDate,
            TaskPriority priority)
        {
            EnsureTaskExists(task);

            // Проверяем все новые значения до изменения исходной задачи.
            TodoModel updatedTask = new TodoModel
            {
                Title = title,
                Description = description,
                DueDate = dueDate,
                Priority = priority
            };

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.Priority = updatedTask.Priority;
        }

        public void DeleteTask(TodoModel task)
        {
            EnsureTaskExists(task);
            _tasks.Remove(task);
        }

        public void ChangeStatus(TodoModel task, TaskStatus status)
        {
            EnsureTaskExists(task);
            task.Status = status;
        }

        private void EnsureTaskExists(TodoModel task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (!_tasks.Contains(task))
            {
                throw new ArgumentException("Задача не найдена в списке.", nameof(task));
            }
        }
    }
}