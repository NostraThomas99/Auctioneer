using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Auctioneer.Helpers;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Auctioneer.Tasks;

public class AdjustItemPriceTask
{
    public AdjustItemPriceTask(uint itemId)
    {
        ItemId = itemId;
    }

    public uint ItemId;
    public int CurrentPrice;
    public int LowestPrice;

    private static Regex PriceAdjustRegex() => new Regex("[^0-9]");

    public unsafe bool Enter()
    {
        Auctioneer.Status = "Getting current market price";
        AtkUnitBase* AddonPtr1;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out AddonPtr1) ||
            !GenericHelpers.IsAddonReady(AddonPtr1) || !EzThrottler.Throttle("Throttle", 1000))
            return false;
        string text1 = AddonPtr1->GetTextNodeById(29U)->NodeText.ExtractText();
        if (AddonPtr1->GetTextNodeById(5U)->NodeText.ExtractText() == "Please wait and try your search again.")
            AddonPtr1->Close(true);
        Auctioneer.Status = "Waiting for listings";
        if (string.IsNullOrEmpty(text1))
            return false;
        if (int.Parse(PriceAdjustRegex().Replace(text1, "")) == 0)
        {
            Auctioneer.Status = "No listings";
            if (false) //opened from checked listing
            {
                LowestPrice = 0;
                AddonPtr1->Close(true);
                return true;
            }

            Callback.Fire(AddonPtr1, true, 0);
            AtkUnitBase* AddonPtr2;
            if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ItemHistory", out AddonPtr2) ||
                !GenericHelpers.IsAddonReady(AddonPtr2))
                return false;
            string text2 =
                AddonPtr2->UldManager.NodeList[3]->GetAsAtkComponentNode()->Component->UldManager.NodeList[2]->
                        GetAsAtkComponentNode()->Component->UldManager.NodeList[6]->GetAsAtkTextNode()->NodeText
                    .ExtractText();
            if (!int.TryParse(PriceAdjustRegex().Replace(text2, ""), out LowestPrice))
                return false;
            EzThrottler.Throttle("Throttle", 1000);
            Svc.Log.Fatal(text2);
            return true;
        }

        int num = int.Parse(PriceAdjustRegex().Replace(text1, ""));
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
        interpolatedStringHandler.AppendLiteral("Number of Items: ");
        interpolatedStringHandler.AppendFormatted<int>(num);
        string stringAndClear1 = interpolatedStringHandler.ToStringAndClear();
        Auctioneer.Status = stringAndClear1;
        string text3 =
            AddonPtr1->UldManager.NodeList[5]->GetAsAtkComponentNode()->Component->UldManager.NodeList[1]->
                    GetAsAtkComponentNode()->Component->UldManager.NodeList[10]->GetAsAtkTextNode()->NodeText
                .ExtractText();
        if (!int.TryParse(PriceAdjustRegex().Replace(text3, ""), out LowestPrice))
            return false;
        Svc.Log.Info("Current Item is not HQ");
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
        interpolatedStringHandler.AppendLiteral(" Price From ItemSearchResult Addon: ");
        interpolatedStringHandler.AppendFormatted<int>(LowestPrice);
        Svc.Log.Debug(interpolatedStringHandler.ToStringAndClear());
        Svc.Log.Warning(
            "Failed to get price from network getting price from ItemSearchResult Addon, Ignore Own Retainer will not function at this time");
        Svc.Log.Info("Current market price: " + LowestPrice);

        AddonPtr1->Close(true);
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
        interpolatedStringHandler.AppendLiteral("LOWEST PRICE IS ");
        interpolatedStringHandler.AppendFormatted<int>(LowestPrice);
        string stringAndClear2 = interpolatedStringHandler.ToStringAndClear();
        Auctioneer.Status = stringAndClear2;
        return true;
    }

    public unsafe bool Execute()
    {
        Auctioneer.Status = "Adjusting price";
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSell", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        var adjustedPrice = LowestPrice - 1;
        Callback.Fire(AddonPtr, true, 2, adjustedPrice);
        Callback.Fire(AddonPtr, true, 0);
        return true;
    }
}