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
        /// Реалізує логіку динамічного перемикання візуальних тем застосунку. 
        /// Метод оновлює MergedDictionaries, замінюючи поточний словник ресурсів на вибраний (світлий або темний).
        /// </summary>
        /// <returns>Повертає логічне значення: true, якщо встановлено темну тему; false, якщо світлу.</returns>
        public bool ToggleTheme()
            {
                _isDarkTheme = !_isDarkTheme;
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
