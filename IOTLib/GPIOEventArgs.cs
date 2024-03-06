using System.Device.Gpio;
using System.Text.Json;

namespace IOTLib;

public class GPIOEventArgs : EventArgs
{
    public int Pin
    {
        get; set;
    }
    public bool State
    {
        get; set;
    }
    public GPIOEventArgs(int pin, bool state)
    {
        Pin = pin;
        State = state;
    }
    public override string ToString() => JsonSerializer.Serialize(this);
}
