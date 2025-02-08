using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore2;
using ExileCore2.PoEMemory;
using SanctumMerchant.Helper;
using MyPlugin.Utils;
using Vector2 = System.Numerics.Vector2;

namespace SanctumMerchant
{
    public class SanctumMerchant : BaseSettingsPlugin<SanctumRewardsSettings>
    {
        private Vector2 _scrollOffset = Vector2.Zero;
        private List<(string Name, string Cost, Vector2 Position, string Visibility, string CanBuy, string Warning)> _rewardDetails = new();
        private int _currencyAmount = 0;
        private Element _downArrow;
        private Element _upArrow;
        private Element _purchaseButton;
        private Element _closeButton;
        private readonly JsonLoader _jsonLoader;
        
        public SanctumMerchant()
        {
            _jsonLoader = new JsonLoader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Source", "SanctumMerchant", "Json", "HelloWorld.json"));
        }

        public override bool Initialise()
        {   
            _jsonLoader.LoadJson();
            return true;
        }

        public override void Tick()
        {
            var sanctumRewardWindow = GameController.IngameState.IngameUi?.SanctumRewardWindow;

            if (sanctumRewardWindow != null && sanctumRewardWindow.IsVisible)
            {
                UpdateRewardElements(sanctumRewardWindow);

                SanctumUiHelper.LocateButtons(sanctumRewardWindow, 
                    out _downArrow, 
                    out _upArrow, 
                    out _purchaseButton, 
                    out _closeButton);

                
                if (Keyboard.IsKeyDown(Settings.PurchaseKey.Value))
                {
                    _ = BuyBoonAsync();
                }
               
            }
        }

        private void UpdateRewardElements(Element sanctumRewardWindow)
        {
            _rewardDetails.Clear();
            
            var scrollElement = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                                                     .Children.ElementAtOrDefault(1)?
                                                     .Children.ElementAtOrDefault(0);

            if (scrollElement != null)
            {
                _scrollOffset = scrollElement.ScrollOffset;
            }

            var currencyElement = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                                                     .Children.ElementAtOrDefault(0)?
                                                     .Children.ElementAtOrDefault(0)?
                                                     .Children.ElementAtOrDefault(1);

            string currencyText = currencyElement?.Text?.Replace(",", "").Trim();
            _currencyAmount = int.TryParse(currencyText, out int parsedCurrency) ? parsedCurrency : 0;

            for (int i = 0; i < 25; i++) 
            {
                var rewardTextElement = scrollElement?.Children.ElementAtOrDefault(1)?
                                                       .Children.ElementAtOrDefault(i)?
                                                       .Children.ElementAtOrDefault(1)?
                                                       .Children.ElementAtOrDefault(0)?
                                                       .Children.ElementAtOrDefault(0);

                string rewardName = rewardTextElement?.Text ?? "No Text";

                var rewardCostElement = scrollElement?.Children.ElementAtOrDefault(1)?
                                                       .Children.ElementAtOrDefault(i)?
                                                       .Children.ElementAtOrDefault(2)?
                                                       .Children.ElementAtOrDefault(0)?
                                                       .Children.ElementAtOrDefault(1);

                string rewardCostText = rewardCostElement?.Text?.Replace(",", "").Trim() ?? "Unknown";
                int rewardCost = int.TryParse(rewardCostText, out int parsedCost) ? parsedCost : int.MaxValue;

                var rewardItemWindow = scrollElement?.Children.ElementAtOrDefault(1)?
                                                         .Children.ElementAtOrDefault(i)?
                                                         .Children.ElementAtOrDefault(1);

                Vector2 itemPosition = Vector2.Zero;
                string visibility = "NOT VISIBLE";
                if (rewardItemWindow != null)
                {
                    var itemRect = rewardItemWindow.GetClientRectCache;
                    itemPosition = new Vector2(itemRect.Center.X, itemRect.Center.Y);

                    if (itemPosition.Y >= 423 && itemPosition.Y <= 1419)
                    {
                        visibility = "VISIBLE";
                    }
                }

                string canBuy = (rewardCost <= _currencyAmount) ? "CAN BUY" : "CANNOT BUY";

                _rewardDetails.Add((rewardName, rewardCostText, itemPosition, visibility, canBuy, ""));
            }
        }
        
        private string _stopReason = "";

        private async Task BuyBoonAsync()
{
    _stopReason = "";
    Vector2 lastBoughtPosition = Vector2.Zero;
    bool boughtSilverTongue = false;

    while (Keyboard.IsKeyDown(Settings.PurchaseKey.Value))
    {
        // 🛑 Step 1: Search for "Boon: Silver Tongue" and buy it
        for (int attempt = 0; attempt < 3; attempt++) // Allow up to 10 scrolls
        {
            UpdateRewardElements(GameController.IngameState.IngameUi.SanctumRewardWindow);

            var silverTongue = _rewardDetails.FirstOrDefault(x =>
                x.Name.Equals("Boon: Silver Tongue", StringComparison.OrdinalIgnoreCase) &&
                x.Visibility == "VISIBLE" &&
                x.CanBuy == "CAN BUY");

            if (silverTongue.Name != null)
            {
                // Buy "Silver Tongue"
                await Mouse.SetCursorPosAndLeftClickAsync(silverTongue.Position, 750, Vector2.Zero);
                await Mouse.SetCursorPosAndLeftClickAsync(_purchaseButton.GetClientRectCache.Center, 750, Vector2.Zero);
                await Task.Delay(200); // Allow UI to update

                boughtSilverTongue = true;
                break; // Exit the loop once purchased
            }

            // If not found, scroll down to check more items
            if (_downArrow != null)
            {
                await Mouse.SetCursorPosAndLeftClickAsync(_downArrow.GetClientRectCache.Center, 50, Vector2.Zero);
                await Task.Delay(200); // Allow UI to refresh
            }
            else
            {
                break; // No more scrolls available
            }
        }

        // If we bought Silver Tongue, stop and exit function
        if (boughtSilverTongue) return;

        // 🛑 Step 2: Continue normal JSON-based Boon selection if Silver Tongue wasn't found
        bool boughtSomething = false;

        while (true)
        {
            UpdateRewardElements(GameController.IngameState.IngameUi.SanctumRewardWindow);

            var matchingBoon = _rewardDetails.FirstOrDefault(x =>
                _jsonLoader.SanctumEffects.SelectMany(p => p.Effects)
                .Any(e => x.Name.Equals($"Boon: {e.EffectName}", StringComparison.OrdinalIgnoreCase)) &&
                x.Visibility == "VISIBLE" &&
                x.CanBuy == "CAN BUY");

            if (matchingBoon.Name != null)
            {
                await Mouse.SetCursorPosAndLeftClickAsync(matchingBoon.Position, 750, Vector2.Zero);
                await Mouse.SetCursorPosAndLeftClickAsync(_purchaseButton.GetClientRectCache.Center, 750, Vector2.Zero);
                boughtSomething = true;
                await Task.Delay(125);
                lastBoughtPosition = matchingBoon.Position;
                continue;
            }
            break;
        }

        if (!boughtSomething) break;

        UpdateRewardElements(GameController.IngameState.IngameUi.SanctumRewardWindow);

        var nextBoon = _rewardDetails.FirstOrDefault(x =>
            _jsonLoader.SanctumEffects.SelectMany(p => p.Effects)
            .Any(e => x.Name.Equals($"Boon: {e.EffectName}", StringComparison.OrdinalIgnoreCase)) &&
            x.CanBuy == "CAN BUY");

        if (nextBoon.Name != null)
        {
            if (_downArrow != null && nextBoon.Position.Y > lastBoughtPosition.Y)
            {
                await Mouse.SetCursorPosAndLeftClickAsync(_downArrow.GetClientRectCache.Center, 50, Vector2.Zero);
            }
            else if (_upArrow != null && nextBoon.Position.Y < lastBoughtPosition.Y)
            {
                await Mouse.SetCursorPosAndLeftClickAsync(_upArrow.GetClientRectCache.Center, 50, Vector2.Zero);
            }
        }
    }

    _stopReason = !_rewardDetails.Any(x => x.CanBuy == "CAN BUY") ? "No Boons Available to Buy!" : "Finished Buying!";
}


        public override void Render()
        {
            if (!Settings.Debug.Value) return;
            int yOffset = 180;
            Graphics.DrawText($"Currency: {_currencyAmount}", new Vector2(100, yOffset), Color.Cyan);
            yOffset += 20;
            Graphics.DrawText($"Scroll Offset: {_scrollOffset}", new Vector2(100, yOffset), Color.Yellow);
            yOffset += 20;

            foreach (var (name, cost, position, visibility, canBuy, warning) in _rewardDetails)
            {
                Color textColor = visibility == "VISIBLE" ? Color.Lime : Color.Red;
                Graphics.DrawText($"Reward: {name} - Cost: {cost} - {visibility} - {canBuy}", new Vector2(100, yOffset), textColor);
                yOffset += 20;
            }
            
            yOffset += 30;
            Graphics.DrawText("Sanctum Priority Effects:", new Vector2(100, yOffset), Color.Orange);
            yOffset += 20;

            foreach (var priority in _jsonLoader.SanctumEffects)
            {
                Graphics.DrawText($"{priority.MenuName}:", new Vector2(100, yOffset), Color.Yellow);
                yOffset += 20;

                foreach (var effect in priority.Effects)
                {
                    Graphics.DrawText($"  - {effect.EffectName}", new Vector2(120, yOffset), Color.White);
                    yOffset += 20;
                }

                yOffset += 10;
            }

            yOffset += 30;
            Graphics.DrawText($"Status: {_stopReason}", new Vector2(100, yOffset), Color.Red);
        }
    }
}
