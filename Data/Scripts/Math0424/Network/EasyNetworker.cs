using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Math0424.Networking
{

    /// <summary>
    /// Author: Math0424
    /// Version 1.2
    /// </summary>
    public class EasyNetworker
    {

        private readonly ushort CommsId;
        public List<IMyPlayer> TempPlayers { get; private set; }
        
        /// <summary>
        /// Final packet in
        /// </summary>
        public Action<PacketIn> OnRecievedPacket;

        /// <summary>
        /// Before sending to players, serverside only
        /// </summary>
        public Action<PacketIn> ProcessPacket;

        public EasyNetworker(ushort CommsId)
        {
            this.CommsId = CommsId;
            TempPlayers = null;
        }

        public void Register()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(CommsId, RecivedPacket);
        }

        public void UnRegister()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(CommsId, RecivedPacket);
        }

        public void TransmitToServer(IPacket data, TransmitFlag flag)
        {
            PacketBase packet = new PacketBase(data.GetId(), flag);
            packet.Wrap(data);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }

        public void TransmitToPlayersWithinRange(Vector3D pos, IPacket data, TransmitFlag flag)
        {
            TransmitToPlayersWithinRange(pos, data, MyAPIGateway.Session.SessionSettings.SyncDistance, flag);
        }

        public void TransmitToPlayersWithinRange(Vector3D pos, IPacket data, int range, TransmitFlag flag)
        {
            PacketBase packet = new PacketBase(data.GetId(), flag);
            packet.Range = range;
            packet.TransmitLocation = pos;
            packet.Wrap(data);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }

        private void RecivedPacket(ushort handler, byte[] raw, ulong id, bool isFromServer)
        {
            try
            {
                PacketBase packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(raw);
                PacketIn packetIn = new PacketIn(packet.Id, packet.Data, id, isFromServer);

                ProcessPacket?.Invoke(packetIn);
                if (packetIn.IsCancelled)
                    return;

                if (isFromServer)
                {
                    if (MyAPIGateway.Session.IsServer && !packet.Flag.HasFlag(TransmitFlag.Final))
                    {
                        if (!packet.Flag.HasFlag(TransmitFlag.ExcludeSender))
                        {
                            OnRecievedPacket?.Invoke(packetIn);
                        }
                    } 
                    else
                    {
                        OnRecievedPacket?.Invoke(packetIn);
                    }
                }

                if (!packetIn.IsCancelled && MyAPIGateway.Session.IsServer && packet.Flag.HasFlag(TransmitFlag.AllPlayers))
                {
                    TransmitPacket(id, packet);
                }

            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Malformed packet from {id}!");
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}\n\n{e.InnerException}\n\n{e.Source}");
                
                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[Sprays mod critical error! | Send SpaceEngineers.Log]", 10000, MyFontEnum.Red);
            }
        }

        public void UpdatePlayers()
        {
            if (TempPlayers == null)
                TempPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
            else
                TempPlayers.Clear();

            MyAPIGateway.Players.GetPlayers(TempPlayers);
        }

        private void TransmitPacket(ulong sender, PacketBase packet)
        {
            UpdatePlayers();

            foreach (var p in TempPlayers)
            {
                if (p.IsBot || (packet.Flag.HasFlag(TransmitFlag.ExcludeSender) && p.SteamUserId == sender) || 
                    (MyAPIGateway.Session.IsServer && MyAPIGateway.Session?.Player?.SteamUserId == sender))
                    continue;

                PacketBase send = new PacketBase(packet.Id, TransmitFlag.Final);
                send.Data = packet.Data;

                if (packet.Range != -1)
                {
                    if (packet.Range >= Vector3D.Distance(p.GetPosition(), packet.TransmitLocation))
                    {
                        MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(send), p.SteamUserId);
                    }
                }
                else
                {
                    MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(send), p.SteamUserId);
                }
            }
        }

        [ProtoContract]
        private class PacketBase
        {

            [ProtoMember(1)] public int Id;
            [ProtoMember(2)] public int Range = -1;
            [ProtoMember(3)] public Vector3D TransmitLocation = Vector3D.Zero;
            [ProtoMember(4)] public TransmitFlag Flag;
            [ProtoMember(5)] public byte[] Data;

            public PacketBase() { }

            public PacketBase(int Id, TransmitFlag Flag)
            {
                this.Id = Id;
                this.Flag = Flag;
            }

            public void Wrap(object data)
            {
                Data = MyAPIGateway.Utilities.SerializeToBinary(data);
            }
        }
        public enum TransmitFlag
        {
            Final = 0,
            Server = 1,
            AllPlayers = 2,
            ExcludeSender = 4,
        }

        public interface IPacket
        {
            int GetId();
        }

        public class PacketIn {
            public bool IsCancelled { protected set; get; }
            public int PacketId { protected set; get; }
            public ulong SenderId { protected set; get; }
            public bool IsFromServer { protected set; get; }
            
            private readonly byte[] Data;

            public PacketIn(int packetId, byte[] data, ulong senderId, bool isFromServer)
            {
                this.PacketId = packetId;
                this.SenderId = senderId;
                this.IsFromServer = isFromServer;
                this.Data = data;
            }

            public T UnWrap<T>()
            {
                return MyAPIGateway.Utilities.SerializeFromBinary<T>(Data);
            }

            public void SetCancelled(bool value)
            {
                this.IsCancelled = value;
            }
        }

    }
}
