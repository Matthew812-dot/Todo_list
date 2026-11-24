using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace TodoList
{
    public partial class MainWindow : Window
    {
        private TodoService _service = new TodoService();
        private bool _hasUnsavedChanges;

        public MainWindow()
        {
            InitializeComponent();
            TasksGrid.ItemsSource = _service.Tasks;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Загрузить ранее сохранённые задачи из JSON-файла?",
                "Загрузка задач", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                LoadTasks();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            RunAction(() =>
            {
                TodoModel task = _service.AddTask(TitleTextBox.Text, DescriptionTextBox.Text,
                    DueDatePicker.SelectedDate, ReadPriority());
                RefreshTasks(task);
                _hasUnsavedChanges = true;
            });
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            RunAction(() =>
            {
                TodoModel task = GetSelectedTask();
                _service.EditTask(task, TitleTextBox.Text, DescriptionTextBox.Text,
                    DueDatePicker.SelectedDate, ReadPriority());
                RefreshTasks(task);
                _hasUnsavedChanges = true;
            });
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RunAction(() =>
            {
                TodoModel task = GetSelectedTask();
                if (MessageBox.Show(this, "Удалить выбранную задачу?", "Удаление",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                _service.DeleteTask(task);
                RefreshTasks(null);
                ClearFields();
                _hasUnsavedChanges = true;
            });
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            RunAction(() =>
            {
                TodoModel task = GetSelectedTask();
                TaskStatus status = (TaskStatus)Enum.Parse(typeof(TaskStatus),
                    (string)StatusComboBox.SelectedValue);
                _service.ChangeStatus(task, status);
                RefreshTasks(task);
                _hasUnsavedChanges = true;
            });
        }

        private void TasksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TodoModel task = TasksGrid.SelectedItem as TodoModel;
            if (task == null)
            {
                return;
            }

            TitleTextBox.Text = task.Title;
            DescriptionTextBox.Text = task.Description;
            DueDatePicker.SelectedDate = task.DueDate;
            PriorityComboBox.SelectedValue = task.Priority.ToString();
            StatusComboBox.SelectedValue = task.Status.ToString();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TasksGrid.SelectedItem = null;
            ClearFields();
        }

        private void ClearFields()
        {
            TitleTextBox.Clear();
            DescriptionTextBox.Clear();
            DueDatePicker.SelectedDate = null;
            PriorityComboBox.SelectedValue = TaskPriority.Medium.ToString();
            StatusComboBox.SelectedValue = TaskStatus.NotStarted.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTasks();
        }

        private bool SaveTasks()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "JSON-файлы (*.json)|*.json",
                FileName = "tasks.json",
                DefaultExt = ".json",
                AddExtension = true
            };
            if (dialog.ShowDialog(this) != true)
            {
                return false;
            }

            try
            {
                ITaskRepository repository = new JsonTaskRepository(dialog.FileName);
                repository.Save(_service.Tasks);
                _hasUnsavedChanges = false;
                return true;
            }
            catch (Exception exception)
            {
                ShowError(exception);
                return false;
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        private void LoadTasks()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "JSON-файлы (*.json)|*.json",
                CheckFileExists = true
            };
            if (dialog.ShowDialog(this) != true || !ConfirmUnsavedChanges())
            {
                return;
            }

            RunAction(() =>
            {
                ITaskRepository repository = new JsonTaskRepository(dialog.FileName);
                // Готовим новый список целиком, прежде чем заменять текущие задачи.
                TodoService loadedService = new TodoService();
                foreach (TodoModel savedTask in repository.Load())
                {
                    TodoModel task = loadedService.AddTask(savedTask.Title, savedTask.Description,
                        savedTask.DueDate, savedTask.Priority);
                    task.CreatedAt = savedTask.CreatedAt;
                    loadedService.ChangeStatus(task, savedTask.Status);
                }

                _service = loadedService;
                TasksGrid.ItemsSource = _service.Tasks;
                ClearFields();
                _hasUnsavedChanges = false;
            });
        }

        private bool ConfirmUnsavedChanges()
        {
            if (!_hasUnsavedChanges)
            {
                return true;
            }

            MessageBoxResult answer = MessageBox.Show(this,
                "Сохранить изменения в списке задач?", "Несохранённые изменения",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Yes)
            {
                return SaveTasks();
            }

            return answer == MessageBoxResult.No;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !ConfirmUnsavedChanges();
        }

        private TaskPriority ReadPriority()
        {
            return (TaskPriority)Enum.Parse(typeof(TaskPriority),
                (string)PriorityComboBox.SelectedValue);
        }

        private TodoModel GetSelectedTask()
        {
            TodoModel task = TasksGrid.SelectedItem as TodoModel;
            if (task == null)
            {
                throw new InvalidOperationException("Сначала выберите задачу в таблице.");
            }

            return task;
        }

        private void RefreshTasks(TodoModel selectedTask)
        {
            // Обычный List не уведомляет WPF об изменениях автоматически.
            TasksGrid.Items.Refresh();
            TasksGrid.SelectedItem = selectedTask;
        }

        private void RunAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private void ShowError(Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}