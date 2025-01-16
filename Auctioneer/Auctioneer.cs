using Auctioneer.Helpers;
using Auctioneer.IPC;
using Auctioneer.Tasks;
using AutoRetainerAPI;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using ECommons.Commands;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using ImGuiNET;

namespace Auctioneer;

// ReSharper disable once ClassNeverInstantiated.Global
public class Auctioneer : IDalamudPlugin
{
    public static Configuration Config;
    public TaskManager TaskManager;
    public AutoRetainerApi AutoRetainerApi;
    public WindowSystem WindowSystem;
    public ConfigWindow ConfigWindow;
    public Auctioneer(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
        AutoRetainerApi = new AutoRetainerApi();
        Config = Configuration.Load();

        TaskManager = new TaskManager();

        ConfigWindow = new ConfigWindow();
        WindowSystem = new WindowSystem();
        WindowSystem.AddWindow(ConfigWindow);

        Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ConfigOpen;
        AutoRetainerApi.OnRetainerPostprocessStep += AuctioneerOnRetainerPostProcessStep;
        AutoRetainerApi.OnRetainerReadyToPostprocess += AuctioneerOnRetainerReadyToPostProcess;
        AutoRetainerApi.OnRetainerListTaskButtonsDraw += AuctioneerOnRetainerListTaskButtonsDraw;
    }

    private void ConfigOpen()
    {
        OnCommand("auctioneer", "");
    }

    private void AuctioneerOnRetainerListTaskButtonsDraw()
    {
        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Clipboard))
        {
            AutoRetainerApi.ProcessIPCTaskFromOverlay();
        }
        ImGuiEx.Tooltip("Adjust retainer listing prices");
    }

    [Cmd("/auctioneer", "Open config menu", true, true)]
    public void OnCommand(string command, string args)
    {
        ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
    }

    private void AuctioneerOnRetainerReadyToPostProcess(string retainerName)
    {
        TaskManager.Enqueue(() => LockMarketbuddy());
        TaskManager.DelayNext(1000);
        //TaskManager.Enqueue(() => RetainerHelper.ClickOnRetainerByName(retainerName));
        TaskManager.Enqueue(() => RetainerHelper.OpenRetainerSellList());
        TaskManager.Enqueue(() => AdjustItems());
        TaskManager.Enqueue(() => RetainerHelper.CloseRetainerSellList());
        //TaskManager.Enqueue(() => RetainerHelper.CloseSelectString());
        TaskManager.Enqueue(() => UnlockMarketbuddy());
        TaskManager.Enqueue(() => AutoRetainerApi.FinishRetainerPostProcess());
    }

    private bool LockMarketbuddy()
    {
        if (Marketbuddy_IPCSubscriber.IsEnabled && !Marketbuddy_IPCSubscriber.IsLocked(nameof(Auctioneer)))
        {
            Marketbuddy_IPCSubscriber.Lock(nameof(Auctioneer));
            return true;
        }

        if (Marketbuddy_IPCSubscriber.IsEnabled && Marketbuddy_IPCSubscriber.IsLocked(nameof(Auctioneer)))
        {
            return true;
        }
        return false;
    }

    private bool UnlockMarketbuddy()
    {
        if (Marketbuddy_IPCSubscriber.IsEnabled && Marketbuddy_IPCSubscriber.IsLocked(nameof(Auctioneer)))
        {
            Marketbuddy_IPCSubscriber.Unlock(nameof(Auctioneer));
            return true;
        }
        return false;
    }

    private void AuctioneerOnRetainerPostProcessStep(string retainerName)
    {
        AutoRetainerApi.RequestRetainerPostprocess();
    }

    private void AuctioneerUpdate(IFramework framework)
    {
        if (TaskManager.IsBusy)
            return;
        if (RetainersToProcess.TryDequeue(out var retainer))
        {
            TaskManager.Enqueue(() => RetainerHelper.ClickOnRetainerByName(retainer.Name));
            TaskManager.Enqueue(() => RetainerHelper.OpenRetainerSellList());
            TaskManager.Enqueue(() => AdjustItems());
        }
    }

    internal Queue<GameRetainerManager.Retainer> RetainersToProcess { get; set; } = new();

    public void Process()
    {
        foreach (var retainer in GameRetainerManager.Retainers)
        {
            RetainersToProcess.Enqueue(retainer);
        }
    }

    private bool AdjustItems()
    {
        List<uint> items = new();
        TaskManager.EnqueueImmediate(() => RetainerHelper.GetSellListItems(ref items));
        TaskManager.EnqueueImmediate(() => LoopItems(ref items));
        return true;
    }

    private bool LoopItems(ref List<uint> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var index = i;

            TaskManager.EnqueueImmediate(() => RetainerHelper.ClickSellingItem(index));
            TaskManager.EnqueueImmediate(() => RetainerHelper.ClickAdjustPrice());

            var adjustTask = new AdjustItemPriceTask(items[index]);
            TaskManager.DelayNextImmediate(2000);
            TaskManager.EnqueueImmediate(() => adjustTask.ClickComparePrice());
            TaskManager.EnqueueImmediate(() => adjustTask.GetCurrentMarketPrice());
            TaskManager.EnqueueImmediate(() => adjustTask.AdjustPrice());
            TaskManager.DelayNextImmediate(2000);
        }
        return true;
    }

    public void Dispose()
    {
        AutoRetainerApi.Dispose();
        Marketbuddy_IPCSubscriber.Dispose();
        ECommonsMain.Dispose();
    }
}