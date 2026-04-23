using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Користувацький елемент керування, що представляє галерею біологічних видів. 
    /// Забезпечує відображення списку організмів та перегляд детальної інформації про обрану істоту.
    /// </summary>
    public partial class SpeciesGalleryView : UserControl
    {
        public SpeciesGalleryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обробляє натискання на картку організму. Витягує дані з контексту об'єкта та 
        /// перемикає інтерфейс у режим відображення детальної інформації.
        /// </summary>
        private void OrganismCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
            {
                var organism = element.DataContext;

                string commonName = GetPropertyValue(organism, "CommonName") ?? "Без назви";
                string scientificName = GetPropertyValue(organism, "ScientificName") ?? "";
                string type = GetPropertyValue(organism, "Type") ?? "";
                string existence = GetPropertyValue(organism, "Existence") ?? "Період не вказано";
                string lifestyle = GetPropertyValue(organism, "Lifestyle") ?? "Опис буде додано згодом.";
                string? imagePath = GetPropertyValue(organism, "Image");

                DetailCommonName.Text = commonName;
                DetailScientificName.Text = scientificName;
                DetailType.Text = type;
                DetailExistence.Text = existence;
                DetailLifestyle.Text = lifestyle;

                try
                {
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        DetailImage.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                    }
                    else
                    {
                        DetailImage.Source = null;
                    }
                }
                catch
                {
                    DetailImage.Source = null;
                }

                GalleryListState.Visibility = Visibility.Collapsed;
                SpeciesDetailState.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Допоміжний метод для безпечного отримання значень властивостей об'єкта. 
        /// Підтримує роботу як зі звичайними класами C#, так і з динамічними елементами JsonElement.
        /// </summary>
        private string? GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop != null)
                {
                    return prop.GetValue(obj)?.ToString();
                }

                if (obj is JsonElement element && element.TryGetProperty(propertyName, out JsonElement value))
                {
                    return value.GetString();
                }
            }
            catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Помилка доступу до властивості {propertyName}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Обробник події натискання кнопки повернення. Приховує панель деталей та 
        /// повертає користувача до загального списку галереї.
        /// </summary>
        private void BackToList_Click(object sender, RoutedEventArgs e)
        {
            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Скидає стан модуля до початкового вигляду галереї, оновлюючи джерело даних 
        /// відповідно до поточного обраного періоду.
        /// </summary>
        public void ResetToGallery()
        {
            if (this.DataContext is EarthEvolutionProject.Models.Period period)
            {
                SpeciesItemsControl.ItemsSource = period.Organisms;
            }

            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Відображає передану колекцію елементів у галереї. Використовується для 
        /// виведення результатів фільтрації або пошуку.
        /// </summary>
        public void DisplayResults(System.Collections.IEnumerable items)
        {
            SpeciesItemsControl.ItemsSource = items;

            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Примусово відкриває панель детальної інформації для конкретного об'єкта організму.
        /// </summary>
        public void ShowOrganismDetails(EarthEvolutionProject.Models.Organism organism)
        {
            if (organism == null) return;

            DetailCommonName.Text = organism.CommonName;
            DetailScientificName.Text = organism.ScientificName;
            DetailType.Text = organism.Type;
            DetailExistence.Text = organism.Existence;
            DetailLifestyle.Text = organism.Lifestyle;

            try
            {
                if (!string.IsNullOrEmpty(organism.Image))
                {
                    DetailImage.Source = new BitmapImage(new Uri(organism.Image, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    DetailImage.Source = null;
                }
            }
            catch
            {
                DetailImage.Source = null;
            }

            GalleryListState.Visibility = Visibility.Collapsed;
            SpeciesDetailState.Visibility = Visibility.Visible;
        }
    }
}