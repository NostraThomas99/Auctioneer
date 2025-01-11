using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;

namespace Auctioneer;

public static class Extensions
{
    private static string[] BellName => [Svc.Data.GetExcelSheet<EObjName>().GetRow(2000401).Singular.ExtractText(), "リテイナーベル"];

    internal static bool IsRetainerBell(this IGameObject o)
    {
        return o != null &&
               (o.ObjectKind == ObjectKind.EventObj || o.ObjectKind == ObjectKind.Housing)
               && o.Name.ToString().EqualsIgnoreCaseAny(BellName);
    }
}