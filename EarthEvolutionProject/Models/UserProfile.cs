using System;
using System.Collections.Generic;
using System.Text;

namespace EarthEvolutionProject.Models
{
    public class UserProfile
    {
        public string Username { get; set; } = string.Empty;
        public bool IsDarkTheme { get; set; } = true;
        public string LastSelectedPeriodId { get; set; } = "triassic";
        public string? LastSelectedOrganismId { get; set; } = null;
        public bool ShowInterestingFacts { get; set; } = true;
        public List<string> SearchHistory { get; set; } = new List<string>();
        public bool ShowWelcomeFacts { get; set; } = true;
    }

    public class AppConfiguration
    {
        public string? LastActiveUsername { get; set; }
        public List<UserProfile> Profiles { get; set; } = new List<UserProfile>();
    }
}
