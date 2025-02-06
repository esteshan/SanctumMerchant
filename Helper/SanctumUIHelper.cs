using System.Linq;
using ExileCore2.PoEMemory;

namespace MyPlugin.Helper
{
    public static class SanctumUiHelper
    {
        public static void LocateButtons(Element sanctumRewardWindow, 
                                         out Element downArrow, 
                                         out Element upArrow, 
                                         out Element purchaseButton, 
                                         out Element closeButton)
        {
            downArrow = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                                                 .Children.ElementAtOrDefault(1)?
                                                 .Children.ElementAtOrDefault(0)?
                                                 .Children.ElementAtOrDefault(2)?
                                                 .Children.ElementAtOrDefault(1);

            upArrow = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                                               .Children.ElementAtOrDefault(1)?
                                               .Children.ElementAtOrDefault(0)?
                                               .Children.ElementAtOrDefault(2)?
                                               .Children.ElementAtOrDefault(0);

            purchaseButton = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                                                       .Children.ElementAtOrDefault(2);

            closeButton = sanctumRewardWindow.Children.ElementAtOrDefault(3);
        }
    }
}
