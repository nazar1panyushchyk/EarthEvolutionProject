using System.Collections.Generic;

namespace EarthEvolutionProject.Models
{
    /// <summary>
    /// Модель даних, що представляє окремий біологічний вид. Описує загальну та наукову назви, 
    /// тип харчування, посилання на зображення, опис способу життя, а також містить посилання 
    /// на батьківський період для зручної навігації.
    /// </summary>
    public class Organism
    {
        public required string Id { get; set; }
        public required string CommonName { get; set; }
        public required string ScientificName { get; set; }
        public required string Type { get; set; }
        public required string Image { get; set; }
        public string Existence { get; set; } = "";
        public string Lifestyle { get; set; } = "";
        public string PeriodName { get; set; } = "";
        public string PeriodId { get; set; } = "";
    }
}