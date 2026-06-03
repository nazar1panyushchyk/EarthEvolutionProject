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
using System.Windows.Shapes;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Діалогове вікно для відображення випадкових палеонтологічних або історичних фактів.
    /// Забезпечує інтерактивну взаємодію з користувачем та збереження налаштувань показу.
    /// </summary>
    public partial class FactWindow : Window
    {
        /// <summary>
        /// Повертає логічне значення, яке вказує, чи відмітив користувач прапорець відмови від подальшого відображення вікна з фактами.
        /// </summary>
        public bool DontShowAgain => DontShowAgainCheckBox.IsChecked ?? false;

        /// <summary>
        /// Конструктор вікна відображення фактів. Ініціалізує графічні компоненти XAML та встановлює переданий текстовий рядок у поле відображення.
        /// </summary>
        /// <param name="randomFact">Текстовий рядок, що містить інформаційне повідомлення або випадковий факт.</param>
        public FactWindow(string randomFact)
        {
            InitializeComponent();
            FactTextBlock.Text = randomFact;
        }

        /// <summary>
        /// Обробляє натискання на графічну кнопку закриття вікна, встановлюючи позитивний результат діалогу та завершуючи роботу форми.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Перехоплює натискання клавіш на рівні вікна, забезпечуючи його швидке закриття при натисканні клавіші Escape.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Обробляє натискання миші на зовнішню фонову область контейнера, дозволяючи закривати вікно кліком поза межами інформаційного блоку.
        /// </summary>
        private void OutsideGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Перехоплює та зупиняє подальше поширення події натискання миші в межах інформаційної рамки, запобігаючи випадковому закриттю вікна.
        /// </summary>
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}