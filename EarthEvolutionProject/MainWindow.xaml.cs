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
using Newtonsoft.Json;

namespace EarthEvolutionProject
{
    public partial class MainWindow : Window
    {
        private readonly Services.ProfileManager _profileManager;
        public List<Period> _allPeriods = [];
        public List<string> _allFacts = [];
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

                ComparisonGrid.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// Обробляє подію повного рендерингу вмісту вікна. Виконує первинне заповнення списків фільтрації 
        /// унікальними категоріями організмів та ініціює показ вікна з цікавими фактами.
        /// </summary>
        /// <param name="sender">Джерело події рендерингу.</param>
        /// <param name="e">Аргументи події.</param>
        private void MainWindow_ContentRendered(object? sender, EventArgs e)
        {
            this.ContentRendered -= MainWindow_ContentRendered;

            if (_allPeriods != null && _allPeriods.Any())
            {
                var allTypes = _allPeriods
                    .SelectMany(p => p.Organisms)
                    .Select(o => o.Type)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .Distinct()
                    .OrderBy(t => t);

                SpeciesControl.PopulateComboBoxes(allTypes, _allPeriods);
            }

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

                var dataWrapper = System.Text.Json.JsonSerializer.Deserialize<EvolutionDataWrapper>(jsonString);

                var periods = dataWrapper?.Periods ?? [];

                _allPeriods = periods;
                _allFacts = dataWrapper?.InterestingFacts ?? [];

                foreach (var period in periods)
                {
                    if (period.Organisms != null)
                    {
                        foreach (var organism in period.Organisms)
                        {
                            organism.PeriodName = period.Name;
                            organism.PeriodId = period.Id;
                        }
                    }
                }

                if (periods.Any())
                {
                    var allTypes = periods
                        .SelectMany(p => p.Organisms ?? [])
                        .Select(o => o.Type)
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct()
                        .OrderBy(t => t);

                    MainSearchBar.SetTypes(allTypes);
                }

                RestoreSessionState();

                ComparisonPage.LoadData(periods);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка завантаження даних: {ex.Message}");
            }
        }

        /// <summary>
        /// Виконує збереження поточного стану бази знань та масиву фактів назад у текстовий файл JSON 
        /// із використанням інструментів бібліотеки Newtonsoft.Json та перевіркою наявності цільової директорії.
        /// </summary>
        public void SaveDataToJson()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "evolution_data.json");

                var dataWrapper = new EvolutionDataWrapper
                {
                    InterestingFacts = _allFacts,
                    Periods = _allPeriods
                };

                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dataWrapper, Newtonsoft.Json.Formatting.Indented);

                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, jsonString, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження даних: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Перевіряє налаштування профілю користувача та за потреби обирає випадковий текстовий рядок 
        /// із масиву фактів для відображення у спеціальному модальному вікні при старті.
        /// </summary>
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

        /// <summary>
        /// Обробляє зміну стану прапорця відображення фактів у налаштуваннях екрана, 
        /// синхронізуючи значення з профілем користувача та перезаписуючи конфігурацію.
        /// </summary>
        /// <param name="sender">Елемент керування CheckBox.</param>
        /// <param name="e">Аргументи події зміни стану.</param>
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

            TimelineControl.UpdateActiveButton(periodId);

            SwitchPeriod(periodId);
        }

        /// <summary>
        /// Виконує перемикання контексту даних застосунку на обраний період, оновлює стан кнопок 
        /// на шкалі часу та скидає стан відображення галереї видів.
        /// </summary>
        /// <param name="periodId">Ідентифікатор періоду, який необхідно активувати.</param>
        public void SwitchPeriod(string periodId)
        {
            var selectedPeriod = _allPeriods.FirstOrDefault(p => p.Id.Equals(periodId, StringComparison.OrdinalIgnoreCase));
            if (selectedPeriod != null)
            {
                this.DataContext = selectedPeriod;

                string formattedId = char.ToUpper(periodId[0]) + periodId.Substring(1).ToLower();
                TimelineControl.UpdateActiveButton(formattedId);

                SpeciesControl.DisplayResults(null!);
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
                ComparisonGrid.Visibility = Visibility.Collapsed;
                MainContentGrid.Visibility = Visibility.Visible;
                SearchResultsGrid.Visibility = Visibility.Collapsed;
                TimelinePanel.Visibility = Visibility.Visible;

                if (DataContext is Period p) SpeciesControl.DisplayResults(p.Organisms);
                return;
            }

            ComparisonGrid.Visibility = Visibility.Collapsed;
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
        /// автоматично перемикає програму на відповідний період та відкриває детальку картку обраного організму.
        /// </summary>
        /// <param name="sender">Джерело події вибору.</param>
        /// <param name="selectedOrganism">Об'єкт моделі обраного організму.</param>
        private void OnSearchResultSelected(object sender, Organism selectedOrganism)
        {
            MainSearchBar.ClearAndHide();

            ComparisonGrid.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// Обробляє подію закриття головного вікна програми. Фіксує останню активну сутність організму, 
        /// яка переглядалася користувачем на момент виходу, для її наступного відновлення.
        /// </summary>
        /// <param name="sender">Джерело події закриття.</param>
        /// <param name="e">Аргументи скасування події.</param>
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

        /// <summary>
        /// Відновлює стан користувацького сеансу на основі завантаженого профілю: заповнює історію пошуку, 
        /// активує збережений раніше геологічний період та автоматично відкриває картку останньої істоти.
        /// </summary>
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

        /// <summary>
        /// Обробляє натискання на кнопку налаштувань. Ініціалізує стан елементів керування у спливаючому 
        /// вікні параметрами профілю та відкриває графічний елемент Popup.
        /// </summary>
        /// <param name="sender">Кнопка налаштувань.</param>
        /// <param name="e">Аргументи події кліку.</param>
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

        /// <summary>
        /// Обробляє подію закриття спливаючого вікна налаштувань, скидаючи внутрішній прапорець 
        /// активності для коректної обробки наступних кліків миші.
        /// </summary>
        /// <param name="sender">Компонент Popup налаштувань.</param>
        /// <param name="e">Аргументи події закриття.</param>
        private void SettingsPopup_Closed(object sender, EventArgs e)
        {
            if (!SettingsButton.IsMouseOver)
            {
                _isPopupOpenBeforeClick = false;
            }
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            bool isComparisonOpen = ComparisonGrid.Visibility == Visibility.Visible;

            if (isComparisonOpen)
            {
                ComparisonGrid.Visibility = Visibility.Collapsed;
                MainContentGrid.Visibility = Visibility.Visible;
                TimelinePanel.Visibility = Visibility.Visible;
            }
            else
            {
                ComparisonPage.LoadData(_allPeriods);

                MainContentGrid.Visibility = Visibility.Collapsed;
                SearchResultsGrid.Visibility = Visibility.Collapsed;
                TimelinePanel.Visibility = Visibility.Collapsed;
                ComparisonGrid.Visibility = Visibility.Visible;
            }
        }

        public void NavigateToComparisonWithOrganism(Organism selectedOrganism)
        {
            ComparisonPage.LoadData(_allPeriods);

            MainContentGrid.Visibility = Visibility.Collapsed;
            SearchResultsGrid.Visibility = Visibility.Collapsed;
            TimelinePanel.Visibility = Visibility.Collapsed;

            ComparisonGrid.Visibility = Visibility.Visible;

            ComparisonPage.SelectOrganismForComparison(selectedOrganism);
        }
    }
}