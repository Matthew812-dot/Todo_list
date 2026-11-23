using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoList
{
    public class JsonTaskRepository : ITaskRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonTaskRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));
            }

            _filePath = Path.GetFullPath(filePath);
            _options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            _options.Converters.Add(new JsonStringEnumConverter());
        }

        public void Save(IEnumerable<TodoModel> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            List<TodoModel> taskList = new List<TodoModel>(tasks);
            if (taskList.Contains(null))
            {
                throw new ArgumentException("Список не должен содержать null вместо задачи.", nameof(tasks));
            }

            string json = JsonSerializer.Serialize(taskList, _options);
            File.WriteAllText(_filePath, json);
        }

        public List<TodoModel> Load()
        {
            string json;
            try
            {
                json = File.ReadAllText(_filePath);
            }
            catch (FileNotFoundException)
            {
                // При первом запуске файл с задачами ещё может отсутствовать.
                return new List<TodoModel>();
            }

            List<TodoModel> tasks = JsonSerializer.Deserialize<List<TodoModel>>(json, _options);
            if (tasks == null || tasks.Contains(null))
            {
                throw new JsonException("Файл должен содержать список задач без элементов null.");
            }

            return tasks;
        }
    }
}