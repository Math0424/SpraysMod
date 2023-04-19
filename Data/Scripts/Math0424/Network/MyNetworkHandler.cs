using Math0424.Networking;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using static Math0424.Networking.EasyNetworker;

namespace Sprays.Math0424
{
    class MyNetworkHandler : IDisposable
    {

        public EasyNetworker MyNetwork;
        public static MyNetworkHandler Static;

        public static void Init()
        {
            if (Static == null)
            {
                Static = new MyNetworkHandler();
            }
        }

        protected MyNetworkHandler()
        {
            MyNetwork = new EasyNetworker(37725);
            MyNetwork.Register();

            MyNetwork.OnRecievedPacket += PacketIn;
            MyNetwork.ProcessPacket += PacketProcess;
        }

        private void PacketIn(PacketIn e)
        {
            if (e.PacketId == 1)
            {
                e.UnWrap<PacketNewSpray>().ActivateSpray();
            }
        }

        private void PacketProcess(PacketIn e)
        {
            if (MyAPIGateway.Session.IsServer && !e.IsFromServer && e.PacketId == 1)
            {
                PacketNewSpray packet = e.UnWrap<PacketNewSpray>();
                IMyCubeGrid grid = MyEntities.GetEntityById(packet.GridId) as IMyCubeGrid;
                long id = MyAPIGateway.Players.TryGetIdentityId(e.SenderId);
                if (grid != null && id != 0)
                {
                    if ((int)MyAPIGateway.Session.GetUserPromoteLevel(e.SenderId) < 3)
                    {
                        if (((SprayDef.SprayFlags)packet.Spray.Flags).HasFlag(SprayDef.SprayFlags.AdminOnly))
                        {
                            //they modified games files to see admin sprays
                            SprayUtil.Log($"Player '{e.SenderId}' tried to spray an admin only spray!");
                            e.SetCancelled(true);
                        }
                        else if (!grid.CanSprayGrid(id, e.SenderId))
                        {
                            //they modified game files to spray anything they want
                            SprayUtil.Log($"Player '{e.SenderId}' tried to spray a grid they dont own!");
                            e.SetCancelled(true);
                        }
                    }
                }
                else
                {
                    //malicious packet?
                    SprayUtil.Log($"Recieved malformed packet request from '{e.SenderId}' on requested grid '{packet.GridId}'");
                    e.SetCancelled(true);
                }

                if (!e.IsCancelled && MyAPIGateway.Utilities.IsDedicated)
                {
                    MyNetwork?.OnRecievedPacket.Invoke(e);
                }
            }
        }

        public void Dispose()
        {
            MyNetwork.UnRegister();
            MyNetwork = null;
            Static = null;
        }
    }
}
