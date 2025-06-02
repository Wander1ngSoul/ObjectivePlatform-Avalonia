using Avalonia;
using Avalonia.Controls;
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
    public partial class RealEstatesWindow : UserControl
    {
        private List<RealEstates> _allRealEstates = new List<RealEstates>();
        private List<Point> _searchPolygon = new List<Point>();

        public RealEstatesWindow()
        {
            InitializeComponent();
            var createButton = this.FindControl<Button>("CreateRealEstateButton");
            createButton.Click += CreateRealEstate_Click;

            var backButton = this.FindControl<Button>("BackButton");
            backButton.Click += BackButton_Click;

            LoadRealEstates();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            (TopLevel.GetTopLevel(this) as Window)?.Close();
        }

        private void LoadRealEstates()
        {
            using (var db = new AppDbContext())
            {
                _allRealEstates = db.RealEstates
                    .Include(re => re.RealEstateType)
                    .Include(re => re.District)
                    .ToList();
                FilterRealEstates();
            }
        }

        private static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t))
                return s.Length;

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

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

        private bool IsFuzzyMatch(string source, string target, int maxDistance)
        {
            if (string.IsNullOrEmpty(source))return false;
            if (string.IsNullOrEmpty(target)) return false;

            source = source.ToLower();
            target = target.ToLower();

            if (source.Contains(target) || target.Contains(source))
                return true;

            return LevenshteinDistance(source, target) <= maxDistance;
        }


        private void FilterRealEstates(string searchText = "")
        {
            var realEstatesPanel = this.FindControl<StackPanel>("RealEstatesPanel");
            realEstatesPanel.Children.Clear();

            var filteredRealEstates = string.IsNullOrWhiteSpace(searchText)
                ? _allRealEstates
                : _allRealEstates.Where(re =>
                    IsFuzzyMatch(re.City, searchText, 3) ||
                    IsFuzzyMatch(re.Street, searchText, 3) ||
                    IsFuzzyMatch(re.House?.ToString(), searchText, 1) ||
                    IsFuzzyMatch(re.Flat?.ToString(), searchText, 1)).ToList();

            foreach (var realEstate in filteredRealEstates)
            {
                var border = new Border
                {
                    Margin = new Thickness(0, 0, 0, 5),
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(5)
                };

                var grid = new Grid
                {
                    ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            }
                };

                var realEstateText = new TextBlock
                {
                    Text = $"{realEstate.Id} - {realEstate.City}, {realEstate.Street} {realEstate.House}/{realEstate.Flat} | " +
                           $"Тип: {realEstate.RealEstateType?.Name} | Район: {realEstate.District?.Name} | " +
                           $"Площадь: {realEstate.Area} м² | Комнат: {realEstate.Rooms} | Этаж: {realEstate.Floor}",
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(realEstateText, 0);

                var editButton = new Button
                {
                    Content = "Редактировать",
                    Margin = new Thickness(5, 0, 5, 0),
                    Tag = realEstate.Id
                };
                editButton.Click += EditRealEstate_Click;
                Grid.SetColumn(editButton, 1);

                var deleteButton = new Button
                {
                    Content = "Удалить",
                    Tag = realEstate.Id
                };
                deleteButton.Click += DeleteRealEstate_Click;
                Grid.SetColumn(deleteButton, 2);

                grid.Children.Add(realEstateText);
                grid.Children.Add(editButton);
                grid.Children.Add(deleteButton);

                border.Child = grid;
                realEstatesPanel.Children.Add(border);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTextBox = sender as TextBox;
            FilterRealEstates(searchTextBox?.Text ?? "");
        }

        public void SetSearchPolygon(List<Point> polygon)
        {
            _searchPolygon = polygon;
            FilterRealEstates();
        }

        private void CreateRealEstate_Click(object? sender, RoutedEventArgs e)
        {
            var newRealEstate = new RealEstates();
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
            mainWindow.Content = new EditRealEstatesWindow(newRealEstate, true);
        }

        private void EditRealEstate_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int realEstateId)
            {
                using (var db = new AppDbContext())
                {
                    var realEstate = db.RealEstates.FirstOrDefault(re => re.Id == realEstateId);
                    if (realEstate != null)
                    {
                        var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
                        mainWindow.Content = new EditRealEstatesWindow(realEstate);
                    }
                }
            }
        }

        private async void DeleteRealEstate_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int realEstateId)
            {
                // Создаем диалог подтверждения
                var confirmDialog = new Window
                {
                    Title = "Подтверждение удаления",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(10),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var textBlock = new TextBlock
                {
                    Text = "Вы уверены, что хотите удалить этот объект недвижимости?",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var buttonPanel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 10
                };

                var yesButton = new Button
                {
                    Content = "Да",
                    Width = 80
                };

                var noButton = new Button
                {
                    Content = "Нет",
                    Width = 80
                };

                yesButton.Click += async (s, args) =>
                {
                    using (var db = new AppDbContext())
                    {
                        var realEstate = db.RealEstates.FirstOrDefault(re => re.Id == realEstateId);
                        if (realEstate != null)
                        {
                            db.RealEstates.Remove(realEstate);
                            db.SaveChanges();
                            LoadRealEstates();
                        }
                    }
                    confirmDialog.Close();
                };

                noButton.Click += (s, args) =>
                {
                    confirmDialog.Close();
                };

                buttonPanel.Children.Add(yesButton);
                buttonPanel.Children.Add(noButton);

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(buttonPanel);

                confirmDialog.Content = stackPanel;

                var parentWindow = TopLevel.GetTopLevel(this) as Window;

                await confirmDialog.ShowDialog(parentWindow);
            }
        }
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}