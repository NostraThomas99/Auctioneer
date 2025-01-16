using ECommons.EzIpcManager;

namespace Auctioneer.IPC;

internal static class Marketbuddy_IPCSubscriber
{
    private static EzIPCDisposalToken[] _disposalTokens = EzIPC.Init(typeof(Marketbuddy_IPCSubscriber), "Marketbuddy");

    [EzIPC(null, true, null, SafeWrapper.Inherit)]
    internal static readonly Func<string, bool> IsLocked;

    [EzIPC(null, true, null, SafeWrapper.Inherit)]
    internal static readonly Func<string, bool> Lock;

    [EzIPC(null, true, null, SafeWrapper.Inherit)]
    internal static readonly Func<string, bool> Unlock;

    internal static bool IsEnabled => IPCSubscriber_Common.IsReady("Marketbuddy");

    internal static void Dispose()
    {
        IPCSubscriber_Common.DisposeAll(Marketbuddy_IPCSubscriber._disposalTokens);
    }
}