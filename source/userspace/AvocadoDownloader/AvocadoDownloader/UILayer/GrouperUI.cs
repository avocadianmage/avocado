using AvocadoDownloader.BusinessLayer;
using StandardLibrary.Utilities;
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

            switch (e.Key)
            {
                case Key.Delete:
                    remove((Grouper)DataContext, WPF.IsShiftKeyDown);
                    break;
            }
        }

        void remove(Grouper grouper, bool deleteFromDisk)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            grouper.Remove(deleteFromDisk);
        }
    }
}