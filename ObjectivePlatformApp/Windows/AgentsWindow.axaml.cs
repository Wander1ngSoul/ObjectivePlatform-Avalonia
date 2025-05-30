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

        private void DeleteAgent_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int agentId)
            {
                using (var db = new AppDbContext())
                {
                    var agent = db.Agents.FirstOrDefault(a => a.Id == agentId);
                    if (agent != null)
                    {
                        db.Agents.Remove(agent);
                        db.SaveChanges();
                        LoadAgents();
                    }
                }
            }
        }
    }
}