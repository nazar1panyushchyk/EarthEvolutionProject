using EarthEvolutionProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Користувацький елемент керування для пошуку та фільтрації даних. 
    /// Забезпечує введення текстових запитів та вибір категорій організмів через випадаючий список.
    /// </summary>
    public partial class SearchBarView : UserControl
    {
        public event EventHandler? FilterChanged;
        public event EventHandler? BackRequested;
        public event EventHandler? HistoryUpdated;

        private List<string> _searchHistory = new List<string>();

        public string SearchText => SearchInput.Text;

        public List<string> SearchHistoryList => _searchHistory;

        public List<string> SelectedTypes => TypeFilter.ItemsSource?
             .Cast<FilterItem>()
             .Where(x => x.IsSelected)
             .Select(x => x.TypeName)
             .ToList() ?? new List<string>();

        public SearchBarView()
        {
            InitializeComponent();
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(SearchInput.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            UpdateBackButtonVisibility();
        }

        /// <summary>
        /// Завантажує історію пошуку з профілю користувача в інтерфейс пошукового бару.
        /// </summary>
        public void InitSearchHistory(List<string> history)
        {
            _searchHistory = history ?? new List<string>();
            HistoryListBox.ItemsSource = null;
            HistoryListBox.ItemsSource = _searchHistory;
        }

        /// <summary>
        /// Центральний метод запуску пошуку та збереження історії.
        /// Викликається ТІЛЬКИ коли користувач свідомо підтвердив запит.
        /// </summary>
        private void PerformSearch(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                AddQueryToHistory(query);
            }

            HistoryPopup.IsOpen = false;

            UpdateBackButtonVisibility();

            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch(SearchInput.Text);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch(SearchInput.Text);
        }

        private void HistoryListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is string selectedQuery)
            {
                SearchInput.Text = selectedQuery;
                SearchInput.CaretIndex = SearchInput.Text.Length;

                e.Handled = true;

                PerformSearch(selectedQuery);
            }
        }

        private void SearchInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_searchHistory != null && _searchHistory.Any())
            {
                HistoryPopup.IsOpen = true;
            }

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.PreviewMouseDown += ParentWindow_PreviewMouseDown;
            }
        }

        private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(SearchInput.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void SearchInput_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_searchHistory != null && _searchHistory.Any() && !HistoryPopup.IsOpen)
            {
                HistoryPopup.IsOpen = true;

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.PreviewMouseDown -= ParentWindow_PreviewMouseDown;
                    parentWindow.PreviewMouseDown += ParentWindow_PreviewMouseDown;
                }
            }
        }

        private void HistoryPopup_Closed(object sender, EventArgs e)
        {
            HistoryPopup.IsOpen = false;
        }

        private void AddQueryToHistory(string query)
        {
            _searchHistory.Remove(query);

            _searchHistory.Insert(0, query);

            if (_searchHistory.Count > 10)
            {
                _searchHistory.RemoveAt(_searchHistory.Count - 1);
            }

            HistoryListBox.ItemsSource = null;
            HistoryListBox.ItemsSource = _searchHistory;

            HistoryUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            _searchHistory.Clear();
            HistoryListBox.ItemsSource = null;
            HistoryPopup.IsOpen = false;
            HistoryUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ParentWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!SearchInput.IsMouseOver && !HistoryPopup.IsMouseOver)
            {
                HistoryPopup.IsOpen = false;

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    parentWindow.PreviewMouseDown -= ParentWindow_PreviewMouseDown;
                }
            }
        }

        /// <summary>
        /// Обробляє вибір категорій у списку фільтрів. Оновлює стан інтерфейсу 
        /// та ініціює подію зміни параметрів фільтрації.
        /// </summary>
        private void OnFilterCheckBoxClick(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholder();
            UpdateBackButtonVisibility();
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Обробляє натискання лівою кнопкою миші на область випадаючого списку. 
        /// Забезпечує програмне відкриття або закриття переліку категорій при кліку на текстове поле.
        /// </summary>
        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TypeFilter.IsDropDownOpen = !TypeFilter.IsDropDownOpen;
        }

        /// <summary>
        /// Оновлює текстове повідомлення у випадаючому списку залежно від кількості обраних категорій.
        /// </summary>
        private void UpdatePlaceholder()
        {
            if (ComboPlaceholder == null) return;

            int count = SelectedTypes.Count;

            if (count == 0)
            {
                ComboPlaceholder.Text = "Всі категорії";
                ComboPlaceholder.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#88FFFFFF"));
            }
            else
            {
                ComboPlaceholder.Text = $"Обрано: {count}";
                ComboPlaceholder.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        /// <summary>
        /// Заповнює список фільтрів доступними типами організмів, отриманими з бази даних.
        /// </summary>
        /// <param name="types">Колекція унікальних назв типів (категорій) організмів.</param>
        public void SetTypes(IEnumerable<string> types)
        {
            var items = types.Select(t => new FilterItem { TypeName = t }).ToList();
            TypeFilter.ItemsSource = items;
            UpdatePlaceholder();
        }

        /// <summary>
        /// Скидає всі обрані фільтри та текстове поле пошуку до початкового стану.
        /// </summary>
        private void ResetFilters_Click(object? sender, RoutedEventArgs? e)
        {
            if (TypeFilter.ItemsSource is List<FilterItem> items)
            {
                foreach (var item in items)
                {
                    item.IsSelected = false;
                }

                TypeFilter.ItemsSource = null;
                TypeFilter.ItemsSource = items;

                UpdatePlaceholder();
                UpdateBackButtonVisibility();
                FilterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обробляє натискання кнопки повернення: зберігає введений текст для зручності користувача,
        /// але ховає кнопку "Назад" та ініціює подію виходу з режиму пошуку.
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryPopup != null)
            {
                HistoryPopup.IsOpen = false;
            }

            if (BackButton != null)
            {
                BackButton.Visibility = Visibility.Collapsed;
            }

            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Керує видимістю кнопки "Назад" залежно від наявності введеного тексту або обраних категорій фільтрації.
        /// </summary>
        private void UpdateBackButtonVisibility()
        {
            if (BackButton == null) return;

            bool isSearching = !string.IsNullOrWhiteSpace(SearchInput.Text) || SelectedTypes.Any();

            BackButton.Visibility = isSearching ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Виконує повне очищення параметрів пошуку та примусово приховує кнопку повернення.
        /// </summary>
        public void ClearAndHide()
        {
            //SearchInput.Text = string.Empty;

            ResetFilters_Click(null, null);

            if (BackButton != null)
            {
                BackButton.Visibility = Visibility.Collapsed;
            }
        }
    }
} 