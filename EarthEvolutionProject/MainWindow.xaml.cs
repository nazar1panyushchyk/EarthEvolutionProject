using EarthEvolutionProject.Models;
using EarthEvolutionProject.Services;
using EarthEvolutionProject.Views;
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
        private readonly Services.ProfileManager _profileManager;
        private List<Period> _allPeriods = [];
        private List<string> _allFacts = [];
        private bool _isNavigatingBack = false;
        private bool _isPopupOpenBeforeClick = false;

        /// <summary>
        /// Конструктор головного вікна застосунку. Виконує ініціалізацію компонентів інтерфейсу, 
        /// завантаження початкових даних та налаштування обробки події повернення з режиму пошуку.
        /// </summary>
        public MainWindow(Services.ProfileManager profileManager)
        {
            InitializeComponent();
            _profileManager = profileManager;

            if (Application.Current is App app && _profileManager.CurrentProfile != null)
            {
                app.SetTheme(_profileManager.CurrentProfile.IsDarkTheme);
            }

            InitializeAppData();

            this.ContentRendered += MainWindow_ContentRendered;

            MainSearchBar.BackRequested += (s, e) =>
            {
                _isNavigatingBack = true;

                SearchResultsGrid.Visibility = Visibility.Collapsed;
                MainContentGrid.Visibility = Visibility.Visible;
                TimelinePanel.Visibility = Visibility.Visible;

                if (_profileManager.CurrentProfile != null)
                {
                    string savedPeriodId = _profileManager.CurrentProfile.LastSelectedPeriodId;
                    if (string.IsNullOrEmpty(savedPeriodId))
                    {
                        savedPeriodId = "triassic";
                    }

                    SwitchPeriod(savedPeriodId);
                }
                else
                {
                    SwitchPeriod("triassic");
                }

                _isNavigatingBack = false;
            };

            MainSearchBar.HistoryUpdated += (s, e) =>
            {
                if (_profileManager.CurrentProfile != null)
                {
                    _profileManager.CurrentProfile.SearchHistory = new List<string>(MainSearchBar.SearchHistoryList);
                    _profileManager.SaveConfiguration();
                }
            };

            SpeciesControl.OrganismSelected += (s, organism) =>
            {
                if (_profileManager.CurrentProfile != null)
                {
                    _profileManager.CurrentProfile.LastSelectedOrganismId = organism?.Id;
                    _profileManager.SaveConfiguration();
                }
            };

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            this.ContentRendered -= MainWindow_ContentRendered;

            ShowWelcomeFactIfNeeded();
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

                var dataWrapper = JsonSerializer.Deserialize<EvolutionDataWrapper>(jsonString);

                _allPeriods = dataWrapper?.Periods ?? [];
                _allFacts = dataWrapper?.InterestingFacts ?? [];

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

                RestoreSessionState();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка ініціалізації: " + ex.Message);
            }
        }

        private void ShowWelcomeFactIfNeeded()
        {
            var profile = _profileManager.CurrentProfile;

            if (profile == null || !profile.ShowWelcomeFacts || _allFacts == null || !_allFacts.Any())
            {
                return;
            }

            Random random = new Random();
            int randomIndex = random.Next(_allFacts.Count);
            string selectedFact = _allFacts[randomIndex];

            FactWindow factWindow = new FactWindow(selectedFact);
            factWindow.Owner = this; 

            if (factWindow.ShowDialog() == true)
            {
                if (factWindow.DontShowAgain)
                {
                    profile.ShowWelcomeFacts = false;
                    _profileManager.SaveConfiguration();
                }
            }
        }

        private void WelcomeFactsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var profile = _profileManager.CurrentProfile;
            if (profile != null && WelcomeFactsCheckBox != null)
            {
                profile.ShowWelcomeFacts = WelcomeFactsCheckBox.IsChecked ?? false;

                _profileManager.SaveConfiguration();
            }
        }

        /// <summary>
        /// Обробляє подію вибору конкретного періоду на інтерактивній шкалі часу.
        /// </summary>
        /// <param name="sender">Джерело події (компонент шкали часу).</param>
        /// <param name="periodId">Унікальний ідентифікатор обраного геологічного періоду.</param>
        private void OnTimelinePeriodSelected(object sender, string periodId)
        {
            if (_profileManager.CurrentProfile != null)
            {
                _profileManager.CurrentProfile.LastSelectedPeriodId = periodId;
                _profileManager.CurrentProfile.LastSelectedOrganismId = null;
                _profileManager.SaveConfiguration();
            }

            SwitchPeriod(periodId);
        }

        /// <summary>
        /// Виконує перемикання контексту даних застосунку на обраний період, оновлює стан кнопок 
        /// на шкалі часу та скидає стан відображення галереї видів.
        /// </summary>
        /// <param name="periodId">Ідентифікатор періоду, який необхідно активувати.</param>
        private void SwitchPeriod(string periodId)
        {
            var selectedPeriod = _allPeriods.FirstOrDefault(p => p.Id.Equals(periodId, StringComparison.OrdinalIgnoreCase));
            if (selectedPeriod != null)
            {
                this.DataContext = selectedPeriod;

                string formattedId = char.ToUpper(periodId[0]) + periodId.Substring(1).ToLower();
                TimelineControl.UpdateActiveButton(formattedId);

                SpeciesControl.DisplayResults(selectedPeriod.Organisms);
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
            if (_isNavigatingBack)
            {
                return;
            }

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

            if (_profileManager.CurrentProfile != null)
            {
                _profileManager.CurrentProfile.LastSelectedPeriodId = selectedOrganism.PeriodId;
                _profileManager.CurrentProfile.LastSelectedOrganismId = selectedOrganism.Id;
                _profileManager.SaveConfiguration();
            }

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
            if (Application.Current is App app && _profileManager.CurrentProfile != null)
            {
                bool isDark = app.ToggleTheme();
                _profileManager.CurrentProfile.IsDarkTheme = isDark;
                _profileManager.SaveConfiguration();

                if (ThemeButton.Template.FindName("ThemeIcon", ThemeButton) is TextBlock textBlock)
                {
                    textBlock.Text = isDark ? "🌙" : "🔆";
                }
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_profileManager.CurrentProfile != null)
            {
                if (SpeciesControl.SpeciesDetailState.Visibility == Visibility.Visible &&
                    SpeciesControl.DataContext is Organism activeOrganism)
                {
                    _profileManager.CurrentProfile.LastSelectedOrganismId = activeOrganism.Id;
                }
                else
                {
                    _profileManager.CurrentProfile.LastSelectedOrganismId = null;
                }

                _profileManager.SaveConfiguration();
            }
        }

        private void RestoreSessionState()
        {
            if (_profileManager.CurrentProfile == null || _allPeriods == null || !_allPeriods.Any()) return;

            var profile = _profileManager.CurrentProfile;

            if (profile.SearchHistory != null)
            {
                MainSearchBar.InitSearchHistory(profile.SearchHistory);
            }

            string savedPeriodId = profile.LastSelectedPeriodId;
            if (string.IsNullOrEmpty(savedPeriodId))
            {
                savedPeriodId = "triassic";
            }

            SwitchPeriod(savedPeriodId);

            if (!string.IsNullOrEmpty(profile.LastSelectedOrganismId))
            {
                var currentPeriod = _allPeriods.FirstOrDefault(p => p.Id.Equals(savedPeriodId, StringComparison.OrdinalIgnoreCase));
                if (currentPeriod != null)
                {
                    var savedOrganism = currentPeriod.Organisms.FirstOrDefault(o => o.Id.Equals(profile.LastSelectedOrganismId, StringComparison.OrdinalIgnoreCase));
                    if (savedOrganism != null)
                    {
                        SpeciesControl.ShowOrganismDetails(savedOrganism);
                        SpeciesControl.DataContext = savedOrganism;
                    }
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPopupOpenBeforeClick)
            {
                _isPopupOpenBeforeClick = false;
                return;
            }

            var profile = _profileManager.CurrentProfile;
            if (profile != null)
            {
                WelcomeFactsCheckBox.IsChecked = profile.ShowWelcomeFacts;
            }

            SettingsPopup.IsOpen = true;
            _isPopupOpenBeforeClick = true;
        }

        private void SettingsPopup_Closed(object sender, EventArgs e)
        {
            if (!SettingsButton.IsMouseOver)
            {
                _isPopupOpenBeforeClick = false;
            }
        }
    }
}