using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;

namespace GenerationsPOS.Utilities
{
    /// <summary>
    /// Workaround for only updating a binding when a text box is exited: https://github.com/AvaloniaUI/Avalonia/issues/6071#issuecomment-861574988
    /// </summary>
    public class TBFocusBindingBehavior : Behavior<TextBox>
    {
        static TBFocusBindingBehavior()
        {
            TextProperty.Changed.Subscribe(e =>
            {
                ((TBFocusBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<TBFocusBindingBehavior, string>(
            "Text", defaultBindingMode: BindingMode.TwoWay);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                Text = AssociatedObject.Text;
            }
        }

        private void OnBindingValueChanged()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.Text = Text;
            }
        }
    }

    public class ACFocusBindingBehavior : Behavior<AutoCompleteBox>
    {
        static ACFocusBindingBehavior()
        {
            TextProperty.Changed.Subscribe(e =>
            {
                ((ACFocusBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<ACFocusBindingBehavior, string>(
            "Text", defaultBindingMode: BindingMode.TwoWay);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                Text = AssociatedObject.Text;
            }
        }

        private void OnBindingValueChanged()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.Text = Text;
            }
        }
    }

    public class UDFocusBindingBehavior : Behavior<NumericUpDown>
    {
        static UDFocusBindingBehavior()
        {
            ValueProperty.Changed.Subscribe(e =>
            {
                ((UDFocusBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<decimal?> ValueProperty = AvaloniaProperty.Register<UDFocusBindingBehavior, decimal?>(
            "Value", defaultBindingMode: BindingMode.TwoWay);

        public decimal? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                Value = AssociatedObject.Value;
            }
        }

        private void OnBindingValueChanged()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.Value = Value;
            }
        }
    }
}
