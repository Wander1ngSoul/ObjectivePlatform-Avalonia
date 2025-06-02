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
using Microsoft.EntityFrameworkCore;

namespace ObjectivePlatformApp
{
    public partial class AgentsWindow : UserControl
    {
        private List<Agents> _allAgents = new List<Agents>();

        public AgentsWindow()
        {
            InitializeComponent();
            var createButton = this.FindControl<Button>("CreateAgentButton");
            createButton.Click += CreateAgent_Click;

            var backButton = this.FindControl<Button>("BackButton");
            backButton.Click += BackButton_Click;

            LoadAgents();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            (TopLevel.GetTopLevel(this) as Window)?.Close();
        }

        private void LoadAgents()
        {
            using (var db = new AppDbContext())
            {
                _allAgents = db.Agents.ToList();
                FilterAgents();
            }
        }

        private void FilterAgents(string searchText = "")
        {
            var agentsPanel = this.FindControl<StackPanel>("AgentsPanel");
            agentsPanel.Children.Clear();

            var filteredAgents = string.IsNullOrWhiteSpace(searchText)
                ? _allAgents
                : _allAgents.Where(agent =>
                    IsMatch(agent.FirstName, searchText) ||
                    IsMatch(agent.LastName, searchText) ||
                    IsMatch(agent.MiddleName ?? "", searchText)).ToList();

            foreach (var agent in filteredAgents)
            {
                var grid = new Grid
                {
                    Margin = new Thickness(5)
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

                var agentText = new TextBlock
                {
                    Text = $"{agent.Id} - {agent.LastName} {agent.FirstName} {agent.MiddleName} - Комиссия: {agent.Commision}%",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(agentText, 0);

                var editButton = new Button
                {
                    Content = "Редактировать",
                    Margin = new Thickness(0, 0, 5, 0),
                    Tag = agent.Id
                };
                editButton.Click += EditAgent_Click;
                Grid.SetColumn(editButton, 1);

                var deleteButton = new Button
                {
                    Content = "Удалить",
                    Tag = agent.Id
                };
                deleteButton.Click += DeleteAgent_Click;
                Grid.SetColumn(deleteButton, 2);

                grid.Children.Add(agentText);
                grid.Children.Add(editButton);
                grid.Children.Add(deleteButton);

                agentsPanel.Children.Add(grid);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTextBox = sender as TextBox;
            FilterAgents(searchTextBox?.Text ?? "");
        }

        private bool IsMatch(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return false;
            if (string.IsNullOrEmpty(target)) return false;

            source = source.ToLower();
            target = target.ToLower();

            if (source.Contains(target) || target.Contains(source))
                return true;

            return LevenshteinDistance(source, target) <= 3;
        }

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

        private void CreateAgent_Click(object? sender, RoutedEventArgs e)
        {
            var newAgent = new Agents();
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
            mainWindow.Content = new EditAgent(newAgent, true);
        }

        private void EditAgent_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int agentId)
            {
                using (var db = new AppDbContext())
                {
                    var agent = db.Agents.FirstOrDefault(a => a.Id == agentId);
                    if (agent != null)
                    {
                        var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
                        mainWindow.Content = new EditAgent(agent);
                    }
                }
            }
        }

        private async void DeleteAgent_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int agentId)
            {
                // Создаем диалоговое окно подтверждения
                var confirmDialog = new Window
                {
                    Title = "Подтверждение удаления",
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    MinWidth = 300
                };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20)
                };

                using (var db = new AppDbContext())
                {
                    var agent = db.Agents
                        .Include(a => a.Demands)
                        .Include(a => a.Offers)
                        .FirstOrDefault(a => a.Id == agentId);

                    if (agent == null) return;

                    // Проверка на связанные записи
                    bool hasActiveOffers = db.Offers.Any(o => o.AgentId == agentId);
                    bool hasActiveDemands = db.Demands.Any(d => d.AgentId == agentId);
                    bool hasDeals = db.Deals.Any(d =>
                        d.Offer.AgentId == agentId ||
                        d.Demand.AgentId == agentId);

                    if (hasActiveOffers || hasActiveDemands || hasDeals)
                    {
                        stackPanel.Children.Add(new TextBlock
                        {
                            Text = "Невозможно удалить агента, так как он связан с активными предложениями, потребностями или сделками.",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 20),
                            FontSize = 16
                        });

                        var okButton = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Center };
                        okButton.Click += (s, args) => confirmDialog.Close();
                        stackPanel.Children.Add(okButton);
                        confirmDialog.Content = stackPanel;
                        await confirmDialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);
                        return;
                    }

                    // Добавляем текст подтверждения
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = $"Вы уверены, что хотите удалить агента:\n{agent.LastName} {agent.FirstName} {agent.MiddleName}?",
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 20),
                        FontSize = 16
                    });

                    // Добавляем кнопки подтверждения
                    var buttonsPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 10
                    };

                    var yesButton = new Button { Content = "Да" };
                    var noButton = new Button { Content = "Нет" };

                    yesButton.Click += async (s, args) =>
                    {
                        db.Agents.Remove(agent);
                        db.SaveChanges();
                        LoadAgents();
                        confirmDialog.Close();

                        // Показываем сообщение об успешном удалении
                        var successWindow = new Window
                        {
                            Title = "Успешно",
                            Content = new TextBlock
                            {
                                Text = $"Агент {agent.LastName} {agent.FirstName} успешно удален.",
                                Margin = new Thickness(20),
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 16
                            },
                            SizeToContent = SizeToContent.WidthAndHeight,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        await successWindow.ShowDialog(TopLevel.GetTopLevel(this) as Window);
                    };

                    noButton.Click += (s, args) =>
                    {
                        confirmDialog.Close();
                    };

                    buttonsPanel.Children.Add(yesButton);
                    buttonsPanel.Children.Add(noButton);
                    stackPanel.Children.Add(buttonsPanel);
                    confirmDialog.Content = stackPanel;

                    await confirmDialog.ShowDialog(TopLevel.GetTopLevel(this) as Window);
                }
            }
        }
    }
}