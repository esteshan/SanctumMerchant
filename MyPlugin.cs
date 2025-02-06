using System.Collections.Generic;
using System.Drawing;
using ExileCore2;
using ExileCore2.PoEMemory.MemoryObjects;
using UnIdy.Utils; // Using UnIdy.Utils for Mouse & Keyboard control
using Vector2 = System.Numerics.Vector2;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ExileCore2.PoEMemory;

namespace SanctumRewards;

public class MyPlugin : BaseSettingsPlugin<SanctumRewardsSettings>
{
    private Vector2 _scrollOffset = Vector2.Zero;

    private List<(string Name, string Cost, Vector2 Position, string Visibility, string CanBuy, string Warning)>
        _rewardDetails = new();

    private int _currencyAmount = 0;
    private Element _downArrow;
    private Element _upArrow;
    private Element _purchaseButton;
    private Element _closeButton;

    public override bool Initialise()
    {
        return true;
    }

    public override void Tick()
    {
        var sanctumRewardWindow = GameController.IngameState.IngameUi?.SanctumRewardWindow;

        if (sanctumRewardWindow != null && sanctumRewardWindow.IsVisible)
        {
            UpdateRewardElements(sanctumRewardWindow);

            // Locate buttons
            _downArrow = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                .Children.ElementAtOrDefault(1)?
                .Children.ElementAtOrDefault(0)?
                .Children.ElementAtOrDefault(2)?
                .Children.ElementAtOrDefault(1);

            _upArrow = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                .Children.ElementAtOrDefault(1)?
                .Children.ElementAtOrDefault(0)?
                .Children.ElementAtOrDefault(2)?
                .Children.ElementAtOrDefault(0);

            _purchaseButton = sanctumRewardWindow.Children.ElementAtOrDefault(0)?
                .Children.ElementAtOrDefault(2);

            _closeButton = sanctumRewardWindow.Children.ElementAtOrDefault(3);

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

        for (int i = 0; i < 15; i++) // Assuming max 15 elements
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

            // If it's a relic, mark it as "DO NOT BUY"
            string warning = rewardName.Contains("Relic") ? "DO NOT BUY" : "";

            _rewardDetails.Add((rewardName, rewardCostText, itemPosition, visibility, canBuy, warning));
        }
    }

    private void BuyBoon(Element sanctumRewardWindow)
    {
        while (Keyboard.IsKeyDown(Keys.F5)) // Stop if F5 is released
        {
            // Refresh RewardElements dynamically after every buy
            UpdateRewardElements(sanctumRewardWindow);

            var buyableBoon = _rewardDetails.FirstOrDefault(x =>
                x.Name.Contains("Boon") && x.Visibility == "VISIBLE" && x.CanBuy == "CAN BUY");

            if (buyableBoon.Name != null)
            {
                Mouse.SetCursorPosAndLeftClick(buyableBoon.Position, 50, Vector2.Zero);
                System.Threading.Thread.Sleep(100);
                Mouse.SetCursorPosAndLeftClick(_purchaseButton.GetClientRectCache.Center, 50, Vector2.Zero);
                System.Threading.Thread.Sleep(1000);
                continue; // Continue only if F5 is still held
            }

            bool anyBuyableBoon = _rewardDetails.Any(x => x.Name.Contains("Boon") && x.CanBuy == "CAN BUY");

            if (anyBuyableBoon && _downArrow != null)
            {
                Mouse.SetCursorPosAndLeftClick(_downArrow.GetClientRectCache.Center, 50, Vector2.Zero);
                System.Threading.Thread.Sleep(300);
                continue; // Continue scrolling as long as F5 is held
            }

            break; // Stop buying if no more buyable boons
        }
    }

    public override void Render()
    {
        if (!Settings.Debug.Value) return;

        int yOffset = 180;

        // Draw Currency Amount on screen
        Graphics.DrawText($"Currency: {_currencyAmount}", new Vector2(100, yOffset), Color.Cyan);
        yOffset += 20;

        // Draw Scroll Offset on screen
        Graphics.DrawText($"Scroll Offset: {_scrollOffset}", new Vector2(100, yOffset), Color.Yellow);
        yOffset += 20;

        // Draw each reward item with visibility and cost
        foreach (var (name, cost, position, visibility, canBuy, warning) in _rewardDetails)
        {
            Color textColor = visibility == "VISIBLE" ? Color.Lime : Color.Red;
            Color buyColor = canBuy == "CAN BUY" ? Color.Green : Color.Red;
            Color warningColor = Color.Red;

            Graphics.DrawText($"Reward: {name} - Cost: {cost} - Pos: {position} - {visibility} - {canBuy}",
                new Vector2(100, yOffset), textColor);

            if (!string.IsNullOrEmpty(warning))
            {
                Graphics.DrawText($"  {warning}", new Vector2(500, yOffset), warningColor); // Draw "DO NOT BUY" in red
            }

            yOffset += 20;
        }
    }
}