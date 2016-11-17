using AvocadoDownloader.BusinessLayer;
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
                    remove((Grouper)DataContext);
                    break;
            }
        }

        void remove(Grouper grouper)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            grouper.Remove();
        }
    }
}