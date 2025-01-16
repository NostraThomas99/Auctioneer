using System.Runtime.CompilerServices;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.EzIpcManager;
using ECommons.Reflection;

namespace Auctioneer.IPC;

internal class IPCSubscriber_Common
{
    internal static bool IsReady(string pluginName)
    {
        try
        {
            return DalamudReflector.TryGetDalamudPlugin(pluginName, out IDalamudPlugin _, ignoreCache: true);
        }
        catch
        {
            return false;
        }
    }

    internal static void DisposeAll(EzIPCDisposalToken[] _disposalTokens)
    {
        foreach (EzIPCDisposalToken disposalToken in _disposalTokens)
        {
            try
            {
                disposalToken.Dispose();
            }
            catch (Exception ex)
            {
                IPluginLog log = Svc.Log;
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
                interpolatedStringHandler.AppendLiteral("Error while unregistering IPC: ");
                interpolatedStringHandler.AppendFormatted<Exception>(ex);
                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                object[] objArray = Array.Empty<object>();
                log.Error(stringAndClear, objArray);
            }
        }
    }
}