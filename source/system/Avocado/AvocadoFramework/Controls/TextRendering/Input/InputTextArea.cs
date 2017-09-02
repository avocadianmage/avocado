using StandardLibrary.WPF;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvocadoFramework.Controls.TextRendering.Input
{
    public abstract class InputTextArea : TextArea
    {
        protected Border StylizedCaret { get; private set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            initCaret();
        }

        void initCaret()
        {
            StylizedCaret = this.GetTemplateElement<Border>("Caret");

            // Set size.
            StylizedCaret.Width = CharDimensions.Width + 1;
            StylizedCaret.Height = CharDimensions.Height + 1;

            // RichTextBox events.
            SelectionChanged += (s, e) => updateCaretLocation();
            TextChanged += (s, e) => updateCaretLocation();
            AddHandler(
                ScrollViewer.ScrollChangedEvent,
                new ScrollChangedEventHandler((s, e) => updateCaretLocation()));

            // Window events (Window.GetWindow returns null in design mode).
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var window = Window.GetWindow(this);
                window.Activated +=
                    (s, e) => StylizedCaret.Visibility = Visibility.Visible;
                window.Deactivated +=
                    (s, e) => StylizedCaret.Visibility = Visibility.Collapsed;
            }
        }

        void updateCaretLocation()
        {
            var caretRect = CaretPosition.GetCharacterRect(
                CaretPosition.LogicalDirection);
            Canvas.SetLeft(StylizedCaret, caretRect.X);
            Canvas.SetTop(StylizedCaret, caretRect.Y);
        }

        protected virtual void OnPaste()
        {
            // Disallow other styling when pasting.
            Selection.Text = Clipboard.GetText(TextDataFormat.Text);
            ClearSelection();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Esc: clear the currently selected text.
                case Key.Escape:
                    ClearSelection();
                    break;
            }

            base.OnPreviewKeyDown(e);
        }
    }
}
