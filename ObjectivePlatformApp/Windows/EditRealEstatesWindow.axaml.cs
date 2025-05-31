using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ObjectivePlatformApp.Data;
using ObjectivePlatformApp.Models;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ObjectivePlatformApp
{
    public partial class EditRealEstatesWindow : UserControl
    {
        private RealEstates _realEstate;
        private bool _isNewRealEstate;
        private bool _isValid = false;

        private readonly Regex _numberRegex = new Regex(@"^\d*$");
        private readonly Regex _decimalRegex = new Regex(@"^\d*\.?\d*$");

        public EditRealEstatesWindow()
        {
            InitializeComponent();
            BackButton.Click += BackButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;

            // Подписка на события изменения текста
            CityTextBox.TextChanged += Field_TextChanged;
            StreetTextBox.TextChanged += Field_TextChanged;
            HouseTextBox.TextChanged += Field_TextChanged;
            FlatTextBox.TextChanged += Field_TextChanged;
            FloorTextBox.TextChanged += Field_TextChanged;
            RoomsTextBox.TextChanged += Field_TextChanged;
            AreaTextBox.TextChanged += Field_TextChanged;
            LatitudeTextBox.TextChanged += Field_TextChanged;
            LongitudeTextBox.TextChanged += Field_TextChanged;
        }

        public EditRealEstatesWindow(RealEstates realEstate, bool isNewRealEstate = false) : this()
        {
            _realEstate = realEstate;
            _isNewRealEstate = isNewRealEstate;

            // Заполнение полей
            CityTextBox.Text = realEstate.City;
            StreetTextBox.Text = realEstate.Street;
            HouseTextBox.Text = realEstate.House?.ToString();
            FlatTextBox.Text = realEstate.Flat?.ToString();
            FloorTextBox.Text = realEstate.Floor?.ToString();
            RoomsTextBox.Text = realEstate.Rooms?.ToString();
            AreaTextBox.Text = realEstate.Area?.ToString();
            LatitudeTextBox.Text = realEstate.Latitude?.ToString();
            LongitudeTextBox.Text = realEstate.Longitude?.ToString();

            ValidateAllFields();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!_isValid) return;

            _realEstate.City = CityTextBox.Text?.Trim();
            _realEstate.Street = StreetTextBox.Text?.Trim();
            _realEstate.House = int.TryParse(HouseTextBox.Text, out var house) ? house : null;
            _realEstate.Flat = int.TryParse(FlatTextBox.Text, out var flat) ? flat : null;
            _realEstate.Floor = int.TryParse(FloorTextBox.Text, out var floor) ? floor : null;
            _realEstate.Rooms = int.TryParse(RoomsTextBox.Text, out var rooms) ? rooms : null;
            _realEstate.Area = double.TryParse(AreaTextBox.Text, out var area) ? area : null;
            _realEstate.Latitude = double.TryParse(LatitudeTextBox.Text, out var lat) ? lat : null;
            _realEstate.Longitude = double.TryParse(LongitudeTextBox.Text, out var lon) ? lon : null;

            using (var db = new AppDbContext())
            {
                if (_isNewRealEstate)
                {
                    db.RealEstates.Add(_realEstate);
                }
                else
                {
                    db.RealEstates.Update(_realEstate);
                }
                db.SaveChanges();
            }

            NavigateBack();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            NavigateBack();
        }

        private void Field_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            var errorTextBlock = this.FindControl<TextBlock>($"{textBox.Name}Error");
            if (errorTextBlock == null) return;

            var text = textBox.Text?.Trim() ?? "";

            if (textBox.Name == nameof(HouseTextBox) ||
                textBox.Name == nameof(FlatTextBox) ||
                textBox.Name == nameof(FloorTextBox) ||
                textBox.Name == nameof(RoomsTextBox))
            {
                if (!string.IsNullOrWhiteSpace(text) && !_numberRegex.IsMatch(text))
                {
                    errorTextBlock.Text = "Должно быть целым числом";
                }
                else
                {
                    errorTextBlock.Text = "";
                }
            }
            else if (textBox.Name == nameof(AreaTextBox) ||
                     textBox.Name == nameof(LatitudeTextBox) ||
                     textBox.Name == nameof(LongitudeTextBox))
            {
                if (!string.IsNullOrWhiteSpace(text) && !_decimalRegex.IsMatch(text))
                {
                    errorTextBlock.Text = "Должно быть числом (дробная часть через точку)";
                }
                else
                {
                    errorTextBlock.Text = "";
                }
            }

            ValidateAllFields();
        }

        private void ValidateAllFields()
        {
            bool cityValid = !string.IsNullOrWhiteSpace(CityTextBox.Text);
            bool streetValid = !string.IsNullOrWhiteSpace(StreetTextBox.Text);
            bool houseValid = string.IsNullOrWhiteSpace(HouseTextBox.Text) || _numberRegex.IsMatch(HouseTextBox.Text);
            bool flatValid = string.IsNullOrWhiteSpace(FlatTextBox.Text) || _numberRegex.IsMatch(FlatTextBox.Text);
            bool floorValid = string.IsNullOrWhiteSpace(FloorTextBox.Text) || _numberRegex.IsMatch(FloorTextBox.Text);
            bool roomsValid = string.IsNullOrWhiteSpace(RoomsTextBox.Text) || _numberRegex.IsMatch(RoomsTextBox.Text);
            bool areaValid = string.IsNullOrWhiteSpace(AreaTextBox.Text) || _decimalRegex.IsMatch(AreaTextBox.Text);
            bool latValid = string.IsNullOrWhiteSpace(LatitudeTextBox.Text) || _decimalRegex.IsMatch(LatitudeTextBox.Text);
            bool lonValid = string.IsNullOrWhiteSpace(LongitudeTextBox.Text) || _decimalRegex.IsMatch(LongitudeTextBox.Text);

            _isValid = cityValid && streetValid && houseValid && flatValid &&
                      floorValid && roomsValid && areaValid && latValid && lonValid;
            SaveButton.IsEnabled = _isValid;
        }
    }
}