namespace IOTLib;

public class GPIOConfig
{
    public Dictionary<int, EventHandler<GPIOEventArgs>> PinHandlers { get; set; } = new Dictionary<int, EventHandler<GPIOEventArgs>>();
}
