using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace EarthEvolutionProject.Models
{
    /// <summary>
    /// Клас, що представляє елемент фільтрації в інтерфейсі. Реалізує інтерфейс INotifyPropertyChanged 
    /// для динамічного оновлення стану вибору (IsSelected) у графічній оболонці застосунку.
    /// </summary>
    public class FilterItem : INotifyPropertyChanged
    {
        public string TypeName { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
