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
    public partial class RealEstatesWindow : UserControl
    {
        private List<RealEstates> _allRealEstates = new List<RealEstates>();

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

        private void FilterRealEstates(string searchText = "")
        {
            var realEstatesPanel = this.FindControl<StackPanel>("RealEstatesPanel");
            realEstatesPanel.Children.Clear();

            var filteredRealEstates = string.IsNullOrWhiteSpace(searchText)
                ? _allRealEstates
                : _allRealEstates.Where(re =>
                    IsMatch(re.City, searchText) ||
                    IsMatch(re.Street, searchText) ||
                    IsMatch(re.House.ToString(), searchText) ||
                    IsMatch(re.Flat.ToString(), searchText)).ToList();

            foreach (var realEstate in filteredRealEstates)
            {
                var grid = new Grid
                {
                    Margin = new Thickness(5)
                };

                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

                var realEstateText = new TextBlock
                {
                    Text = $"{realEstate.Id} - {realEstate.City}, {realEstate.Street} {realEstate.House}/{realEstate.Flat} | " +
                           $"Тип: {realEstate.RealEstateType?.Name} | Район: {realEstate.District?.Name} | " +
                           $"Этаж: {realEstate.Floor}, Комнат: {realEstate.Rooms}, Площадь: {realEstate.Area}м²",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(realEstateText, 0);

                var editButton = new Button
                {
                    Content = "Редактировать",
                    Margin = new Thickness(0, 0, 5, 0),
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

                realEstatesPanel.Children.Add(grid);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTextBox = sender as TextBox;
            FilterRealEstates(searchTextBox?.Text ?? "");
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
                    var realEstate = db.RealEstates
                        .Include(re => re.RealEstateType)
                        .Include(re => re.District)
                        .FirstOrDefault(re => re.Id == realEstateId);

                    if (realEstate != null)
                    {
                        var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
                        mainWindow.Content = new EditRealEstatesWindow(realEstate);
                    }
                }
            }
        }

        private void DeleteRealEstate_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int realEstateId)
            {
                using (var db = new AppDbContext())
                {
                    var realEstate = db.RealEstates.FirstOrDefault(re => re.Id == realEstateId);
                    if (realEstate != null)
                    {
                        bool hasDemands = db.Demands.Any(d => d.Id == realEstateId);
                        bool hasOffers = db.Offers.Any(o => o.Id == realEstateId);

                        if (hasDemands || hasOffers)
                        {
                            var errorWindow = new Window
                            {
                                Title = "Ошибка",
                                Content = new TextBlock
                                {
                                    Text = "Невозможно удалить объект недвижимости, так как он связан с потребностью или предложением.",
                                    Margin = new Thickness(20),
                                    TextWrapping = TextWrapping.Wrap
                                },
                                SizeToContent = SizeToContent.WidthAndHeight,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner
                            };
                            errorWindow.ShowDialog(TopLevel.GetTopLevel(this) as Window);
                            return;
                        }

                        db.RealEstates.Remove(realEstate);
                        db.SaveChanges();
                        LoadRealEstates();
                    }
                }
            }
        }
    }
}