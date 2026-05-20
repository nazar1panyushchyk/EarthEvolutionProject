using System;
using System.Collections.Generic;
using System.Text;

namespace EarthEvolutionProject.Models
{
    public class EvolutionDataWrapper
    {
        public List<string> InterestingFacts { get; set; } = [];
        public List<Period> Periods { get; set; } = [];
    }
}
