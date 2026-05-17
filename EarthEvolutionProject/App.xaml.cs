using EarthEvolutionProject.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace EarthEvolutionProject
{
    /// <summary>
    /// Основний клас застосунку, що успадковується від Application. 
    /// Забезпечує управління життєвим циклом програми та глобальними ресурсами.
    /// </summary>
    public partial class App : Application
    {
        private bool _isDarkTheme = true;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        public bool ToggleTheme()
        {
            return SetTheme(!_isDarkTheme);
        }

        public bool SetTheme(bool isDark)
        {
            _isDarkTheme = isDark;
            string themeName = _isDarkTheme ? "DarkTheme" : "LightTheme";

            try
            {
                var uri = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);
                ResourceDictionary newTheme = new ResourceDictionary { Source = uri };

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(newTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження теми: {ex.Message}");
            }

            return _isDarkTheme;
        }
    } 
}
