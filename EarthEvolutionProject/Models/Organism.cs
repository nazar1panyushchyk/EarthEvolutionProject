using System.Collections.Generic;

public class Organism
{
    public required string Id { get; set; }
    public required string CommonName { get; set; }
    public required string ScientificName { get; set; }
    public required string Type { get; set; }
    public required string Image { get; set; }
    public string Existence { get; set; } = "";
    public string Lifestyle { get; set; } = "";
}