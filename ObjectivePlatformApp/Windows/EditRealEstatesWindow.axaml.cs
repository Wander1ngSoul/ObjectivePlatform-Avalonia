using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ObjectivePlatformApp.Data;
using ObjectivePlatformApp.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Avalonia.Media;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ObjectivePlatformApp
{
    public partial class EditRealEstatesWindow : UserControl
    {
        private RealEstates _realEstate;
        private bool _isNewRealEstate;

        private readonly Regex _numberRegex = new Regex(@"^-?\d*\.?\d+$");
        private readonly Regex _intRegex = new Regex(@"^\d+$");
        private readonly Regex _addressPartRegex = new Regex(@"^[a-zA-Zа-яА-Я0-9\s\.\-]*$");

        public EditRealEstatesWindow()
        {
            InitializeComponent();
            InitializeEvents();
        }

        public EditRealEstatesWindow(RealEstates realEstate, bool isNewRealEstate = false) : this()
        {
            _realEstate = realEstate;
            _isNewRealEstate = isNewRealEstate;

            LoadRealEstateData();
            InitializeRealEstateTypeComboBox();
            ValidateAllFields();
        }

        private void InitializeRealEstateTypeComboBox()
        {
            if (_realEstate.RealEstateTypeId > 0)
            {
                switch (_realEstate.RealEstateTypeId)
                {
                    case 1: TablesComboBox.SelectedIndex = 2; break;
                    case 2: TablesComboBox.SelectedIndex = 0; break;
                    case 3: TablesComboBox.SelectedIndex = 1; break;
                }
            }
        }

        private void InitializeEvents()
        {
            BackButton.Click += (s, e) => NavigateBack();
            CancelButton.Click += (s, e) => NavigateBack();
            SaveButton.Click += SaveButton_Click;

            TablesComboBox.SelectionChanged += (s, e) =>
            {
                TypeError.Text = TablesComboBox.SelectedItem == null ? "Выберите тип недвижимости" : "";
                UpdateFieldsVisibility();
                UpdateSaveButtonState();
            };

            CityTextBox.TextChanged += (s, e) => { ValidateAddressField(CityTextBox, CityError); UpdateSaveButtonState(); };
            StreetTextBox.TextChanged += (s, e) => { ValidateAddressField(StreetTextBox, StreetError); UpdateSaveButtonState(); };
            HouseTextBox.TextChanged += (s, e) => { ValidateAddressField(HouseTextBox, HouseError); UpdateSaveButtonState(); };
            FlatTextBox.TextChanged += (s, e) => { ValidateAddressField(FlatTextBox, FlatError); UpdateSaveButtonState(); };
            FloorTextBox.TextChanged += (s, e) => { ValidateFloorField(); UpdateSaveButtonState(); };
            RoomsTextBox.TextChanged += (s, e) => { ValidateRoomsField(); UpdateSaveButtonState(); };
            AreaTextBox.TextChanged += (s, e) => { ValidateAreaField(); UpdateSaveButtonState(); };
            LatitudeTextBox.TextChanged += (s, e) => { ValidateCoordinateField(LatitudeTextBox, LatitudeError, -90, 90); UpdateSaveButtonState(); };
            LongitudeTextBox.TextChanged += (s, e) => { ValidateCoordinateField(LongitudeTextBox, LongitudeError, -180, 180); UpdateSaveButtonState(); };
        }



        private async void DeleteRealEstate_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int realEstateId)
            {
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
                            LoadRealEstateData();
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

                // Получаем родительское окно
                var parentWindow = TopLevel.GetTopLevel(this) as Window;

                // Показываем диалог как модальное окно
                await confirmDialog.ShowDialog(parentWindow);
            }
        }

        private void UpdateFieldsVisibility()
        {
            bool isApartment = TablesComboBox.SelectedIndex == 2;
            bool isHouse = TablesComboBox.SelectedIndex == 0;
            bool isLand = TablesComboBox.SelectedIndex == 1;

            FloorTextBox.IsVisible = isApartment;
            FloorError.IsVisible = isApartment;
            FlatTextBox.IsVisible = isApartment;
            FlatError.IsVisible = isApartment;

            RoomsTextBox.IsVisible = isApartment || isHouse;
            RoomsError.IsVisible = isApartment || isHouse;
            AreaTextBox.IsVisible = isApartment || isHouse || isLand;
            AreaError.IsVisible = isApartment || isHouse || isLand;
        }

        private void ValidateAddressField(TextBox textBox, TextBlock errorBlock)
        {
            var text = textBox.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(text))
            {
                errorBlock.Text = "";
                return;
            }

            if (!_addressPartRegex.IsMatch(text))
            {
                errorBlock.Text = "Адрес может содержать только буквы, цифры, пробелы, точки и дефисы";
            }
            else if (text.Length > 100)
            {
                errorBlock.Text = "Адресная часть не должна превышать 100 символов";
            }
            else
            {
                errorBlock.Text = "";
            }
        }

        private void ValidateFloorField()
        {
            var text = FloorTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(text))
            {
                FloorError.Text = "";
                return;
            }

            if (!_intRegex.IsMatch(text))
            {
                FloorError.Text = "Этаж должен быть целым числом";
            }
            else if (int.TryParse(text, out var floor) && floor < 0)
            {
                FloorError.Text = "Этаж не может быть отрицательным";
            }
            else
            {
                FloorError.Text = "";
            }
        }

        private void ValidateRoomsField()
        {
            var text = RoomsTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(text))
            {
                RoomsError.Text = "";
                return;
            }

            if (!_intRegex.IsMatch(text))
            {
                RoomsError.Text = "Количество комнат должно быть целым числом";
            }
            else if (int.TryParse(text, out var rooms) && rooms <= 0)
            {
                RoomsError.Text = "Количество комнат должно быть положительным";
            }
            else
            {
                RoomsError.Text = "";
            }
        }

        private void ValidateAreaField()
        {
            var text = AreaTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(text))
            {
                AreaError.Text = "";
                return;
            }

            if (!_numberRegex.IsMatch(text))
            {
                AreaError.Text = "Площадь должна быть числом";
            }
            else if (double.TryParse(text, out var area) && area <= 0)
            {
                AreaError.Text = "Площадь должна быть положительной";
            }
            else
            {
                AreaError.Text = "";
            }
        }

        private void ValidateCoordinateField(TextBox textBox, TextBlock errorBlock, double minValue, double maxValue)
        {
            var text = textBox.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(text))
            {
                errorBlock.Text = "";
                return;
            }

            if (!_numberRegex.IsMatch(text))
            {
                errorBlock.Text = "Введите корректное число";
            }
            else if (double.TryParse(text, out var value))
            {
                if (value < minValue || value > maxValue)
                {
                    errorBlock.Text = $"Значение должно быть между {minValue} и {maxValue}";
                }
                else
                {
                    errorBlock.Text = "";
                }
            }
            else
            {
                errorBlock.Text = "Введите корректное число";
            }
        }

        private void LoadRealEstateData()
        {
            CityTextBox.Text = _realEstate.City;
            StreetTextBox.Text = _realEstate.Street;
            HouseTextBox.Text = _realEstate.House?.ToString();
            FlatTextBox.Text = _realEstate.Flat?.ToString();
            FloorTextBox.Text = _realEstate.Floor?.ToString();
            RoomsTextBox.Text = _realEstate.Rooms?.ToString();
            AreaTextBox.Text = _realEstate.Area?.ToString();
            LatitudeTextBox.Text = _realEstate.Latitude?.ToString();
            LongitudeTextBox.Text = _realEstate.Longitude?.ToString();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!IsFormValid()) return;

            UpdateRealEstateData();
            SaveRealEstateToDatabase();
            var successWindow = new Window
            {
                Title = "Уведомление",
                Content = new TextBlock
                {
                    Text = "Процесс выполнен успешно!",
                    Margin = new Thickness(20),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16
                },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                MinWidth = 350
            };
            successWindow.ShowDialog(TopLevel.GetTopLevel(this) as Window);
            NavigateBack();
        }

        private void UpdateRealEstateData()
        {
            _realEstate.City = CityTextBox.Text?.Trim();
            _realEstate.Street = StreetTextBox.Text?.Trim();

            _realEstate.House = int.TryParse(HouseTextBox.Text?.Trim(), out var house) ? house : null;
            _realEstate.Flat = int.TryParse(FlatTextBox.Text?.Trim(), out var flat) ? flat : null;
            _realEstate.Floor = int.TryParse(FloorTextBox.Text?.Trim(), out var floor) ? floor : null;
            _realEstate.Rooms = int.TryParse(RoomsTextBox.Text?.Trim(), out var rooms) ? rooms : null;

            _realEstate.Area = double.TryParse(AreaTextBox.Text?.Trim(), out var area) ? area : null;
            _realEstate.Latitude = double.TryParse(LatitudeTextBox.Text?.Trim(), out var lat) ? lat : null;
            _realEstate.Longitude = double.TryParse(LongitudeTextBox.Text?.Trim(), out var lon) ? lon : null;

            if (TablesComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _realEstate.RealEstateTypeId = selectedItem.Content.ToString() switch
                {
                    "Дом" => 2,
                    "Земля" => 3,
                    "Квартира" => 1,
                    _ => 0
                };
            }
        }

        private void SaveRealEstateToDatabase()
        {
            using var db = new AppDbContext();
            if (_isNewRealEstate)
                db.RealEstates.Add(_realEstate);
            else
                db.RealEstates.Update(_realEstate);
            db.SaveChanges();
        }

        private void NavigateBack()
        {
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
            mainWindow.Content = new RealEstatesWindow();
        }

        private void ValidateAllFields()
        {
            ValidateAddressField(CityTextBox, CityError);
            ValidateAddressField(StreetTextBox, StreetError);
            ValidateAddressField(HouseTextBox, HouseError);
            ValidateAddressField(FlatTextBox, FlatError);
            ValidateFloorField();
            ValidateRoomsField();
            ValidateAreaField();
            ValidateCoordinateField(LatitudeTextBox, LatitudeError, -90, 90);
            ValidateCoordinateField(LongitudeTextBox, LongitudeError, -180, 180);
            UpdateFieldsVisibility();
            UpdateSaveButtonState();
        }

        private bool IsFormValid()
        {
            if (TablesComboBox.SelectedItem == null) return false;

            if (!string.IsNullOrEmpty(CityTextBox.Text) && CityError.Text != "") return false;
            if (!string.IsNullOrEmpty(StreetTextBox.Text) && StreetError.Text != "") return false;
            if (!string.IsNullOrEmpty(HouseTextBox.Text) && HouseError.Text != "") return false;
            if (!string.IsNullOrEmpty(FlatTextBox.Text) && FlatError.Text != "") return false;

            if (!string.IsNullOrEmpty(FloorTextBox.Text) && FloorError.Text != "") return false;
            if (!string.IsNullOrEmpty(RoomsTextBox.Text) && RoomsError.Text != "") return false;
            if (!string.IsNullOrEmpty(AreaTextBox.Text) && AreaError.Text != "") return false;
            if (!string.IsNullOrEmpty(LatitudeTextBox.Text) && LatitudeError.Text != "") return false;
            if (!string.IsNullOrEmpty(LongitudeTextBox.Text) && LongitudeError.Text != "") return false;

            return true;
        }

        private void UpdateSaveButtonState()
        {
            SaveButton.IsEnabled = IsFormValid();
        }
    }
}