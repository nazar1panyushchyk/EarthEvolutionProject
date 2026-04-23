using System.Collections.Generic;

namespace EarthEvolutionProject.Models
{
    /// <summary>
    /// Модель даних, що описує геологічний період. Містить ідентифікатор, назву ери, часові межі, 
    /// текстовий опис, посилання на головне зображення та колекцію представників фауни цього періоду.
    /// </summary>
    public class Period
    {
        public required string Id { get; set; }
        public required string Era { get; set; }
        public required string Name { get; set; }
        public required string Timeframe { get; set; }
        public required string MainImage { get; set; }
        public required string Description { get; set; }
        public List<Organism> Organisms { get; set; } = [];
    }
}