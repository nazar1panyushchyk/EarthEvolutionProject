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

        /// <summary>
        /// Обробляє подію запуску застосунку. Виконує базову ініціалізацію системи 
        /// та забезпечує створення і відображення стартового вікна авторизації користувачів.
        /// </summary>
        /// <param name="e">Аргументи події запуску, що містять параметри командного рядка.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        /// <summary>
        /// Виконує інверсію поточного стану колірної палітри застосунку на протилежний 
        /// за допомогою виклику методу встановлення теми.
        /// </summary>
        /// <returns>Логічне значення true, якщо встановлено темну тему, або false, якщо світлу.</returns>
        public bool ToggleTheme()
        {
            return SetTheme(!_isDarkTheme);
        }

        /// <summary>
        /// Здійснює динамічне завантаження словника ресурсів графічної теми із файлової структури проєкту 
        /// та виконує його повне перепризначення в глобальній колекції MergedDictionaries.
        /// </summary>
        /// <param name="isDark">Прапорець, який визначає необхідність активації темної теми.</param>
        /// <returns>Поточний зафіксований стан візуальної теми після завершення операції завантаження.</returns>
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