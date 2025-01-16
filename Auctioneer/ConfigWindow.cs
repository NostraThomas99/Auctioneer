using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using ImGuiNET;

namespace Auctioneer;

public class ConfigWindow : Window
{
    public ConfigWindow() : base("Auctioneer Configuration", ImGuiWindowFlags.None)
    {
        Size = new(300, 300);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        bool needsSaving = false;
        var postTask = Auctioneer.Config.RunAsPostTask;
        if (ImGui.Checkbox("Run as PostTask", ref postTask))
        {
            Auctioneer.Config.RunAsPostTask = postTask;
            needsSaving = true;
        }
        ImGuiEx.Tooltip("When enabled, Auctioneer will automatically adjust your listings when AutoRetainer finishes processing a retainer.");
        if (needsSaving)
            Auctioneer.Config.Save();
    }
}