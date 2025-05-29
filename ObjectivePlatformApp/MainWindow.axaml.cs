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
                        MainContent.Content = new ClientsWindow();
                       
                        break;


                    default:
                        MainContent.Content = null; 
                        break;
                }
            }
        }
    }
}
