using AvocadoFramework.Animation;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UtilityLib.WPF;

namespace AvocadoFramework.Controls.TextRendering
{
    public abstract class InputTextArea : TextArea
    {
        protected bool IsAltKeyDown
            => isModifierKeyDown(ModifierKeys.Alt);

        protected bool IsControlKeyDown
            => isModifierKeyDown(ModifierKeys.Control);

        protected bool IsShiftKeyDown
            => isModifierKeyDown(ModifierKeys.Shift);

        bool isModifierKeyDown(ModifierKeys key)
            => Keyboard.Modifiers.HasFlag(key);

        protected bool InputEnabled
        {
            get { return inputEnabled; }
            set { inputEnabled = value; }
        }

        bool inputEnabled = false;
        Border caret;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            initCaret();
            TextBase.Focus();
        }

        void initCaret()
        {
            caret = this.GetTemplateElement<Border>("Caret");
            caret.BorderBrush = new BrushAnimation().GetFadingBrush(
                Config.CaretBrush, Config.CursorBlinkDuration, true); 

            // Set size.
            var formattedText = new FormattedText(
                default(char).ToString(),
                CultureInfo.CurrentUICulture,
                TextBase.FlowDirection,
                new Typeface(
                    TextBase.FontFamily,
                    TextBase.FontStyle,
                    TextBase.FontWeight,
                    TextBase.FontStretch),
                TextBase.FontSize,
                TextBase.Foreground);
            caret.Width = formattedText.Width + 1;
            caret.Height = formattedText.Height + 1;

            // Hook events.
            TextBase.SelectionChanged += onSelectionChanged;
            TextBase.LostFocus += 
                (s, e) => caret.Visibility = Visibility.Collapsed;
            TextBase.GotFocus +=
                (s, e) => caret.Visibility = Visibility.Visible;
        }

        void onSelectionChanged(object sender, RoutedEventArgs e)
        {
            var caretLocation = TextBase.CaretPosition.GetCharacterRect(
                TextBase.CaretPosition.LogicalDirection);
            Canvas.SetLeft(caret, caretLocation.X);
            Canvas.SetTop(caret, caretLocation.Y);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Ignore input if the InputEnabled flag is false.
            // The exception to this is system key handling (ex: Alt+F4).
            if (!InputEnabled && e.Key != Key.System)
            {
                e.Handled = true;
                return;
            }
            // Handle any special key actions.
            HandleSpecialKeys(e);

            // Handle all other keys.
            base.OnPreviewKeyDown(e);
        }

        protected virtual void HandleSpecialKeys(KeyEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Key)
            {
                // Sanitize linebreak.
                case Key.Enter:
                    WriteLine();
                    e.Handled = true;
                    break;
                    
                // Disallow other styling when pasting.
                case Key.V:
                    if (!IsControlKeyDown) break;
                    var text = Clipboard.GetText(TextDataFormat.Text)
                        .Replace(Environment.NewLine, "\r");
                    Write(text, Foreground);
                    e.Handled = true;
                    break;
            }
        }
    }
}
