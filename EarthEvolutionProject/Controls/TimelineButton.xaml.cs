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

namespace EarthEvolutionProject.Controls
{
    public partial class TimelineButton : UserControl
    {
        public event RoutedEventHandler? Click;

        public TimelineButton()
        {
            InitializeComponent();
        }

        public string Title { get => TitleText.Text; set => TitleText.Text = value; }
        public string Years { get => YearsText.Text; set => YearsText.Text = value; }
        public Brush ButtonBackground { get => ActionButton.Background; set => ActionButton.Background = value; }

        public void SetActive(bool isActive)
        {
            ActiveArrow.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
            ActiveFrame.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;

            ActionButton.BorderBrush = isActive
                ? new SolidColorBrush(Color.FromRgb(167, 190, 75))
                : Brushes.Transparent;

            TitleText.FontWeight = isActive ? FontWeights.Bold : FontWeights.SemiBold;
            TitleText.FontSize = isActive ? 14 : 13;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}