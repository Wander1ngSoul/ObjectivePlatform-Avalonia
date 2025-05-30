using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ObjectivePlatformApp.Data;
using ObjectivePlatformApp.Models;
using System.Text.RegularExpressions;
using System;

namespace ObjectivePlatformApp
{
    public partial class EditAgent : UserControl
    {
        private Agents _agent;
        private bool _isNewAgent;
        private bool _isValid = false;

        private readonly Regex _nameRegex = new Regex(@"^[А-ЯЁа-яёA-Za-z\-]+$");
        private readonly Regex _commissionRegex = new Regex(@"^\d{1,3}$");

        public EditAgent()
        {
            InitializeComponent();
            BackButton.Click += BackButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;

            LastNameTextBox.TextInput += NameTextBox_TextInput;
            FirstNameTextBox.TextInput += NameTextBox_TextInput;
            MiddleNameTextBox.TextInput += NameTextBox_TextInput;

            LastNameTextBox.TextChanged += NameTextBox_TextChanged;
            FirstNameTextBox.TextChanged += NameTextBox_TextChanged;
            MiddleNameTextBox.TextChanged += NameTextBox_TextChanged;
            CommissionTextBox.TextChanged += CommissionTextBox_TextChanged;
        }

        private void NameTextBox_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e)
        {
            if (sender is TextBox textBox && !_nameRegex.IsMatch(e.Text))
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
            var textBox = sender as TextBox;
            var fieldName = textBox?.Name switch
            {
                nameof(FirstNameTextBox) => "Имя",
                nameof(LastNameTextBox) => "Фамилия",
                nameof(MiddleNameTextBox) => "Отчество",
                _ => string.Empty
            };

            var errorTextBlock = this.FindControl<TextBlock>($"{textBox?.Name}Error");

            if (textBox == null || errorTextBlock == null) return;

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

            ValidateAllFields();
        }

        private void CommissionTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var text = CommissionTextBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(text))
            {
                CommissionError.Text = "Комиссия обязательна для заполнения";
            }
            else if (!_commissionRegex.IsMatch(text))
            {
                CommissionError.Text = "Комиссия должна быть числом от 0 до 100";
            }
            else if (int.TryParse(text, out int commission))
            {
                if (commission < 0 || commission > 100)
                {
                    CommissionError.Text = "Комиссия должна быть от 0 до 100%";
                }
                else
                {
                    CommissionError.Text = "";
                }
            }
            else
            {
                CommissionError.Text = "Введите корректное число";
            }

            ValidateAllFields();
        }

        private void ValidateAllFields()
        {
            bool lastNameValid = !string.IsNullOrWhiteSpace(LastNameTextBox.Text) &&
                               _nameRegex.IsMatch(LastNameTextBox.Text.Trim());

            bool firstNameValid = !string.IsNullOrWhiteSpace(FirstNameTextBox.Text) &&
                                _nameRegex.IsMatch(FirstNameTextBox.Text.Trim());

            bool middleNameValid = !string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) &&
                                 _nameRegex.IsMatch(MiddleNameTextBox.Text.Trim());

            bool commissionValid = !string.IsNullOrWhiteSpace(CommissionTextBox.Text) &&
                                 _commissionRegex.IsMatch(CommissionTextBox.Text.Trim()) &&
                                 int.TryParse(CommissionTextBox.Text, out int commission) &&
                                 commission >= 0 && commission <= 100;

            _isValid = lastNameValid && firstNameValid && middleNameValid && commissionValid;
            SaveButton.IsEnabled = _isValid;
        }
    }
}