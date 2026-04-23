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
    /// <summary>
    /// Користувацький елемент керування, що представляє інтерактивну кнопку на шкалі часу.
    /// Містить логіку відображення назви періоду, років та керування візуальним станом активності.
    /// </summary>
    public partial class TimelineButton : UserControl
    {
        /// <summary>
        /// Подія, що виникає при натисканні на кнопку.
        /// </summary>
        public event RoutedEventHandler? Click;

        public TimelineButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Отримує або встановлює заголовок (назву періоду), що відображається на кнопці.
        /// </summary>
        public string Title { get => TitleText.Text; set => TitleText.Text = value; }

        /// <summary>
        /// Отримує або встановлює часовий діапазон, що відображається під назвою.
        /// </summary>
        public string Years { get => YearsText.Text; set => YearsText.Text = value; }

        /// <summary>
        /// Отримує або встановлює колір фону основної частини кнопки.
        /// </summary>
        public Brush ButtonBackground { get => ActionButton.Background; set => ActionButton.Background = value; }

        /// <summary>
        /// Змінює візуальний стан кнопки (активний/неактивний).
        /// Оновлює видимість декоративних елементів, шрифти та колір межі.
        /// </summary>
        /// <param name="isActive">True, якщо період обраний; False для звичайного стану.</param>
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

        /// <summary>
        /// Внутрішній обробник події натискання, що транслює клік на рівень всього компонента.
        /// </summary>
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}