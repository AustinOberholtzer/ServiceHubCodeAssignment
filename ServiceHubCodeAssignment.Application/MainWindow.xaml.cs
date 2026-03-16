using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using ServiceHubCodeAssignment.Application.Models;
using ServiceHubCodeAssignment.Application.ViewModels;

namespace ServiceHubCodeAssignment.Application;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.SelectedProduct = e.NewValue as ProductInfo;
        }
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });

            e.Handled = true;
        }
        catch (Exception)
        {
            MessageBox.Show("Unable to open austinoberholtzer@gmail.com in your default email application.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
