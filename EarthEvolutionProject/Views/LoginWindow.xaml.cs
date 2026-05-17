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
    public partial class LoginWindow : Window
    {
        private readonly ProfileManager _profileManager;

        public LoginWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            LoadUsers();
        }

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

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is string selectedUsername)
            {
                NewUserTextBox.Text = selectedUsername;
            }
        }

        private void NewUserTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UsersListBox.SelectedItem is string selected && selected != NewUserTextBox.Text)
            {
                UsersListBox.SelectedIndex = -1;
            }
        }

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
    }
}
