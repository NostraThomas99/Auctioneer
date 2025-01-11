using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Dalamud.Game;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace Auctioneer.Helpers;

internal static class RetainerHelper
{
    private static Dictionary<uint, string> Items { get; set; }

    public static unsafe bool PostListing(int gridNumber, int slot)
    {
        AtkUnitBase* AddonPtr;
        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSellList", out AddonPtr) &&
            GenericHelpers.IsAddonReady(AddonPtr))
        {
            Callback.Fire(AddonPtr, true, 2, gridNumber, slot);
            if (!EzThrottler.Throttle(nameof(PostListing), 1000))
                return true;
        }

        return false;
    }

    internal static unsafe bool ClickOnArmoryChest()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("InventoryExpansion", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Callback.Fire(AddonPtr, true, 11, 119);
        return true;
    }

    internal static unsafe bool ClickOnArmoryCategory(int category)
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ArmouryBoard", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Callback.Fire(AddonPtr, true, 0, category);
        return true;
    }

    internal static bool ClickOnRetainerByName(string retainerName)
    {
        var index = GameRetainerManager.Retainers.IndexOf(r => r.Name.ToUpper() == retainerName.ToUpper());
        return ClickOnSpecificRetainer(index);
    }

    internal static unsafe bool ClickOnSpecificRetainer(int retainerIndex)
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerList", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Callback.Fire(AddonPtr, true, 2, retainerIndex, 0, 0);
        GameRetainerManager.Retainer retainer = GameRetainerManager.Retainers[retainerIndex];
        Auctioneer.Status = "Clicking on " + retainer.Name.ToUpper();
        return true;
    }

    internal static unsafe bool OpenRetainerSellList()
    {
        RetainerHelper.CloseContextMenu();
        AtkUnitBase* AddonPtr1;
        if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerList", out AddonPtr1) &&
            GenericHelpers.IsAddonReady(AddonPtr1))
        {
            Svc.Log.Error("No Retainer Selected");
            return false;
        }

        AtkUnitBase* AddonPtr2;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectString", out AddonPtr2) ||
            !GenericHelpers.IsAddonReady(AddonPtr2))
            return false;
        Auctioneer.Status = "Opening retainer sell list";
        Callback.Fire(AddonPtr2, true, 2);
        return true;
    }

    internal static unsafe bool CloseRetainerList()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerList", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Closing retainer list";
        AddonPtr->Close(true);
        return true;
    }

    internal static unsafe bool GetSellListItems(ref List<uint> availableItems)
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSellList", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        for (int index = 0; index < 20; ++index)
        {
            InventoryItem* inventorySlot =
                InventoryManager.Instance()->GetInventoryContainer(InventoryType.RetainerMarket)->GetInventorySlot(
                    index);
            if (inventorySlot->ItemId != 0U)
            {
                availableItems.Add(inventorySlot->ItemId);
            }
        }
        return true;
    }

    internal static unsafe bool CloseItemSearchResult()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ItemSearchResult", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Closing item search result";
        AddonPtr->Close(true);
        return true;
    }

    internal static unsafe bool CloseRetainerSell()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSell", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Closing adjust price";
        AddonPtr->Close(true);
        return true;
    }

    internal static unsafe bool CloseRetainerSellList()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSellList", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Closing retainer sell list";
        AddonPtr->Close(true);
        return true;
    }

    internal static unsafe bool CloseSelectString()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectString", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Closing select string window";
        AddonPtr->Close(true);
        return true;
    }

    internal static unsafe bool CloseContextMenu()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContextMenu", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Closing context menu";
        AddonPtr->Close(true);
        return true;
    }

    internal static unsafe bool ClickTalk()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("Talk", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Skipping dialog";
        Callback.Fire(AddonPtr, true);
        return true;
    }

    internal static unsafe bool ClickSellingItem(int index)
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSellList", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Clicking on item " + (index + 1);
        Callback.Fire(AddonPtr, true, 0, index, 1);
        return true;
    }

    internal static unsafe bool ClickAdjustPrice()
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("ContextMenu", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Auctioneer.Status = "Clicking adjust price";
        Callback.Fire(AddonPtr, true, 0, 0, 0, 0, 0);
        return true;
    }

    internal static unsafe bool ClickComparePrice(
        ref int CurrentItemPrice,
        ref bool IsCurrentItemHq)
    {
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("RetainerSell", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr) || !EzThrottler.Check("Throttle"))
            return false;
        EzThrottler.Throttle("Throttle", 1500);
        Auctioneer.Status = "Clicking compare price";
        CurrentItemPrice = AddonPtr->AtkValues[5].Int;
        IsCurrentItemHq = Marshal.PtrToStringUTF8((IntPtr)AddonPtr->AtkValues[1].String).Contains('\uE03C');
        Callback.Fire(AddonPtr, true, 4);
        return true;
    }

    private static unsafe bool IsOwnRetainer(ulong retainerId)
    {
        RetainerManager* retainerManagerPtr = RetainerManager.Instance();
        for (uint index = 0; index < retainerManagerPtr->GetRetainerCount(); ++index)
        {
            if (retainerId == retainerManagerPtr->GetRetainerBySortedIndex(index)->RetainerId)
                return true;
        }

        return false;
    }

    internal static unsafe bool ClickEntrustOrWithDrawlGil()
    {
        Auctioneer.Status = "Clicking Entrust or Withdraw Gil";
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectString", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        Callback.Fire(AddonPtr, true, 1);
        return true;
    }

    internal static unsafe bool WithDrawGil(uint gilAmount)
    {
        Auctioneer.Status = "Withdrawing Gil";
        AtkUnitBase* AddonPtr;
        if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>("Bank", out AddonPtr) ||
            !GenericHelpers.IsAddonReady(AddonPtr))
            return false;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
        interpolatedStringHandler.AppendLiteral("WithDrawing ");
        interpolatedStringHandler.AppendFormatted<uint>(gilAmount);
        Svc.Log.Debug(interpolatedStringHandler.ToStringAndClear());
        Callback.Fire(AddonPtr, true, 3, gilAmount);
        Callback.Fire(AddonPtr, true, 0, 0);
        return true;
    }

    public class Retainer
    {
        public int Id { get; set; }

        public int RetainerID { get; set; }

        public string Name { get; set; } = "";

        public int Level { get; set; }

        public ClassJob ClassJob { get; set; }

        public Town Town { get; set; }

        public int ItemCount { get; set; }
    }
}