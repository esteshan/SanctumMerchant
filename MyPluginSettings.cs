using System;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Collections.Generic;
using Newtonsoft.Json;
using ExileCore2.Shared.Attributes;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace SanctumRewards;

public class SanctumRewardsSettings : ISettings
{
    [Menu("Enable Plugin")]
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Debug Mode")]
    public ToggleNode Debug { get; set; } = new ToggleNode(false);
    
}
