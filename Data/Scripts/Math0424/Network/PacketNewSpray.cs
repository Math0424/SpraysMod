using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using static Math0424.Networking.EasyNetworker;

namespace Sprays.Math0424
{
    [ProtoContract]
    class PacketNewSpray : IPacket
    {

        [ProtoMember(1)] public ActiveSpray Spray { get; private set; }

        [ProtoMember(2)] public long GridId { get; private set; }

        public PacketNewSpray() { }

        public PacketNewSpray(long gridId, ActiveSpray spray)
        {
            this.GridId = gridId;
            this.Spray = spray;
        }

        public int GetId() => 1;

        public void ActivateSpray()
        {
            IMyEntity ent = MyEntities.GetEntityById(GridId);
            if (ent != null)
            {
                if (Spray.IsEraser)
                {
                    ent.RemoveNearbySprays(Spray.Pos);
                } 
                else
                {
                    if (!MyAPIGateway.Utilities.IsDedicated)
                        Spray.PlaceSpray(ent);

                    ent.GetOrSetSprayStorage().AddToSprays(Spray);
                }
            }
        }

    }
}
