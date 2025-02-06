using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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

                // Call the helper method to locate buttons
                SanctumUiHelper.LocateButtons(sanctumRewardWindow, 
                    out _downArrow, 
                    out _upArrow, 
                    out _purchaseButton, 
                    out _closeButton);

                // Only buy while F5 is held
                if (Keyboard.IsKeyDown(Keys.F5))
                {
                    BuyBoon(sanctumRewardWindow);
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

            // ✅ Fix: Remove commas before parsing
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

        private void BuyBoon(Element sanctumRewardWindow)
        {
            while (Keyboard.IsKeyDown(Keys.F5)) // Stop if F5 is released
            {
                // Refresh RewardElements dynamically after every buy
                UpdateRewardElements(sanctumRewardWindow);

                bool boughtSomething = false;

                foreach (var priority in _jsonLoader.SanctumEffects)
                {
                    foreach (var effect in priority.Effects)
                    {
                        var matchingBoon = _rewardDetails.FirstOrDefault(x =>
                            x.Name.Equals($"Boon: {effect.EffectName}", StringComparison.OrdinalIgnoreCase) &&
                            x.Visibility == "VISIBLE" && 
                            x.CanBuy == "CAN BUY");

                        if (matchingBoon.Name != null)
                        {
                            Mouse.SetCursorPosAndLeftClick(matchingBoon.Position, 1500, Vector2.Zero);
                            Mouse.SetCursorPosAndLeftClick(_purchaseButton.GetClientRectCache.Center, 1500, Vector2.Zero);
                            boughtSomething = true;
                            break; // Stop checking once an item is bought
                        }
                    }

                    if (boughtSomething)
                        break; // Move out of priority loop if an item was bought
                }

                // If nothing from the JSON was bought, try scrolling for more options
                bool anyBuyableBoon = _rewardDetails.Any(x =>
                    _jsonLoader.SanctumEffects.SelectMany(p => p.Effects)
                    .Any(e => x.Name.Equals($"Boon: {e.EffectName}", StringComparison.OrdinalIgnoreCase)) &&
                    x.CanBuy == "CAN BUY");

                if (anyBuyableBoon && _downArrow != null)
                {
                    Mouse.SetCursorPosAndLeftClick(_downArrow.GetClientRectCache.Center, 50, Vector2.Zero);
                    continue; // Continue scrolling as long as F5 is held
                }

                break; // Stop buying if no more buyable boons
            }
        }

        public override void Render()
        {
            if (!Settings.Debug.Value) return;

            int yOffset = 180;

            // Draw Currency Amount
            Graphics.DrawText($"Currency: {_currencyAmount}", new Vector2(100, yOffset), Color.Cyan);
            yOffset += 20;

            // Draw Scroll Offset
            Graphics.DrawText($"Scroll Offset: {_scrollOffset}", new Vector2(100, yOffset), Color.Yellow);
            yOffset += 20;

            // Draw Reward List
            foreach (var (name, cost, position, visibility, canBuy, warning) in _rewardDetails)
            {
                Color textColor = visibility == "VISIBLE" ? Color.Lime : Color.Red;
                Graphics.DrawText($"Reward: {name} - Cost: {cost} - {visibility} - {canBuy}", new Vector2(100, yOffset), textColor);
                yOffset += 20;
            }

            // Draw JSON Priority Data **Below Other Debug Info**
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
        }
    }
}
