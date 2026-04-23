using EarthEvolutionProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EarthEvolutionProject
{
    public partial class MainWindow : Window
    {
        private List<Period> _allPeriods = [];

        /// <summary>
        /// Конструктор головного вікна застосунку. Виконує ініціалізацію компонентів інтерфейсу, 
        /// завантаження початкових даних та налаштування обробки події повернення з режиму пошуку.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeAppData();

            MainSearchBar.BackRequested += (s, e) =>
            {
                SearchResultsGrid.Visibility = Visibility.Collapsed;
                MainContentGrid.Visibility = Visibility.Visible; 
            };
        }

        /// <summary>
        /// Проводить початкове налаштування застосунку: зчитує дані з JSON-файлу, виконує десеріалізацію, 
        /// встановлює зв'язки між об'єктами та ініціалізує список фільтрів у панелі пошуку.
        /// </summary>
        private void InitializeAppData()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "evolution_data.json");
                string jsonString = File.ReadAllText(filePath);

                _allPeriods = JsonSerializer.Deserialize<List<Period>>(jsonString) ?? [];

                if (_allPeriods != null)
                {
                    foreach (var period in _allPeriods)
                    {
                        foreach (var organism in period.Organisms)
                        {
                            organism.PeriodName = period.Name;
                            organism.PeriodId = period.Id;
                        }
                    }
                }

                if (_allPeriods != null && _allPeriods.Any())
                    {
                    var allTypes = _allPeriods
                        .SelectMany(p => p.Organisms)
                        .Select(o => o.Type)        
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct()                 
                        .OrderBy(t => t);       

                    MainSearchBar.SetTypes(allTypes);
                    }

                SwitchPeriod("triassic");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка ініціалізації: " + ex.Message);
            }
        }

        /// <summary>
        /// Обробляє подію вибору конкретного періоду на інтерактивній шкалі часу.
        /// </summary>
        /// <param name="sender">Джерело події (компонент шкали часу).</param>
        /// <param name="periodId">Унікальний ідентифікатор обраного геологічного періоду.</param>
        private void OnTimelinePeriodSelected(object sender, string periodId)
        {
            SwitchPeriod(periodId);
        }

        /// <summary>
        /// Виконує перемикання контексту даних застосунку на обраний період, оновлює стан кнопок 
        /// на шкалі часу та скидає стан відображення галереї видів.
        /// </summary>
        /// <param name="periodId">Ідентифікатор періоду, який необхідно активувати.</param>
        private void SwitchPeriod(string periodId)
        {
            var selectedPeriod = _allPeriods.FirstOrDefault(p => p.Id == periodId);
            if (selectedPeriod != null)
            {
                this.DataContext = selectedPeriod;
                TimelineControl.UpdateActiveButton(periodId);

                SpeciesControl.ResetToGallery();
            }
        }

        /// <summary>
        /// Реалізує алгоритм фільтрації даних у реальному часі. Аналізує введений текст та обрані категорії, 
        /// після чого змінює видимість контейнерів інтерфейсу для відображення результатів пошуку.
        /// </summary>
        /// <param name="sender">Джерело події зміни фільтра.</param>
        /// <param name="e">Аргументи події.</param>
        private void MainSearchBar_FilterChanged(object sender, EventArgs e)
        {
            string query = MainSearchBar.SearchText;
            var selectedTypes = MainSearchBar.SelectedTypes;

            if (string.IsNullOrWhiteSpace(query) && !selectedTypes.Any())
            {
                MainContentGrid.Visibility = Visibility.Visible;
                SearchResultsGrid.Visibility = Visibility.Collapsed;

                TimelinePanel.Visibility = Visibility.Visible;

                if (DataContext is Period p) SpeciesControl.DisplayResults(p.Organisms);
                return;
            }

            MainContentGrid.Visibility = Visibility.Collapsed;
            SearchResultsGrid.Visibility = Visibility.Visible;

            TimelinePanel.Visibility = Visibility.Collapsed;

            var foundOrganisms = _allPeriods
                .SelectMany(p => p.Organisms)
                .Where(o =>
                    ((o.CommonName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                     (o.ScientificName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                    (!selectedTypes.Any() || selectedTypes.Contains(o.Type))
                ).ToList();

            SearchResultsPage.DisplayResults(foundOrganisms);
        }

        /// <summary>
        /// Обробляє вибір конкретної істоти зі списку результатів пошуку. Скидає стан панелі пошуку, 
        /// автоматично перемикає програму на відповідний період та відкриває детальну картку обраного організму.
        /// </summary>
        /// <param name="sender">Джерело події вибору.</param>
        /// <param name="selectedOrganism">Об'єкт моделі обраного організму.</param>
        private void OnSearchResultSelected(object sender, Organism selectedOrganism)
        {
            MainSearchBar.ClearAndHide();

            MainContentGrid.Visibility = Visibility.Visible;
            SearchResultsGrid.Visibility = Visibility.Collapsed;
            TimelinePanel.Visibility = Visibility.Visible;

            SwitchPeriod(selectedOrganism.PeriodId);

            SpeciesControl.ShowOrganismDetails(selectedOrganism);
        }

        /// <summary>
        /// Забезпечує логіку перемикання візуальних тем застосунку (світлої та темної) та оновлює 
        /// відповідну іконку на кнопці керування темою.
        /// </summary>
        /// <param name="sender">Кнопка перемикання теми.</param>
        /// <param name="e">Аргументи події натискання.</param>
        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is App app)
            {
                bool isDark = app.ToggleTheme();

                if (ThemeButton.Template.FindName("ThemeIcon", ThemeButton) is TextBlock textBlock)
                {
                    textBlock.Text = isDark ? "🌙" : "🔆";
                }
            }
        }
    }
}