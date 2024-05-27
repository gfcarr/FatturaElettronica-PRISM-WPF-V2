using System.Windows.Controls;

namespace FatturaElettronica_PRISM_WPF_V2.Views;
/// <summary>
/// Interaction logic for ContentView
/// </summary>
public partial class ContentView : UserControl
{
    public ContentView()
    {
        InitializeComponent();
    }
    public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var grid = e.Source as DataGrid;
        if (grid.SelectedItem != null)
        {
            grid.UpdateLayout();
            grid.ScrollIntoView(grid.SelectedItem, null);
        }

    }

    private void listBoxAnni_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}
