using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Windows.Media;

namespace EarthEvolutionProject
{
    public partial class MainWindow : Window
    {
        private List<Period> _allPeriods = [];

        public MainWindow()
        {
            InitializeComponent();
            InitializeAppData();
        }

        private void InitializeAppData()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "evolution_data.json");
                string jsonString = File.ReadAllText(filePath);

                _allPeriods = JsonSerializer.Deserialize<List<Period>>(jsonString) ?? [];

                SwitchPeriod("triassic");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка ініціалізації: " + ex.Message);
            }
        }

        private void SwitchPeriod(string periodId)
        {
            var selectedPeriod = _allPeriods.FirstOrDefault(p => p.Id == periodId);
            if (selectedPeriod != null)
            {
                this.DataContext = selectedPeriod;
                UpdateActiveButton(periodId);
            }
        }

        private void PeriodButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                string? periodId = button.Tag.ToString();
                if (!string.IsNullOrEmpty(periodId))
                {
                    SwitchPeriod(periodId);
                }
            }
        }

        private void PeriodButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                switch (btn.Name)
                {
                    case "BtnTriassic":
                        btn.Background = new SolidColorBrush(Color.FromRgb(58, 49, 66));
                        break;
                    case "BtnJurassic":
                        btn.Background = new SolidColorBrush(Color.FromRgb(34, 97, 119));
                        break;
                    case "BtnCretaceous":
                        btn.Background = new SolidColorBrush(Color.FromRgb(90, 109, 56));
                        break;
                    case "BtnPaleogene":
                        btn.Background = new SolidColorBrush(Color.FromRgb(120, 72, 41));
                        break;
                    case "BtnNeogene":
                        btn.Background = new SolidColorBrush(Color.FromRgb(136, 112, 40));
                        break;
                    case "BtnAnthropogene":
                        btn.Background = new SolidColorBrush(Color.FromRgb(95, 106, 116));
                        break;
                }
            }
        }

        private void PeriodButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                switch (btn.Name)
                {
                    case "BtnTriassic":
                        btn.Background = new SolidColorBrush(Color.FromRgb(75, 64, 84));
                        break;
                    case "BtnJurassic":
                        btn.Background = new SolidColorBrush(Color.FromRgb(43, 119, 147));
                        break;
                    case "BtnCretaceous":
                        btn.Background = new SolidColorBrush(Color.FromRgb(109, 132, 68));
                        break;
                    case "BtnPaleogene":
                        btn.Background = new SolidColorBrush(Color.FromRgb(145, 88, 50));
                        break;
                    case "BtnNeogene":
                        btn.Background = new SolidColorBrush(Color.FromRgb(165, 137, 52));
                        break;
                    case "BtnAnthropogene":
                        btn.Background = new SolidColorBrush(Color.FromRgb(115, 128, 140));
                        break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вікно фільтрів у розробці!");
        }

        private void UpdateActiveButton(string activeId)
        {
            string[] ids = { "Triassic", "Jurassic", "Cretaceous", "Paleogene", "Neogene", "Anthropogene" };

            foreach (var id in ids)
            {
                var button = this.FindName($"Btn{id}") as Button;
                var arrow = this.FindName($"Arrow{id}") as UIElement;
                var text = this.FindName($"Text{id}") as TextBlock;

                if (button != null && arrow != null && text != null)
                {
                    bool isActive = id.Equals(activeId, StringComparison.OrdinalIgnoreCase);

                    if (isActive)
                    {
                        button.BorderBrush = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(167, 190, 75));
                        button.BorderThickness = new Thickness(3);

                        arrow.Visibility = Visibility.Visible;

                        text.FontWeight = FontWeights.Bold;
                        text.FontSize = 14;
                    }
                    else
                    {
                        button.BorderBrush = System.Windows.Media.Brushes.Transparent;
                        button.BorderThickness = new Thickness(3);

                        arrow.Visibility = Visibility.Collapsed;

                        text.FontWeight = FontWeights.SemiBold;
                        text.FontSize = 13;
                    }
                }
            }
        }
    }
}