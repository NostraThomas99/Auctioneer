using ECommons;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Auctioneer.Helpers;

internal static class GameRetainerManager
{
    public static unsafe bool Ready => RetainerManager.Instance()->Ready > (byte)0;

    public static unsafe Retainer[] Retainers
    {
        get
        {
            return RetainerManager.Instance()->Retainers.ToArray()
                .Where(x => x.RetainerId != 0UL && x.Name[0] > 0)
                .Select<RetainerManager.Retainer, Retainer>
                (x =>
                    new Retainer(x)).ToArray();
        }
    }

    public static int Count => GameRetainerManager.Retainers.Length;

    public class Retainer
    {
        public RetainerManager.Retainer Handle;
        public string Name;

        public uint VentureID => (uint)this.Handle.VentureId;

        public bool Available => this.Handle.ClassJob != (byte)0 && this.Handle.Available;

        public DateTime VentureComplete
        {
            get => GameRetainerManager.Retainer.DateFromTimeStamp(this.Handle.VentureComplete);
        }

        public ulong RetainerID => this.Handle.RetainerId;

        public uint Gil => this.Handle.Gil;

        public uint VentureCompleteTimeStamp => this.Handle.VentureComplete;

        public int MarkerItemCount => (int)this.Handle.MarketItemCount;

        public uint MarketExpire => this.Handle.MarketExpire;

        public int Level => (int)this.Handle.Level;

        public uint ClassJob => (uint)this.Handle.ClassJob;

        public RetainerManager.RetainerTown Town => this.Handle.Town;

        internal static DateTime DateFromTimeStamp(uint timeStamp)
        {
            return timeStamp != 0U
                ? new DateTime(((long)timeStamp + 62135596800L) * 10000000L, DateTimeKind.Utc)
                : DateTime.MinValue;
        }

        public Retainer(RetainerManager.Retainer handle)
        {
            this.Handle = handle;
            this.Name = handle.Name.Read();
        }
    }
}