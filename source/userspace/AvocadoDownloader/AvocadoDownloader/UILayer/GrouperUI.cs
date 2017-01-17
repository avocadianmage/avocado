using AvocadoDownloader.BusinessLayer;
using StandardLibrary.WPF;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvocadoDownloader.UILayer
{
    class GrouperUI : ItemsControl
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!(e.OriginalSource is GrouperUI)) return;

            var grouper = (Grouper)DataContext;
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    grouper.Open();
                    break;

                case Key.Delete:
                    remove(grouper, WPFUtils.IsShiftKeyDown);
                    break;
            }
        }

        void remove(Grouper grouper, bool deleteFromDisk)
        {
            this.MoveNextFocus();
            grouper.Remove(deleteFromDisk);
        }
    }
}