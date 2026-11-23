using System.Collections.Generic;

namespace TodoList
{
    public interface ITaskRepository
    {
        void Save(IEnumerable<TodoModel> tasks);

        List<TodoModel> Load();
    }
}