using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ObjectivePlatformApp.Data;
using ObjectivePlatformApp.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace ObjectivePlatformApp;

public partial class EditClient : UserControl
{
    private Clients _client;
    private bool _isNewClient;
    private bool _isValid = false;

    // Регулярные выражения для валидации
    private readonly Regex _nameRegex = new Regex(@"^[А-ЯЁа-яёA-Za-z\-]+$");
    private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    private readonly Regex _phoneRegex = new Regex(@"^\+?\d{10,15}$");

    public EditClient()
    {
        InitializeComponent();

        // Подключаем обработчики событий
        BackButton.Click += BackButton_Click;
        SaveButton.Click += SaveButton_Click;
        CancelButton.Click += CancelButton_Click;

        FirstNameTextBox.TextChanged += NameTextBox_TextChanged;
        LastNameTextBox.TextChanged += NameTextBox_TextChanged;
        MiddleNameTextBox.TextChanged += NameTextBox_TextChanged;
        EmailTextBox.TextChanged += EmailTextBox_TextChanged;
        PhoneTextBox.TextChanged += PhoneTextBox_TextChanged;
    }

    public EditClient(Clients client, bool isNewClient = false) : this()
    {
        _client = client;
        _isNewClient = isNewClient;

        FirstNameTextBox.Text = client.FirstName;
        LastNameTextBox.Text = client.LastName;
        MiddleNameTextBox.Text = client.MiddleName;
        EmailTextBox.Text = client.Email;
        PhoneTextBox.Text = client.Phone;

        ValidateAllFields();
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        NavigateBack();
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!_isValid) return;

        _client.FirstName = FirstNameTextBox.Text?.Trim() ?? "";
        _client.LastName = LastNameTextBox.Text?.Trim() ?? "";
        _client.MiddleName = MiddleNameTextBox.Text?.Trim() ?? "";
        _client.Email = EmailTextBox.Text?.Trim() ?? "";
        _client.Phone = PhoneTextBox.Text?.Trim() ?? "";

        using (var db = new AppDbContext())
        {
            if (_isNewClient)
            {
                db.Clients.Add(_client);
            }
            else
            {
                db.Clients.Update(_client);
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
        mainWindow.Content = new ClientsWindow();
    }

    #region Валидация полей

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

        // Для отчества валидация необязательна
        if (textBox.Name == nameof(MiddleNameTextBox) && string.IsNullOrEmpty(text))
        {
            errorTextBlock.Text = "";
        }
        else
        {
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

        ValidateAllFields();
    }

    private void EmailTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = EmailTextBox.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(text))
        {
            EmailError.Text = "Email обязателен для заполнения";
        }
        else if (!_emailRegex.IsMatch(text))
        {
            EmailError.Text = "Введите корректный email (например: example@mail.com)";
        }
        else
        {
            EmailError.Text = "";
        }

        ValidateAllFields();
    }

    private void PhoneTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = PhoneTextBox.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(text))
        {
            PhoneError.Text = "Телефон обязателен для заполнения";
        }
        else if (!_phoneRegex.IsMatch(text))
        {
            PhoneError.Text = "Введите корректный телефон (например: +71234567890 или 1234567890)";
        }
        else
        {
            PhoneError.Text = "";
        }

        ValidateAllFields();
    }

    private void ValidateAllFields()
    {
        // Проверяем обязательные поля
        bool firstNameValid = !string.IsNullOrWhiteSpace(FirstNameTextBox.Text) &&
                            _nameRegex.IsMatch(FirstNameTextBox.Text.Trim());
        bool lastNameValid = !string.IsNullOrWhiteSpace(LastNameTextBox.Text) &&
                           _nameRegex.IsMatch(LastNameTextBox.Text.Trim());
        bool emailValid = !string.IsNullOrWhiteSpace(EmailTextBox.Text) &&
                         _emailRegex.IsMatch(EmailTextBox.Text.Trim());
        bool phoneValid = !string.IsNullOrWhiteSpace(PhoneTextBox.Text) &&
                        _phoneRegex.IsMatch(PhoneTextBox.Text.Trim());

        // Отчество необязательно, но если заполнено - должно быть валидным
        bool middleNameValid = string.IsNullOrWhiteSpace(MiddleNameTextBox.Text) ||
                             _nameRegex.IsMatch(MiddleNameTextBox.Text.Trim());

        _isValid = firstNameValid && lastNameValid && middleNameValid && emailValid && phoneValid;
        SaveButton.IsEnabled = _isValid;
    }

    #endregion
}