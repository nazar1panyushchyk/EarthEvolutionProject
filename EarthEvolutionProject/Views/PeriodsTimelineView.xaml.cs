using EarthEvolutionProject.Controls;
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

namespace EarthEvolutionProject.Views
{
    public partial class PeriodsTimelineView : UserControl
    {
        public event EventHandler<string>? PeriodSelected;

        public PeriodsTimelineView()
        {
            InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is TimelineButton btn && btn.Tag is string periodId)
            {
                PeriodSelected?.Invoke(this, periodId);
            }
        }

        public void UpdateActiveButton(string activeId)
        {
            string[] ids = { "Triassic", "Jurassic", "Cretaceous", "Paleogene", "Neogene", "Anthropogene" };

            foreach (var id in ids)
            {
                if (this.FindName($"Btn{id}") is TimelineButton btn)
                {
                    btn.SetActive(id.Equals(activeId, StringComparison.OrdinalIgnoreCase));
                }
            }
        }
    }
}