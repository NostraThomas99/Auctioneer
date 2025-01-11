using System.Numerics;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ImGuiNET;

namespace Auctioneer;

public class ControlWindow : Window
{
    private readonly Auctioneer _auctioneer;
    public ControlWindow(Auctioneer plugin) : base("Auctioneer Control", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar)
    {
        _auctioneer = plugin;
        IsOpen = true;
    }
    public override void Draw()
    {
        if (ImGui.Button("Start"))
        {
            _auctioneer.Process();
        }
        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            _auctioneer.TaskManager.Abort();
            _auctioneer.RetainersToProcess.Clear();
        }
        ImGui.Text("Status: " + Auctioneer.Status);
        ImGui.Text(string.Join(", ", _auctioneer.TaskManager.TaskStack));
    }

    public override bool DrawConditions()
    {
        var bells = Svc.Objects.Where(o => o.IsRetainerBell() && o.IsTargetable && Vector3.Distance(o.Position, Player.Position) < 3).ToList();
        return bells.Count > 0;
    }
}