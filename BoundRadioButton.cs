using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Gramma.Windows
{
	/// <summary>
	/// Radio button with binding support, thanks to
	/// Aviad P. at http://stackoverflow.com/questions/1317891/simple-wpf-radiobutton-binding.
	/// </summary>
	public class BoundRadioButton : RadioButton
	{
		/// <summary>
		/// The value which should be matched to have radio check enabled.
		/// </summary>
    public object RadioValue
    {
        get { return (object)GetValue(RadioValueProperty); }
        set { SetValue(RadioValueProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RadioValue.
    //   This enables animation, styling, binding, etc...
		/// <summary>
		/// Dependency property for <see cref="RadioValue"/>.
		/// </summary>
    public static readonly DependencyProperty RadioValueProperty =
        DependencyProperty.Register(
            "RadioValue", 
            typeof(object), 
            typeof(BoundRadioButton), 
            new UIPropertyMetadata(null));

		/// <summary>
		/// The bound object.
		/// </summary>
    public object RadioBinding
    {
        get { return (object)GetValue(RadioBindingProperty); }
        set { SetValue(RadioBindingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RadioBinding.
    //   This enables animation, styling, binding, etc...
		/// <summary>
		/// Dependency property for <see cref="RadioBinding"/>.
		/// </summary>
    public static readonly DependencyProperty RadioBindingProperty =
        DependencyProperty.Register(
            "RadioBinding", 
            typeof(object), 
            typeof(BoundRadioButton), 
            new FrameworkPropertyMetadata(
                null, 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                OnRadioBindingChanged));

    private static void OnRadioBindingChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        BoundRadioButton rb = (BoundRadioButton)d;
        if (rb.RadioValue.Equals(e.NewValue))
            rb.SetCurrentValue(RadioButton.IsCheckedProperty, true);
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        SetCurrentValue(RadioBindingProperty, RadioValue);
    }
	}
}
