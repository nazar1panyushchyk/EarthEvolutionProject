using EarthEvolutionProject.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Графічний інтерфейс авторизації та управління профілями користувачів у системі.
    /// Забезпечує завантаження наявних облікових записів, створення нових ідентифікаторів та видалення даних.
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Сервісний об'єкт для управління файловими операціями, збереженням та завантаженням профілів користувачів.
        /// </summary>
        private readonly ProfileManager _profileManager;

        /// <summary>
        /// Конструктор вікна авторизації. Ініціалізує компоненти XAML, створює менеджер профілів та завантажує список користувачів.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            LoadUsers();
        }

        /// <summary>
        /// Завантажує список усіх зареєстрованих імен користувачів із бази даних або файлової системи та налаштовує видимість елементів інтерфейсу.
        /// </summary>
        private void LoadUsers()
        {
            var users = _profileManager.GetAllUsernames();
            if (users.Count == 0)
            {
                NoProfilesText.Visibility = Visibility.Visible;
                UsersListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoProfilesText.Visibility = Visibility.Collapsed;
                UsersListBox.Visibility = Visibility.Visible;
                UsersListBox.ItemsSource = users;
            }
        }

        /// <summary>
        /// Обробляє зміну вибору елемента у списку профілів, автоматично підставляючи обране ім'я в текстове поле введення.
        /// </summary>
        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is string selectedUsername)
            {
                NewUserTextBox.Text = selectedUsername;
            }
        }

        /// <summary>
        /// Обробляє ручне введення або зміну тексту в полі імені, скидаючи виділення у загальному списку профілів при розбіжності даних.
        /// </summary>
        private void NewUserTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is string selected && selected != NewUserTextBox.Text)
            {
                UsersListBox.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Виконує перевірку введеного імені, ініціює авторизацію чи створення нового профілю та відкриває головне вікно програми.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = NewUserTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Будь ласка, введіть або оберіть ім'я профілю.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _profileManager.SelectOrCreateProfile(username);

            MainWindow mainWindow = new MainWindow(_profileManager);
            mainWindow.Show();

            this.Close();
        }

        /// <summary>
        /// Обробляє запит на повне видалення вибраного профілю користувача разом із його локальною історією після підтвердження операції.
        /// </summary>
        private void DeleteProfileItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.CommandParameter is string username)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Ви дійсно хочете видалити профіль \"{username}\" та всю його історію?",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    bool isDeleted = _profileManager.DeleteProfile(username);

                    if (isDeleted)
                    {
                        if (NewUserTextBox.Text.Trim().Equals(username, StringComparison.OrdinalIgnoreCase))
                        {
                            NewUserTextBox.Text = string.Empty;
                        }

                        LoadUsers();

                        MessageBox.Show($"Профіль \"{username}\" успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не вдалося видалити цей профіль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}