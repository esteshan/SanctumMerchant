using ExileCore2.Shared.Nodes;
using System.Windows.Forms;
using ExileCore2.Shared.Interfaces;

public class SanctumRewardsSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
    public HotkeyNode PurchaseKey { get; set; } = new HotkeyNode(Keys.F5); 
    public ToggleNode Debug { get; set; } = new ToggleNode(false); 
}