using EarthEvolutionProject.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EarthEvolutionProject.Views
{
    /// <summary>
    /// Користувацький елемент керування, що забезпечує візуальне порівняння 
    /// часу існування різних біологічних видів на єдиній вертикальній геохронологічній шкалі.
    /// </summary>
    public partial class ComparisonView : UserControl
    {
        private List<Period> _periods = new();
        private double _maximumMillionsOfYears = 252.0;

        /// <summary>
        /// Прапорець для логіки чергування слотів. Визначає, куди буде додано 
        /// наступну істоту при автоматичному переході (true = лівий слот, false = правий слот).
        /// </summary>
        private bool _isFirstSlotNext = true;

        /// <summary>
        /// Конструктор компонента порівняння. Ініціалізує XAML-компоненти та 
        /// підписується на подію зміни розміру вікна для адаптивного перемальовування графіки.
        /// </summary>
        public ComparisonView()
        {
            InitializeComponent();
            SizeChanged += (s, e) => DrawCharts();
        }

        /// <summary>
        /// Завантажує дані геологічних періодів, витягує всі доступні організми 
        /// та наповнює ними випадаючі списки для вибору користувачем.
        /// </summary>
        /// <param name="periods">Список геологічних періодів із даними про організми.</param>
        public void LoadData(List<Period> periods)
        {
            _periods = periods ?? new();

            var allOrganisms = _periods
                .Where(p => p.Organisms != null)
                .SelectMany(p => p.Organisms)
                .OrderBy(o => o.CommonName)
                .ToList();

            ComboOrg1.ItemsSource = allOrganisms;
            ComboOrg2.ItemsSource = allOrganisms;
        }

        /// <summary>
        /// Автоматично обирає переданий організм для порівняння. 
        /// Реалізує логіку чергування: почергово заповнює лівий та правий слоти (випадаючі списки).
        /// </summary>
        /// <param name="org">Організм, який необхідно додати на шкалу порівняння.</param>
        public void SelectOrganismForComparison(Organism org)
        {
            if (org == null || ComboOrg1.ItemsSource == null) return;

            var allOrganisms = ComboOrg1.ItemsSource as IEnumerable<Organism>;
            if (allOrganisms != null)
            {
                var target = allOrganisms.FirstOrDefault(o => o.Id == org.Id);
                if (target != null)
                {
                    if (_isFirstSlotNext)
                    {
                        ComboOrg1.SelectedItem = target;
                    }
                    else
                    {
                        ComboOrg2.SelectedItem = target;
                    }

                    _isFirstSlotNext = !_isFirstSlotNext;
                }
            }
        }


        /// <summary>
        /// Обробляє подію зміни обраного елемента у будь-якому з випадаючих списків.
        /// Викликає повне перемальовування шкал.
        /// </summary>
        private void OnOrganismSelectionChanged(object sender, SelectionChangedEventArgs e) => DrawCharts();


        /// <summary>
        /// Головний метод генерації графіки. Очищає полотна (Canvas), обчислює динамічний масштаб 
        /// шкали часу та викликає методи для малювання осі і стовпчиків обраних істот.
        /// </summary>
        private void DrawCharts()
        {
            if (ActualHeight == 0 || AxisCanvas.ActualHeight == 0) return;

            AxisCanvas.Children.Clear();
            Org1Canvas.Children.Clear();
            Org2Canvas.Children.Clear();

            double h = AxisCanvas.ActualHeight;

            double detectedMax = _periods
                .Select(p => ParseTimeframeToMillionsOfYears(p.Timeframe).Start)
                .DefaultIfEmpty(252.0)
                .Max();

            double millionsOfYearsPerPixel = detectedMax / (h - 160.0);
            _maximumMillionsOfYears = detectedMax + (160.0 * millionsOfYearsPerPixel);

            DrawAxis();

            if (ComboOrg1.SelectedItem is Organism o1)
                DrawBar(o1, Org1Canvas, Brushes.DodgerBlue);

            if (ComboOrg2.SelectedItem is Organism o2)
                DrawBar(o2, Org2Canvas, Brushes.Tomato);
        }

        /// <summary>
        /// Відмальовує вертикальну геохронологічну вісь, горизонтальні пунктирні лінії 
        /// та текстові мітки років. Містить логіку уникнення накладання тексту (Smart Stacking).
        /// </summary>
        private void DrawAxis()
        {
            double h = AxisCanvas.ActualHeight;
            double w = AxisCanvas.ActualWidth;

            Line axisLine = new Line
            {
                X1 = w - 5,
                Y1 = 0,
                X2 = w - 5,
                Y2 = h,
                StrokeThickness = 3
            };
            axisLine.SetResourceReference(Line.StrokeProperty, "BorderBrush");
            AxisCanvas.Children.Add(axisLine);

            var rawMilestones = _periods
                .SelectMany(p => new[] { ParseTimeframeToMillionsOfYears(p.Timeframe).Start, ParseTimeframeToMillionsOfYears(p.Timeframe).End })
                .Append(0)
                .Distinct()
                .Where(t => t <= _maximumMillionsOfYears)
                .OrderBy(t => t)
                .ToList();

            var milestones = new List<double>();
            foreach (var t in rawMilestones)
            {
                if (!milestones.Any(m => Math.Abs(h - (m / _maximumMillionsOfYears * h) - (h - (t / _maximumMillionsOfYears * h))) < 6))
                {
                    milestones.Add(t);
                }
            }

            double textHeight = 20;
            double lowestAvailableY = h + textHeight;
            double margin = 4;

            foreach (double time in milestones)
            {
                double y = h - (time / _maximumMillionsOfYears * h);

                Line tick = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = w - 5,
                    Y2 = y,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 6, 6 }
                };
                tick.SetResourceReference(Line.StrokeProperty, "BorderBrush");
                AxisCanvas.Children.Add(tick);

                var label = new TextBlock
                {
                    Text = time == 0 ? "н.ч." : $"{time:0.##} млн",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                label.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");
                Canvas.SetLeft(label, 5);

                double idealTopY = y - 20;
                if (time == 0) idealTopY = y - 22;

                if (idealTopY < 0) idealTopY = 2;

                if (idealTopY + textHeight > lowestAvailableY)
                {
                    idealTopY = lowestAvailableY - textHeight;
                }

                Canvas.SetTop(label, idealTopY);
                AxisCanvas.Children.Add(label);

                lowestAvailableY = idealTopY - margin;
            }
        }

        /// <summary>
        /// Відмальовує візуальний стовпчик існування конкретного організму, 
        /// а також додає текстову назву, зображення (якщо наявне) та спливаючу підказку.
        /// </summary>
        /// <param name="org">Об'єкт моделі організму.</param>
        /// <param name="canvas">Полотно, на якому буде відмальовано графіку.</param>
        /// <param name="color">Колір заливки стовпчика (відрізняється для першого і другого слота).</param>
        private void DrawBar(Organism org, Canvas canvas, SolidColorBrush color)
        {
            var (start, end) = ParseTimeframeToMillionsOfYears(org.Existence);
            if (start == 0 && end == 0) return;

            double h = canvas.ActualHeight;

            double topY = h - (Math.Min(start, _maximumMillionsOfYears) / _maximumMillionsOfYears * h);
            double bottomY = h - (end / _maximumMillionsOfYears * h);

            double barH = Math.Max(bottomY - topY, 8);
            double barW = 80;
            double centerX = canvas.ActualWidth / 2 - barW / 2;

            var tooltip = new ToolTip();
            var tooltipContent = new StackPanel { Margin = new Thickness(5) };
            tooltipContent.Children.Add(new TextBlock { Text = org.CommonName, FontWeight = FontWeights.Bold, FontSize = 14 });
            tooltipContent.Children.Add(new TextBlock { Text = $"Існування: {org.Existence}", Margin = new Thickness(0, 5, 0, 0), FontSize = 12 });
            tooltip.Content = tooltipContent;

            var bar = new Rectangle
            {
                Width = barW,
                Height = barH,
                Fill = color,
                RadiusX = 8,
                RadiusY = 8,
                ToolTip = tooltip
            };
            Canvas.SetLeft(bar, centerX);
            Canvas.SetTop(bar, topY);
            canvas.Children.Add(bar);

            var nameLabel = new TextBlock
            {
                Text = org.CommonName,
                FontWeight = FontWeights.ExtraBold,
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                Width = canvas.ActualWidth
            };
            nameLabel.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryTextBrush");
            Canvas.SetLeft(nameLabel, 0);
            Canvas.SetTop(nameLabel, topY - 155);
            canvas.Children.Add(nameLabel);

            if (!string.IsNullOrWhiteSpace(org.Image))
            {
                try
                {
                    var img = new Image
                    {
                        Source = new BitmapImage(new Uri(org.Image, UriKind.RelativeOrAbsolute)),
                        Width = 120,
                        Height = 120,
                        Stretch = Stretch.Uniform,
                        Opacity = 1.0
                    };
                    Canvas.SetLeft(img, canvas.ActualWidth / 2 - 60);
                    Canvas.SetTop(img, topY - 130);
                    canvas.Children.Add(img);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Помилка графіки] Не вдалося завантажити зображення для істоти '{org.CommonName}'.");
                    System.Diagnostics.Debug.WriteLine($"Шлях у JSON: {org.Image}");
                    System.Diagnostics.Debug.WriteLine($"Деталі помилки: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Парсить текстовий рядок з часовим періодом (наприклад, "252 – 201 млн років тому") 
        /// і повертає числові значення початку та кінця в мільйонах років.
        /// </summary>
        /// <param name="text">Текстовий опис часових рамок із файлу бази даних.</param>
        /// <returns>Кортеж з двома значеннями типу double: (Початок, Кінець).</returns>
        private (double Start, double End) ParseTimeframeToMillionsOfYears(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return (0, 0);

            string clean = text.ToLower().Trim();
            bool endsNow = clean.Contains("н.ч");

            clean = clean.Replace("років тому", "").Replace("н.ч.", "").Replace("н.ч", "").Trim();
            var parts = clean.Split(new[] { '-', '–', '—' }, StringSplitOptions.RemoveEmptyEntries);

            double ParsePart(string part)
            {
                part = part.Trim();
                double multiplier;

                if (part.Contains("млн")) multiplier = 1.0;
                else if (part.Contains("тис")) multiplier = 0.001;
                else multiplier = 1.0;

                string digits = new string(part
                    .Where(c => char.IsDigit(c) || c == '.' || c == ',')
                    .ToArray());

                return double.TryParse(digits.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double val)
                    ? val * multiplier : 0;
            }

            if (parts.Length == 1 && endsNow)
            {
                double start = ParsePart(parts[0]);
                return (start, 0);
            }

            if (parts.Length == 1)
            {
                double val = ParsePart(parts[0]);
                return (val, val);
            }

            string firstPart = parts[0].Trim();
            string secondPart = parts[1].Trim();

            bool firstHasUnit = firstPart.Contains("млн") || firstPart.Contains("тис");

            if (!firstHasUnit)
            {
                double multiplier;
                if (secondPart.Contains("млн")) multiplier = 1.0;
                else if (secondPart.Contains("тис")) multiplier = 0.001;
                else multiplier = 1.0;

                string digits = new string(firstPart
                    .Where(c => char.IsDigit(c) || c == '.' || c == ',')
                    .ToArray());

                double startVal = double.TryParse(digits.Replace(',', '.'),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out double v) ? v * multiplier : 0;

                double endVal = endsNow ? 0 : ParsePart(secondPart);

                return startVal >= endVal ? (startVal, endVal) : (endVal, startVal);
            }

            double s = ParsePart(firstPart);
            double e = endsNow ? 0 : ParsePart(secondPart);

            return s >= e ? (s, e) : (e, s);
        }
    }
}