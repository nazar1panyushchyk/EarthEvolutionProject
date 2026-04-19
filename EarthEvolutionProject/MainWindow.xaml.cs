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

        private void OnTimelinePeriodSelected(object sender, string periodId)
        {
            SwitchPeriod(periodId);
        }

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
    }
}