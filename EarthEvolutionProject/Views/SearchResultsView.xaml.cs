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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using EarthEvolutionProject.Models;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Користувацький елемент керування для відображення результатів пошуку.
    /// Забезпечує виведення списку знайдених організмів та обробку вибору конкретного об'єкта.
    /// </summary>
    public partial class SearchResultsView : UserControl
    {
        public event EventHandler<Organism>? AnimalSelected;

        public SearchResultsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Оновлює список результатів на екрані. Встановлює джерело даних для контейнера 
        /// та керує видимістю повідомлення про порожній результат, якщо нічого не знайдено.
        /// </summary>
        /// <param name="results">Список об'єктів Organism, що відповідають критеріям пошуку.</param>
        public void DisplayResults(List<Organism> results)
        {
            ResultsItemsControl.ItemsSource = results;

            if (results == null || !results.Any())
            {
                EmptyMessage.Visibility = System.Windows.Visibility.Visible;
                ResultsItemsControl.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                EmptyMessage.Visibility = System.Windows.Visibility.Collapsed;
                ResultsItemsControl.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Обробляє натискання на картку організму. Витягує дані про істоту з контексту 
        /// елемента та ініціює подію AnimalSelected.
        /// </summary>
        private void OnCardClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Organism animal)
            {
                AnimalSelected?.Invoke(this, animal);
            }
        }
    }
}
