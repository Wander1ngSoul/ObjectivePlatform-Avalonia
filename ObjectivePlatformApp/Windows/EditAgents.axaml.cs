using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ObjectivePlatformApp.Data;
using ObjectivePlatformApp.Models;
using System.Text.RegularExpressions;
using System;
using Avalonia.Media;

namespace ObjectivePlatformApp
{
    public partial class EditAgent : UserControl
    {
        private Agents _agent;
        private bool _isNewAgent;
        private bool _isValid = false;

        private readonly Regex _nameRegex = new Regex(@"^[А-ЯЁа-яёA-Za-z\-]+$");
        private readonly Regex _commissionRegex = new Regex(@"^[0-9]*$"); // Разрешаем только цифры при вводе

        public EditAgent()
        {
            InitializeComponent();
            BackButton.Click += BackButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;

            LastNameTextBox.TextInput += NameTextBox_TextInput;
            FirstNameTextBox.TextInput += NameTextBox_TextInput;
            MiddleNameTextBox.TextInput += NameTextBox_TextInput;
            CommissionTextBox.TextInput += CommissionTextBox_TextInput;

            LastNameTextBox.TextChanged += NameTextBox_TextChanged;
            FirstNameTextBox.TextChanged += NameTextBox_TextChanged;
            MiddleNameTextBox.TextChanged += NameTextBox_TextChanged;
            CommissionTextBox.TextChanged += CommissionTextBox_TextChanged;

            LastNameTextBox.LostFocus += NameTextBox_LostFocus;
            FirstNameTextBox.LostFocus += NameTextBox_LostFocus;
            MiddleNameTextBox.LostFocus += NameTextBox_LostFocus;
            CommissionTextBox.LostFocus += CommissionTextBox_LostFocus;
        }

        private void NameTextBox_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e)
        {
            if (sender is TextBox textBox && !_nameRegex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void CommissionTextBox_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e)
        {
            if (sender is TextBox textBox && !_commissionRegex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        public EditAgent(Agents agent, bool isNewAgent = false) : this()
        {
            _agent = agent;
            _isNewAgent = isNewAgent;

            LastNameTextBox.Text = agent.LastName;
            FirstNameTextBox.Text = agent.FirstName;
            MiddleNameTextBox.Text = agent.MiddleName;
            CommissionTextBox.Text = agent.Commision.ToString();

            ValidateNameField(LastNameTextBox, LastNameError, "Фамилия");
            ValidateNameField(FirstNameTextBox, FirstNameError, "Имя");
            ValidateNameField(MiddleNameTextBox, MiddleNameError, "Отчество");
            ValidateCommissionField();
            ValidateAllFields();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!_isValid) return;

            _agent.LastName = LastNameTextBox.Text?.Trim() ?? "";
            _agent.FirstName = FirstNameTextBox.Text?.Trim() ?? "";
            _agent.MiddleName = MiddleNameTextBox.Text?.Trim() ?? "";

            if (int.TryParse(CommissionTextBox.Text, out int commission))
            {
                _agent.Commision = commission;
            }

            using (var db = new AppDbContext())
            {
                if (_isNewAgent)
                {
                    db.Agents.Add(_agent);
                }
                else
                {
                    db.Agents.Update(_agent);
                }
                db.SaveChanges();
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
            }

            NavigateBack();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            var mainWindow = (MainWindow)TopLevel.GetTopLevel(this)!;
            mainWindow.Content = new AgentsWindow();
        }

        private void NameTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var (errorTextBlock, fieldName) = GetErrorBlockAndFieldName(textBox);
                if (errorTextBlock != null)
                {
                    ValidateNameField(textBox, errorTextBlock, fieldName);
                }
            }
            ValidateAllFields();
        }

        private void NameTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var (errorTextBlock, fieldName) = GetErrorBlockAndFieldName(textBox);
                if (errorTextBlock != null)
                {
                    ValidateNameField(textBox, errorTextBlock, fieldName);
                }
            }
            ValidateAllFields();
        }

        private void ValidateNameField(TextBox textBox, TextBlock errorTextBlock, string fieldName)
        {
            var text = textBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(text))
            {
                errorTextBlock.Text = $"{fieldName} обязательно для заполнения";
            }
            else if (!_nameRegex.IsMatch(text))
            {
                errorTextBlock.Text = $"{fieldName} может содержать только буквы и дефис";
            }
            else
            {
                errorTextBlock.Text = "";
            }
        }

        private void CommissionTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            ValidateCommissionField();
            ValidateAllFields();
        }

        private void CommissionTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            ValidateCommissionField();
            ValidateAllFields();
        }

        private void ValidateCommissionField()
        {
            var text = CommissionTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(text))
            {
                CommissionError.Text = "Комиссия обязательна для заполнения";
            }
            else if (!int.TryParse(text, out int commission))
            {
                CommissionError.Text = "Введите корректное число";
            }
            else if (commission < 0 || commission > 100)
            {
                CommissionError.Text = "Комиссия должна быть от 0 до 100%";
            }
            else
            {
                CommissionError.Text = "";
            }
        }

        private (TextBlock? errorTextBlock, string fieldName) GetErrorBlockAndFieldName(TextBox textBox)
        {
            return textBox.Name switch
            {
                nameof(FirstNameTextBox) => (FirstNameError, "Имя"),
                nameof(LastNameTextBox) => (LastNameError, "Фамилия"),
                nameof(MiddleNameTextBox) => (MiddleNameError, "Отчество"),
                _ => (null, string.Empty)
            };
        }

        private void ValidateAllFields()
        {
            bool lastNameValid = string.IsNullOrEmpty(LastNameError.Text);
            bool firstNameValid = string.IsNullOrEmpty(FirstNameError.Text);
            bool middleNameValid = string.IsNullOrEmpty(MiddleNameError.Text);
            bool commissionValid = string.IsNullOrEmpty(CommissionError.Text);

            _isValid = lastNameValid && firstNameValid && middleNameValid && commissionValid;
            SaveButton.IsEnabled = _isValid;
        }
    }
}