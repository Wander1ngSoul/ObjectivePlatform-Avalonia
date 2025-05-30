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
    public partial class AgentsWindow : UserControl
    {
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
                var agents = db.Agents.ToList();
                var agentsPanel = this.FindControl<StackPanel>("AgentsPanel");

                agentsPanel.Children.Clear();

                foreach (var agent in agents)
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