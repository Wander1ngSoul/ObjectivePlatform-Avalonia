using Avalonia.Controls;
using Avalonia.Interactivity;
using ObjectivePlatformApp;

namespace ObjectivePlatformApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            NavigateButton.Click += NavigateButton_Click;
        }

        private void NavigateButton_Click(object? sender, RoutedEventArgs e)
        {
            if (TablesComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selected = selectedItem.Content?.ToString();

                switch (selected)
                {
                    case "Клиенты":
                        var mainWindowClients = (MainWindow)TopLevel.GetTopLevel(this);
                        mainWindowClients.Content = new ClientsWindow();
                        break;

                    case "Агенты":
                        var mainWindowAgents = (MainWindow)TopLevel.GetTopLevel(this);
                        mainWindowAgents.Content = new AgentsWindow();
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
