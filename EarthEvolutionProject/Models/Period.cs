using System.Collections.Generic;

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