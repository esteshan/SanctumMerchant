using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;

namespace SanctumMerchant;

public class SanctumRewardsSettings : ISettings
{
    [Menu("Enable Plugin")]
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Debug Mode")]
    public ToggleNode Debug { get; set; } = new ToggleNode(false);
    
}
