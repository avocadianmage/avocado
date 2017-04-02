using System.ComponentModel;

namespace AvocadoSelector
{
    public class Option : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (description == value) return;
                description = value;
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(IsChecked)));
            }
        }

        public Option(string description)
        {
            Description = description;
            IsChecked = false;
        }
    }
}