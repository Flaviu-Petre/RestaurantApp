using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantApp.UI.Infrastructure
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public virtual void Cleanup()
        {
            // Clean up resources when the ViewModel is no longer needed
        }

        protected async Task RunCommandAsync(Func<Task> action, bool showBusyIndicator = true)
        {
            try
            {
                if (showBusyIndicator)
                    IsBusy = true;

                ErrorMessage = string.Empty;
                await action();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (showBusyIndicator)
                    IsBusy = false;
            }
        }
    }
}