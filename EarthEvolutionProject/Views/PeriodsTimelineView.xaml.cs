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
    /// <summary>
    /// Користувацький елемент керування, що представляє інтерактивну шкалу часу. 
    /// Забезпечує візуалізацію та вибір геологічних періодів у межах інтерфейсу застосунку.
    /// </summary>
    public partial class PeriodsTimelineView : UserControl
    {
        /// <summary>
        /// Подія, що виникає при виборі конкретного періоду на шкалі часу. 
        /// Передає ідентифікатор обраного періоду підписникам (зазвичай головному вікну).
        /// </summary>
        public event EventHandler<string>? PeriodSelected;

        private List<PeriodViewModel> _periods;

        /// <summary>
        /// Внутрішня модель представлення для відображення картки окремого геологічного періоду 
        /// на інтерактивній панелі шкали часу.
        /// </summary>
        public class PeriodViewModel
        {
            public string Title { get; set; } = string.Empty;
            public string Years { get; set; } = string.Empty;
            public Brush BackgroundColor { get; set; } = Brushes.Transparent;
            public string Tag { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        /// <summary>
        /// Конструктор елемента керування шкалою часу. Виконує ініціалізацію візуальних компонентів, 
        /// визначених у XAML-розмітці.
        /// </summary>
        public PeriodsTimelineView()
        {
            InitializeComponent();

            var bc = new BrushConverter();

            _periods = new List<PeriodViewModel>
            {
             new PeriodViewModel { Title = "ТРІАСОВИЙ", Years = "252 – 201.3 млн", BackgroundColor = (bc.ConvertFrom("#4B4054") as Brush) ?? Brushes.Transparent, Tag = "Triassic" },
             new PeriodViewModel { Title = "ЮРСЬКИЙ", Years = "201.3 – 145 млн", BackgroundColor = (bc.ConvertFrom("#2B7793") as Brush) ?? Brushes.Transparent, Tag = "Jurassic" },
             new PeriodViewModel { Title = "КРЕЙДОВИЙ", Years = "145 – 66 млн", BackgroundColor = (bc.ConvertFrom("#6D8444") as Brush) ?? Brushes.Transparent, Tag = "Cretaceous" },
             new PeriodViewModel { Title = "ПАЛЕОГЕН", Years = "66 – 23 млн", BackgroundColor = (bc.ConvertFrom("#915832") as Brush) ?? Brushes.Transparent, Tag = "Paleogene" },
             new PeriodViewModel { Title = "НЕОГЕН", Years = "23 – 2.5 млн", BackgroundColor = (bc.ConvertFrom("#A58934") as Brush) ?? Brushes.Transparent, Tag = "Neogene" },
             new PeriodViewModel { Title = "АНТРОПОГЕН", Years = "2.5 млн – н.ч.", BackgroundColor = (bc.ConvertFrom("#73808C") as Brush) ?? Brushes.Transparent, Tag = "Anthropogene" }
            };

            TimelineItemsControl.ItemsSource = _periods;
        }

        /// <summary>
        /// Обробник натискання на кнопку періоду. Визначає ідентифікатор періоду через властивість Tag кнопки 
        /// та ініціює подію PeriodSelected.
        /// </summary>
        /// <param name="sender">Об'єкт кнопки TimelineButton, на яку було здійснено натискання.</param>
        /// <param name="e">Аргументи події маршрутизації.</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is TimelineButton clickedButton && clickedButton.DataContext is PeriodViewModel periodData)
            {
                string tag = periodData.Tag;

                UpdateActiveButton(tag);

                PeriodSelected?.Invoke(this, tag);
            }
        }

        /// <summary>
        /// Оновлює логічний стан активності елементів шкали часу, порівнюючи їхні унікальні ідентифікатори 
        /// із переданим кодом, та примусово оновлює пов’язаний візуальний компонент керування.
        /// </summary>
        /// <param name="activeId">Текстовий ідентифікатор геологічного періоду, який стає активним.</param>
        public void UpdateActiveButton(string activeId)
        {
            foreach (var period in _periods)
            {
                period.IsActive = period.Tag.Equals(activeId, StringComparison.OrdinalIgnoreCase);
            }

            TimelineItemsControl.Items.Refresh();
        }
    }
}