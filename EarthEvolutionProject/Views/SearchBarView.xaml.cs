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
        /// <summary>
        /// Подія, що виникає при зміні тексту пошуку або стану обраних фільтрів категорій.
        /// </summary>
        public event EventHandler? FilterChanged;

        /// <summary>
        /// Подія, що сигналізує про запит користувача вийти з режиму пошуку та повернутися назад.
        /// </summary>
        public event EventHandler? BackRequested;

        /// <summary>
        /// Подія, яка генерується після успішного додавання або повного очищення елементів історії пошукових запитів.
        /// </summary>
        public event EventHandler? HistoryUpdated;

        private List<string> _searchHistory = new List<string>();

        /// <summary>
        /// Повертає поточний текстовий рядок, введений користувачем у поле пошукового введення.
        /// </summary>
        public string SearchText => SearchInput.Text;

        /// <summary>
        /// Повертає повний внутрішній список збережених раніше текстових пошукових запитів користувача.
        /// </summary>
        public List<string> SearchHistoryList => _searchHistory;

        /// <summary>
        /// Повертає колекцію текстових назв категорій (типів) організмів, які зараз відмічені у випадаючому списку фільтрації.
        /// </summary>
        public List<string> SelectedTypes => TypeFilter.ItemsSource?
             .Cast<FilterItem>()
             .Where(x => x.IsSelected)
             .Select(x => x.TypeName)
             .ToList() ?? new List<string>();

        /// <summary>
        /// Конструктор користувацького елемента пошукової панелі. Виконує базову ініціалізацію графічних підсистем XAML.
        /// </summary>
        public SearchBarView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обробляє динамічну зміну тексту в полі введення, регулюючи видимість підказки-плейсхолдера та кнопки повернення.
        /// </summary>
        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(SearchInput.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
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

        /// <summary>
        /// Обробляє натискання клавіш у полі введення, ініціюючи процес пошуку при натисканні клавіші Enter.
        /// </summary>
        private void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch(SearchInput.Text);
            }
        }

        /// <summary>
        /// Обробляє клік по графічній кнопці із зображенням лупи, запускаючи виконання поточної процедури пошуку.
        /// </summary>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch(SearchInput.Text);
        }

        /// <summary>
        /// Перехоплює клік миші по елементу списку історії, підставляючи вибраний текст у поле введення та активуючи пошук.
        /// </summary>
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

        /// <summary>
        /// Обробляє отримання фокусу полем введення, відображаючи випадаюче вікно історії та підписуючись на кліки по головному вікну.
        /// </summary>
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

        /// <summary>
        /// Обробляє втрату фокусу полем введення пошуку, примусово відновлюючи видимість фонового тексту підказки.
        /// </summary>
        private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(SearchInput.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Обробляє початкове натискання лівої кнопки миші на полі пошуку для коректного відображення або оновлення прив'язок спливаючого вікна.
        /// </summary>
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

        /// <summary>
        /// Обробник події повного закриття спливаючого графічного контейнера PopUp із переліком попередніх запитів.
        /// </summary>
        private void HistoryPopup_Closed(object sender, EventArgs e)
        {
            HistoryPopup.IsOpen = false;
        }

        /// <summary>
        /// Додає новий текстовий запит на початок списку історії, видаляє дублікати та контролює обмеження розміру списку до десяти елементів.
        /// </summary>
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

        /// <summary>
        /// Повністю очищує масив збережених раніше текстових запитів користувача та оновлює стан пов'язаних елементів списку.
        /// </summary>
        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            _searchHistory.Clear();
            HistoryListBox.ItemsSource = null;
            HistoryPopup.IsOpen = false;
            HistoryUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Відстежує кліки по всій площині головного вікна програми для автоматичного приховування вікна історії, якщо користувач клікнув ззовні.
        /// </summary>
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
        /// Керує видимістю кнопки "Назад" залежно від наявності введеного текста або обраних категорій фільтрації.
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
            ResetFilters_Click(null, null);

            if (BackButton != null)
            {
                BackButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}