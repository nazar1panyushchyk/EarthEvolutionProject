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
using Microsoft.Win32;
using EarthEvolutionProject.Models;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Користувацький елемент керування, що представляє галерею біологічних видів. 
    /// Забезпечує відображення списку організмів та перегляд детальної інформації про обрану істоту.
    /// </summary>
    public partial class SpeciesGalleryView : UserControl
    {
        private Organism? _editingOrganism = null;
        private Period? _currentPeriod;
        private List<Period> _allPeriods = [];
        private bool _isEditMode = false;

        /// <summary>
        /// Подія, що виникає при виборі конкретного організму або скиданні вибору в межах галереї.
        /// </summary>
        public event EventHandler<Organism?>? OrganismSelected;

        /// <summary>
        /// Конструктор користувацького елемента керування. Ініціалізує всі графічні компоненти галереї видів.
        /// </summary>
        public SpeciesGalleryView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обробляє натискання на картку організму. Витягує дані з контексту об'єкта та 
        /// перемикає інтерфейс у режим відображення детальної інформації.
        /// </summary>
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

                this.DataContext = element.DataContext;
                if (element.DataContext is Organism org)
                {
                    OrganismSelected?.Invoke(this, org);
                }
            }
        }

        /// <summary>
        /// Допоміжний метод для безпечного отримання значень властивостей об'єкта. 
        /// Підтримує роботу як зі звичайними класами C#, так і з динамічними елементами JsonElement.
        /// </summary>
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка доступу до властивості {propertyName}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Обробник події натискання кнопки повернення. Приховує панель деталей та 
        /// повертає користувача до загального списку галереї.
        /// </summary>
        private void BackToList_Click(object sender, RoutedEventArgs e)
        {
            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;

            OrganismSelected?.Invoke(this, null!);
        }

        /// <summary>
        /// Скидає стан модуля до початкового вигляду галереї, оновлюючи джерело даних 
        /// відповідно до поточного обраного періоду.
        /// </summary>
        public void ResetToGallery()
        {
            if (this.DataContext is EarthEvolutionProject.Models.Period period)
            {
                SpeciesItemsControl.ItemsSource = period.Organisms;
                UpdateScrollViewer();
            }

            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Відображає передану колекцію елементів у галереї. Використовується для 
        /// виведення результатів фільтрації або пошуку.
        /// </summary>
        public void DisplayResults(System.Collections.IEnumerable items)
        {
            SpeciesItemsControl.ItemsSource = items;

            if (items is IEnumerable<Organism> organisms && organisms.Any())
            {
                string periodId = organisms.First().PeriodId;

                _currentPeriod = _allPeriods
                    .FirstOrDefault(p => p.Id == periodId);
            }

            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;

            UpdateScrollViewer();
        }

        /// <summary>
        /// Примусово відкриває панель детальної інформації для конкретного об'єкта організму.
        /// </summary>
        public void ShowOrganismDetails(EarthEvolutionProject.Models.Organism organism)
        {
            if (organism == null) return;

            _currentPeriod = _allPeriods.FirstOrDefault(p => p.Id.Equals(organism.PeriodId, StringComparison.OrdinalIgnoreCase));

            DetailCommonName.Text = organism.CommonName;
            DetailScientificName.Text = organism.ScientificName;
            DetailType.Text = organism.Type;
            DetailExistence.Text = organism.Existence;
            DetailLifestyle.Text = organism.Lifestyle;

            try
            {
                if (!string.IsNullOrEmpty(organism.Image))
                {
                    DetailImage.Source = new BitmapImage(new Uri(organism.Image, UriKind.RelativeOrAbsolute));
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

            this.DataContext = organism;
        }

        /// <summary>
        /// Заповнює випадаючі списки категорій організмів та геологічних періодів для форми редагування.
        /// </summary>
        public void PopulateComboBoxes(IEnumerable<string> types, IEnumerable<Period> periods)
        {
            _allPeriods = periods.ToList();

            TypeComboBox.ItemsSource = types.ToList();
            PeriodComboBox.ItemsSource = _allPeriods;
        }

        /// <summary>
        /// Обробник події кліку для додавання організму. Скидає поля форми та переводить вікно в режим створення.
        /// </summary>
        private void AddOrganismButton_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = false;
            _editingOrganism = null;
            FormTitleTextBlock.Text = "Додавання нового організму";

            ClearFormFields();

            if (_currentPeriod != null)
            {
                PeriodComboBox.SelectedValue = _currentPeriod.Id;
            }

            GalleryListState.Visibility = Visibility.Collapsed;
            SpeciesDetailState.Visibility = Visibility.Collapsed;
            CrudFormGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обробник події кліку для редагування організму. Заповнює форму поточними даними та парсить часові межі.
        /// </summary>
        private void EditOrganismButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is Organism organism)
            {
                _isEditMode = true;
                _editingOrganism = organism;
                FormTitleTextBlock.Text = $"Редагування: {organism.CommonName}";

                CommonNameTextBox.Text = organism.CommonName;
                ScientificNameTextBox.Text = organism.ScientificName;
                TypeComboBox.SelectedItem = organism.Type;
                PeriodComboBox.SelectedValue = organism.PeriodId;
                LifestyleTextBox.Text = organism.Lifestyle;
                ImagePathTextBox.Text = organism.Image;

                try
                {
                    string savedExistence = organism.Existence ?? "";

                    if (savedExistence.Contains("-"))
                    {
                        IsRangeCheckBox.IsChecked = true;
                        IsRangeCheckBox_Changed(null!, null!);

                        string[] parts = savedExistence.Split('-');
                        string left = parts[0].Trim();
                        string right = parts[1].Trim();

                        string leftClean = left.Replace(" років тому", "").Trim();
                        string[] leftWords = leftClean.Split(' ');
                        FromValueTextBox.Text = leftWords[0];
                        SetComboBoxValue(FromUnitComboBox, leftWords.Length > 1 ? leftWords[1] : "млн");

                        if (right.Equals("н.ч", StringComparison.OrdinalIgnoreCase))
                        {
                            SetComboBoxValue(ToUnitComboBox, "н.ч");
                        }
                        else
                        {
                            string rightClean = right.Replace(" років тому", "").Trim();
                            string[] rightWords = rightClean.Split(' ');
                            ToValueTextBox.Text = rightWords[0];
                            SetComboBoxValue(ToUnitComboBox, rightWords.Length > 1 ? rightWords[1] : "млн");
                        }
                    }
                    else
                    {
                        IsRangeCheckBox.IsChecked = false;
                        IsRangeCheckBox_Changed(null!, null!);

                        string clean = savedExistence.Replace(" років тому", "").Trim();
                        string[] words = clean.Split(' ');
                        FromValueTextBox.Text = words[0];
                        SetComboBoxValue(FromUnitComboBox, words.Length > 1 ? words[1] : "млн");
                    }
                }
                catch
                {
                    IsRangeCheckBox.IsChecked = false;
                    IsRangeCheckBox_Changed(null!, null!);
                    FromValueTextBox.Text = organism.Existence;
                    FromUnitComboBox.SelectedIndex = 0;
                }

                GalleryListState.Visibility = Visibility.Collapsed;
                SpeciesDetailState.Visibility = Visibility.Collapsed;
                CrudFormGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Відкриває діалогове вікно операційної системи для вибору графічного файлу зображення істоти.
        /// </summary>
        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Зображення (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Усі файли (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePathTextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Валідує введені дані форми, збирає фінальний рядок існування, створює або оновлює 
        /// об'єкт організму і викликає збереження до файлу конфігурації.
        /// </summary>
        private void SaveFormButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommonNameTextBox.Text) || PeriodComboBox.SelectedValue == null)
            {
                MessageBox.Show("Будь ласка, заповніть обов'язкові поля: Назву та Період.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fromVal = FromValueTextBox.Text.Trim().Replace(',', '.');
            string fromUnit = (FromUnitComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "млн";

            if (!double.TryParse(fromVal, out _))
            {
                MessageBox.Show("Будь ласка, введіть коректне числове значення у перше поле часу.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string finalExistenceString = "";

            if (IsRangeCheckBox.IsChecked == true)
            {
                string toVal = ToValueTextBox.Text.Trim().Replace(',', '.');
                string toUnit = (ToUnitComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "млн";

                if (toUnit == "н.ч")
                {
                    // Форматуємо без нулів та зайвих слів для "нашого часу"
                    finalExistenceString = $"{fromVal} {fromUnit} років тому - н.ч";
                }
                else
                {
                    if (!double.TryParse(toVal, out _))
                    {
                        MessageBox.Show("Ви обрали діапазон, але не ввели коректне числове значення у друге поле (До).", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    finalExistenceString = $"{fromVal} {fromUnit} - {toVal} {toUnit} років тому";
                }
            }
            else
            {
                finalExistenceString = $"{fromVal} {fromUnit} років тому";
            }

            string actionText = _isEditMode ? "зберегти зміни в цьому організмі" : "додати цей новий вид до проєкту";

            MessageBoxResult result = MessageBox.Show($"Ви впевнені, що хочете {actionText}?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (window == null || window._allPeriods == null)
                {
                    MessageBox.Show("Помилка: Головне вікно програми недоступне.", "Помилка критична", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string? selectedPeriodId = PeriodComboBox.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(selectedPeriodId))
                {
                    MessageBox.Show("Не вдалося визначити ID обраного періоду.");
                    return;
                }

                var globalTargetPeriod = window._allPeriods.FirstOrDefault(p => p.Id.Equals(selectedPeriodId, StringComparison.OrdinalIgnoreCase));
                if (globalTargetPeriod == null)
                {
                    MessageBox.Show($"Критична помилка: Період з ID '{selectedPeriodId}' не знайдено в базі даних головного вікна.");
                    return;
                }

                if (_isEditMode && _editingOrganism != null)
                {
                    if (!_editingOrganism.PeriodId.Equals(globalTargetPeriod.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        var oldPeriod = window._allPeriods.FirstOrDefault(
                            p => p.Id.Equals(_editingOrganism.PeriodId, StringComparison.OrdinalIgnoreCase));

                        if (oldPeriod != null)
                        {
                            var itemToRemove = oldPeriod.Organisms.FirstOrDefault(o => o.Id == _editingOrganism.Id);
                            if (itemToRemove != null)
                            {
                                oldPeriod.Organisms.Remove(itemToRemove);
                            }
                        }

                        globalTargetPeriod.Organisms.Add(_editingOrganism);
                    }

                    _editingOrganism.CommonName = CommonNameTextBox.Text;
                    _editingOrganism.ScientificName = ScientificNameTextBox.Text;
                    _editingOrganism.Type = TypeComboBox.SelectedItem?.ToString() ?? "Невідомо";
                    _editingOrganism.PeriodId = globalTargetPeriod.Id;
                    _editingOrganism.PeriodName = globalTargetPeriod.Name;
                    _editingOrganism.Existence = finalExistenceString;
                    _editingOrganism.Lifestyle = LifestyleTextBox.Text;
                    _editingOrganism.Image = ImagePathTextBox.Text;
                }
                else
                {
                    Organism newOrganism = new Organism
                    {
                        Id = "org_" + Guid.NewGuid().ToString().Substring(0, 8),
                        CommonName = CommonNameTextBox.Text,
                        ScientificName = ScientificNameTextBox.Text,
                        Type = TypeComboBox.SelectedItem?.ToString() ?? "Невідомо",
                        PeriodId = globalTargetPeriod.Id,
                        PeriodName = globalTargetPeriod.Name,
                        Existence = finalExistenceString,
                        Lifestyle = LifestyleTextBox.Text,
                        Image = string.IsNullOrWhiteSpace(ImagePathTextBox.Text)
                            ? "/Images/Organisms/default.png"
                            : ImagePathTextBox.Text
                    };

                    globalTargetPeriod.Organisms.Add(newOrganism);
                }

                window.SaveDataToJson();

                _currentPeriod = globalTargetPeriod;
                this.DataContext = globalTargetPeriod;

                SpeciesItemsControl.ItemsSource = null;

                window.SwitchPeriod(globalTargetPeriod.Id);

                CrudFormGrid.Visibility = Visibility.Collapsed;
                SpeciesDetailState.Visibility = Visibility.Collapsed;
                GalleryListState.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Встановлює вибраний елемент у випадаючому списку ComboBox на основі збігу текстового рядка.
        /// </summary>
        private void SetComboBoxValue(ComboBox comboBox, string text)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == text)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        /// <summary>
        /// Обробляє запит на повне видалення організму з бази даних та оновлює стан пов'язаних модулів.
        /// </summary>
        private void DeleteOrganismButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is Organism organism)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Ви впевнені, що хочете безповоротно ВИДАЛИТИ організм \"{organism.CommonName}\" з бази даних?",
                    "Увага! Видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var window = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (window == null) return;

                    var globalPeriod = window._allPeriods?.FirstOrDefault(p => p.Id.Equals(organism.PeriodId, StringComparison.OrdinalIgnoreCase));

                    if (globalPeriod != null)
                    {
                        globalPeriod.Organisms.Remove(organism);

                        window.SaveDataToJson();

                        _currentPeriod = globalPeriod;
                        this.DataContext = globalPeriod;

                        SpeciesItemsControl.ItemsSource = null;

                        window.SwitchPeriod(globalPeriod.Id);
                    }

                    SpeciesDetailState.Visibility = Visibility.Collapsed;
                    CrudFormGrid.Visibility = Visibility.Collapsed;
                    GalleryListState.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Обробник скасування редагування або створення. Повертає користувача в головне вікно галереї.
        /// </summary>
        private void CancelFormButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGallery();
        }

        /// <summary>
        /// Перемикає візуальні контейнери для показу головної сітки галереї та оновлює смуги прокрутки.
        /// </summary>
        private void ShowGallery()
        {
            if (_currentPeriod != null)
            {
                SpeciesItemsControl.ItemsSource = _currentPeriod.Organisms;
                UpdateScrollViewer();
            }

            CrudFormGrid.Visibility = Visibility.Collapsed;
            SpeciesDetailState.Visibility = Visibility.Collapsed;
            GalleryListState.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Повністю очищує текстові поля форми та скидає стан прапорців діапазону.
        /// </summary>
        private void ClearFormFields()
        {
            CommonNameTextBox.Clear();
            ScientificNameTextBox.Clear();
            TypeComboBox.SelectedIndex = -1;
            PeriodComboBox.SelectedIndex = -1;
            LifestyleTextBox.Clear();
            ImagePathTextBox.Clear();
            FromValueTextBox.Clear();
            FromUnitComboBox.SelectedIndex = 0;
            ToValueTextBox.Clear();
            ToUnitComboBox.SelectedIndex = 0;
            IsRangeCheckBox.IsChecked = false;
            IsRangeCheckBox_Changed(null!, null!);
        }

        /// <summary>
        /// Обробляє зміну стану прапорця діапазону років, динамічно згортаючи або розгортаючи колонки форми.
        /// </summary>
        private void IsRangeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (ToValueColumn == null || ToValueTextBox == null || ToUnitComboBox == null) return;

            if (IsRangeCheckBox.IsChecked == true)
            {
                ToValueColumn.Width = new GridLength(1, GridUnitType.Star);
                ToUnitColumn.Width = GridLength.Auto;

                RangeSeparatorText.Visibility = Visibility.Visible;
                ToValueTextBox.Visibility = Visibility.Visible;
                ToUnitComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                ToValueColumn.Width = new GridLength(0);
                ToUnitColumn.Width = new GridLength(0);

                RangeSeparatorText.Visibility = Visibility.Collapsed;
                ToValueTextBox.Visibility = Visibility.Collapsed;
                ToUnitComboBox.Visibility = Visibility.Visible;

                ToValueTextBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Обробляє зміну обраної одиниці виміру часу для другого поля. 
        /// Якщо обрано "н.ч" (наш час), числове поле автоматично заповнюється нулем та стає недоступним для редагування.
        /// </summary>
        private void ToUnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToValueTextBox == null || ToUnitComboBox == null) return;

            if (ToUnitComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content.ToString() == "н.ч")
            {
                ToValueTextBox.Text = "0";
                ToValueTextBox.IsEnabled = false;
            }
            else
            {
                if (ToValueTextBox.Text == "0" && !ToValueTextBox.IsEnabled)
                {
                    ToValueTextBox.Text = string.Empty;
                }
                ToValueTextBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Контролює динамічну видимість вертикальної смуги прокрутки залежно від загальної кількості елементів.
        /// </summary>
        private void UpdateScrollViewer()
        {
            int count = SpeciesItemsControl.Items.Count;
            GalleryScrollViewer.VerticalScrollBarVisibility =
                count > 9 ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        }

        /// <summary>
        /// Виконує перевірку символів введення у реальному часі, дозволяючи лише цифри та розділові знаки.
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(c => char.IsDigit(c) || c == '.' || c == ',');
        }

        private void CompareOrganismButton_Click(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            var currentOrganism = frameworkElement?.DataContext as Organism;

            if (currentOrganism != null)
            {
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToComparisonWithOrganism(currentOrganism);
                }
            }
        }
    }
}