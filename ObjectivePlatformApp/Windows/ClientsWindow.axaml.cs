using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using ObjectivePlatformApp.Data;
using ObjectivePlatformApp.Models;
using System.Linq;
using System;
using Avalonia.Media;

namespace ObjectivePlatformApp
{
    public partial class ClientsWindow : UserControl
    {
        public ClientsWindow()
        {
            InitializeComponent();
            var createButton = this.FindControl<Button>("CreateClientButton");
            createButton.Click += CreateClient_Click;

            LoadClients();
        }

        private void LoadClients()
        {
            using (var db = new AppDbContext())
            {
                var clients = db.Clients.ToList();
                var clientsPanel = this.FindControl<StackPanel>("ClientsPanel");

                clientsPanel.Children.Clear();

                foreach (var client in clients)
                {
                    var grid = new Grid
                    {
                        Margin = new Thickness(5)
                    };

                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));        
                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));      
                    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));        

                    var clientText = new TextBlock
                    {
                        Text = $"{client.Id} - {client.FirstName} {client.LastName} - {client.MiddleName} - {client.Email} - {client.Phone}",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    Grid.SetColumn(clientText, 0);

                    var editButton = new Button
                    {
                        Content = "Редактировать",
                        Margin = new Thickness(0, 0, 5, 0),
                        Tag = client.Id
                    };
                    editButton.Click += EditClient_Click;
                    Grid.SetColumn(editButton, 1);

                    var deleteButton = new Button
                    {
                        Content = "Удалить",
                        Tag = client.Id
                    };
                    deleteButton.Click += DeleteClient_Click;
                    Grid.SetColumn(deleteButton, 2);

                    grid.Children.Add(clientText);
                    grid.Children.Add(editButton);
                    grid.Children.Add(deleteButton);

                    clientsPanel.Children.Add(grid);
                }
            }
        }


        private void CreateClient_Click(object? sender, RoutedEventArgs e)
        {
            // Заглушка для создания клиента
            Console.WriteLine("Создание клиента");
        }

        private void EditClient_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int clientId)
            {
                // Заглушка для редактирования клиента
                Console.WriteLine($"Редактирование клиента с ID: {clientId}");
            }
        }

        private void DeleteClient_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int clientId)
            {
                using (var db = new AppDbContext())
                {
                    var client = db.Clients.FirstOrDefault(c => c.Id == clientId);
                    if (client != null)
                    {
                        db.Clients.Remove(client);
                        db.SaveChanges();
                        LoadClients(); // Обновить список
                    }
                }
            }
        }
    }
}
