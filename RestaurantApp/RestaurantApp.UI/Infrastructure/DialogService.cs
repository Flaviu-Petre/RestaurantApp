using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantApp.UI.Infrastructure
{
    public interface IDialogService
    {
        MessageBoxResult ShowMessage(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
        Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
        bool ShowOpenFileDialog(string filter, out string filePath);
        bool ShowSaveFileDialog(string filter, string defaultFileName, out string filePath);
    }

    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowMessage(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }

        public Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return Task.FromResult(MessageBox.Show(message, title, buttons, icon));
        }

        public bool ShowOpenFileDialog(string filter, out string filePath)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter
            };

            bool? result = dialog.ShowDialog();
            filePath = result == true ? dialog.FileName : null;
            return result == true;
        }

        public bool ShowSaveFileDialog(string filter, string defaultFileName, out string filePath)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName
            };

            bool? result = dialog.ShowDialog();
            filePath = result == true ? dialog.FileName : null;
            return result == true;
        }
    }
}