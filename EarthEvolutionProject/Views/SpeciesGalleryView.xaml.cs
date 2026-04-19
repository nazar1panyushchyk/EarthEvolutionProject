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
    public partial class SpeciesGalleryView : UserControl
    {
        public SpeciesGalleryView()
        {
            InitializeComponent();
        }

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
            catch { }

            return null;
        }

        private void BackToList_Click(object sender, RoutedEventArgs e)
        {
            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }
        
        public void ResetToGallery()
        {
            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }
    }
}
