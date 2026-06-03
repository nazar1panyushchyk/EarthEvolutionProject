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
    public partial class OrganismCardView : UserControl
    {
        /// <summary>
        /// Конструктор елемента керування карткою організму. Ініціалізує компоненти XAML 
        /// та підписується на події динамічної зміни розмірів батьківського вікна застосунку.
        /// </summary>
        public OrganismCardView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                    window.SizeChanged += (ws, we) => UpdateImageSize(window.ActualHeight);

                UpdateImageSize(Window.GetWindow(this)?.ActualHeight ?? 600);
            };
        }

        /// <summary>
        /// Динамічно коригує максимальну висоту графічного зображення картки CardImage 
        /// залежно від поточних вертикальних габаритів головного вікна програми.
        /// </summary>
        /// <param name="windowHeight">Поточне значення фактичної висоти батьківського вікна.</param>
        private void UpdateImageSize(double windowHeight)
        {
            CardImage.MaxHeight = windowHeight > 700 ? 100 : 80;
        }
    }
}
