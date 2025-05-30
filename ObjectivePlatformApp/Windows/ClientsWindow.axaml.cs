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
using System.Collections.Generic;

namespace ObjectivePlatformApp
{
    public partial class ClientsWindow : UserControl
    {
        private List<Clients> _allClients = new List<Clients>();

        public ClientsWindow()
        {
            InitializeComponent();
            var createButton = this.FindControl<Button>("CreateClientButton");
            createButton.Click += CreateClient_Click;

            var backButton = this.FindControl<Button>("BackButton");
            backButton.Click += BackButton_Click;

            LoadClients();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            (TopLevel.GetTopLevel(this) as Window)?.Close();
        }

        private void LoadClients()
        {
            using (var db = new AppDbContext())
            {
                _allClients = db.Clients.ToList();
                FilterClients();
            }
        }

        private void FilterClients(string searchText = "")
        {
            var clientsPanel = this.FindControl<StackPanel>("ClientsPanel");
            clientsPanel.Children.Clear();

            var filteredClients = string.IsNullOrWhiteSpace(searchText)
                ? _allClients
                : _allClients.Where(client =>
                    IsMatch(client.FirstName, searchText) ||
                    IsMatch(client.LastName, searchText) ||
                    IsMatch(client.MiddleName ?? "", searchText)).ToList();

            foreach (var client in filteredClients)
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTextBox = sender as TextBox;
            FilterClients(searchTextBox?.Text ?? "");
        }

        // Метод для проверки соответствия строки с использованием расстояния Левенштейна
        private bool IsMatch(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return false;
            if (string.IsNullOrEmpty(target)) return false;

            source = source.ToLower();
            target = target.ToLower();

            // Если есть точное совпадение (включая частичное)
            if (source.Contains(target) || target.Contains(source))
                return true;

            // Проверяем расстояние Левенштейна
            return LevenshteinDistance(source, target) <= 3;
        }

        // Алгоритм вычисления расстояния Левенштейна
        private int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        private void CreateClient_Click(object? sender, RoutedEventArgs e)
        {
            var newClient = new Clients();
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
            mainWindow.Content = new EditClient(newClient, true);
        }

        private void EditClient_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int clientId)
            {
                using (var db = new AppDbContext())
                {
                    var client = db.Clients.FirstOrDefault(c => c.Id == clientId);
                    if (client != null)
                    {
                        var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
                        mainWindow.Content = new EditClient(client);
                    }
                }
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
                        // Check if client has any demands or offers
                        bool hasDemands = db.Demands.Any(d => d.ClientId == clientId);
                        bool hasOffers = db.Offers.Any(o => o.ClientId == clientId);

                        if (hasDemands || hasOffers)
                        {
                            // Show error message
                            var errorWindow = new Window
                            {
                                Title = "Ошибка",
                                Content = new TextBlock
                                {
                                    Text = "Невозможно удалить клиента, так как он связан с потребностью или предложением.",
                                    Margin = new Thickness(20),
                                    TextWrapping = TextWrapping.Wrap
                                },
                                SizeToContent = SizeToContent.WidthAndHeight,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner
                            };
                            errorWindow.ShowDialog(TopLevel.GetTopLevel(this) as Window);
                            return;
                        }

                        db.Clients.Remove(client);
                        db.SaveChanges();
                        LoadClients();
                    }
                }
            }
        }
    }
}