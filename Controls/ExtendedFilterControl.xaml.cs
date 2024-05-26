using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prova_WPF_PRISM_UserControl_ExtendedFilterControl.Controls;

/// <summary>
/// Logica di interazione per ExtendedFilterControl.xaml
/// </summary>
public partial class ExtendedFilterControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty CancelFilterButtonIsEnabledProperty = DependencyProperty.Register("CancelFilterButtonIsEnabled", typeof(bool), typeof(ExtendedFilterControl));
    public bool CancelFilterButtonIsEnabled
    {
        get => (bool)GetValue(CancelFilterButtonIsEnabledProperty);
        set
        {
            SetValue(CancelFilterButtonIsEnabledProperty, value);
            RaisePropertyChanged();
        }
    }

    public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(ExtendedFilterControl));
    public string WatermarkText
    {
        get => (string)GetValue(WatermarkTextProperty);
        set
        {
            SetValue(WatermarkTextProperty, value);
            RaisePropertyChanged();
        }
    }

    public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText", typeof(string), typeof(ExtendedFilterControl));
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set
        {
            SetValue(FilterTextProperty, value);
            RaisePropertyChanged();
        }
    }

    public static readonly DependencyProperty CancelFilterCommandProperty = DependencyProperty.Register("CancelFilterCommand", typeof(ICommand), typeof(ExtendedFilterControl));
    public ICommand CancelFilterCommand
    {
        get => (ICommand)GetValue(CancelFilterCommandProperty);
        set
        {
            SetValue(CancelFilterCommandProperty, value);
            RaisePropertyChanged();
        }
    }

    public static readonly DependencyProperty CancelFilterCommandParameterProperty = DependencyProperty.Register("CancelFilterCommandParameter", typeof(string), typeof(ExtendedFilterControl));
    public string CancelFilterCommandParameter
    {
        get => (string)GetValue(CancelFilterCommandParameterProperty);
        set
        {
            SetValue(CancelFilterCommandParameterProperty, value);
            RaisePropertyChanged();
        }
    }

    public ExtendedFilterControl()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void extFilterUserControl_Loaded(object sender, RoutedEventArgs e)
    {

    }
}

