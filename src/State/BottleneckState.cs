namespace KingdomBottleneckHeatmap
{
    public enum BottleneckState
    {
        Idle,           // Neutral / unconfigured / dormant
        Running,        // Active production, crafting, or item transit
        Starved,        // Waiting for required input ingredients, resources, or funds
        Clogged,        // Output buffer full, pipe rejected, or blocked downstream
        FullStorage     // Storage containers (Stockpiles) that have reached full capacity
    
    }
}
