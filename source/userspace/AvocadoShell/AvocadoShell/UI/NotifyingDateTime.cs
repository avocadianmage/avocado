using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace AvocadoShell.UI
{
    class NotifyingDateTime : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NotifyingDateTime()
        {
            var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += (s, e) => Now = DateTime.Now;
            timer.Start();
        }

        public DateTime Now
        {
            get { return _now; }
            private set
            {
                _now = value;
                PropertyChanged?.Invoke(
                    this, new PropertyChangedEventArgs(nameof(Now)));
            }
        }
        DateTime _now;
    }
}