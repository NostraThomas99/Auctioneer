using Auctioneer.Helpers;
using Auctioneer.Tasks;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.Automation;
using ECommons.Automation.LegacyTaskManager;
using ECommons.DalamudServices;

namespace Auctioneer;

// ReSharper disable once ClassNeverInstantiated.Global
public class Auctioneer : IDalamudPlugin
{
    private ControlWindow _controlWindow;
    private WindowSystem _windowSystem;
    public static string Status { get; set; } = "Ready";
    public TaskManager TaskManager;
    public Auctioneer(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
        _controlWindow = new ControlWindow(this);
        _windowSystem = new WindowSystem();

        TaskManager = new TaskManager();

        _windowSystem.AddWindow(_controlWindow);

        Svc.PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        Svc.Framework.Update += AuctioneerUpdate;
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
        TaskManager.Enqueue(() => RetainerHelper.CloseRetainerSellList());
        TaskManager.Enqueue(() => RetainerHelper.CloseSelectString());
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
            TaskManager.EnqueueImmediate(() => adjustTask.Enter());
            TaskManager.EnqueueImmediate(() => adjustTask.Execute());
            TaskManager.DelayNextImmediate(2000);
        }
        return true;
    }

    public void Dispose()
    {
        ECommonsMain.Dispose();
    }
}