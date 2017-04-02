using AvocadoFramework.Engine;
using StandardLibrary.Processes;
using StandardLibrary.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AvocadoSelector
{
    public partial class MainWindow : GlassPane
    {
        public ObservableCollection<Option> options { get; } 
            = new ObservableCollection<Option>();

        public MainWindow()
        {
            populateListFromCommandLine();
            InitializeComponent();
            DataContext = this;
            this.MoveNextFocus();
        }

        void populateListFromCommandLine()
        {
            var args = new Arguments();
            Resources.Add("SingleSelect", args.PopArg<bool>().Value);
            foreach (var arg in args.PopRemainingArgs())
            {
                options.Add(new Option(arg));
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                // Write out the selected options and close the window.
                case Key.Enter:
                    Console.WriteLine(getSelectedIndicesString());
                    Close();
                    break;
            }
        }

        IEnumerable<int> getSelectedIndices()
        {
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i].IsChecked) yield return i;
            }
        }

        string getSelectedIndicesString()
        {
            var arrayToJoin = getSelectedIndices()
                .Select(i => i.ToString())
                .ToArray();
            return string.Join(",", arrayToJoin);
        }
    }
}
