using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EarthEvolutionProject.Models;

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

        public string SearchText => SearchInput.Text;

        public List<string> SelectedTypes => TypeFilter.ItemsSource?
             .Cast<FilterItem>()
             .Where(x => x.IsSelected)
             .Select(x => x.TypeName)
             .ToList() ?? new List<string>();

        public SearchBarView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обробляє зміну тексту в полі пошуку. Керує видимістю підказки (placeholder) 
        /// та ініціює подію оновлення результатів фільтрації.
        /// </summary>
        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(SearchInput.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            UpdateBackButtonVisibility();
            FilterChanged?.Invoke(this, EventArgs.Empty);
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
        /// Обробляє натискання кнопки повернення: очищує всі фільтри та ініціює подію 
        /// виходу з режиму пошуку для повернення до головного контенту.
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SearchInput.Text = string.Empty;
            ResetFilters_Click(null, null);

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
            SearchInput.Text = string.Empty;

            ResetFilters_Click(null, null);

            BackButton.Visibility = Visibility.Collapsed;
        }
    }
} 