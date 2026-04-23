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

        /// <summary>
        /// Конструктор елемента керування шкалою часу. Виконує ініціалізацію візуальних компонентів, 
        /// визначених у XAML-розмітці.
        /// </summary>
        public PeriodsTimelineView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обробник натискання на кнопку періоду. Визначає ідентифікатор періоду через властивість Tag кнопки 
        /// та ініціює подію PeriodSelected.
        /// </summary>
        /// <param name="sender">Об'єкт кнопки TimelineButton, на яку було здійснено натискання.</param>
        /// <param name="e">Аргументи події маршрутизації.</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is TimelineButton btn && btn.Tag is string periodId)
            {
                PeriodSelected?.Invoke(this, periodId);
            }
        }

        /// <summary>
        /// Оновлює візуальний стан кнопок на шкалі часу, встановлюючи активний стан для вибраного періоду 
        /// та скидаючи його для всіх інших кнопок.
        /// </summary>
        /// <param name="activeId">Ідентифікатор періоду, який має стати активним (виділеним).</param>
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