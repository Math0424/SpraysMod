using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

namespace Sprays.Math0424
{
    [ProtoContract]
    public class ActiveSpray
    {

        private static MyStringHash defaultMat = MyStringHash.GetOrCompute("Default");
        public static ConcurrentDictionary<ActiveSpray, IMyEntity> Animated = new ConcurrentDictionary<ActiveSpray, IMyEntity>();

        [ProtoMember(1)] public Vector3 Pos;
        [ProtoMember(2)] public Vector3 Norm;
        [ProtoMember(3)] public Vector3 Up;
        [ProtoMember(4)] public float Size;
        [ProtoMember(5)] public string SprayId;
        [ProtoMember(6)] public uint Flags;

        [ProtoIgnore] public uint Id { protected set; get; }
        [ProtoIgnore] public int CurrentFrame { protected set; get; }

        [ProtoIgnore] public bool IsAnimated => ((SprayDef.SprayFlags)Flags).HasFlag(SprayDef.SprayFlags.Animated);
        [ProtoIgnore] public bool IsEraser => SprayId.Equals(SprayDef.EraserGuid.ToString());

        public ActiveSpray() { } //empty constructor for serialization

        public ActiveSpray(Vector3 pos, Vector3 norm, Vector3 up, SprayDef spray, float size)
        {
            this.Up = up;
            this.Pos = pos;
            this.Norm = norm;
            this.SprayId = spray.Id;
            this.Size = size;
            this.Flags = spray.Flags;
        }

        public void IncrementFrame(IMyEntity ent, bool update)
        {
            CurrentFrame++;
            if (CurrentFrame >= (Flags >> 24))
                CurrentFrame = 0;

            if (update)
                SprayOrUpdate(ent);
        }

        public void Clear()
        {
            if (IsAnimated)
                Animated.Remove(this);
            MyDecals.RemoveDecal(Id, true);
        }

        public void PlaceSpray(IMyEntity ent)
        {
            if (IsEraser)
            {
                ent.RemoveNearbySprays(Pos);
            }
            else
            {
                if (IsAnimated)
                {
                    if (Animated.ContainsKey(this))
                        return;
                    Animated.TryAdd(this, ent);
                }

                SprayOrUpdate(ent);
            }
        }

        private static List<uint> decalCache = new List<uint>(1);
        private void SprayOrUpdate(IMyEntity ent)
        {
            MyDecals.RemoveDecal(Id, true);

            MyStringHash sprayHash;
            string HashValue = SprayId;
            if (IsAnimated)
                HashValue = $"{SprayId}_{CurrentFrame}";

            if (!MyStringHash.TryGet(HashValue, out sprayHash))
            {
                if (IsAnimated)
                    Animated.Remove(this);
                return;
            }

            MyDecalRenderInfo myDecalRenderInfo = new MyDecalRenderInfo
            {
                PhysicalMaterial = defaultMat,
                VoxelMaterial = defaultMat,
                Flags = MyDecalFlags.IgnoreRenderLimits | MyDecalFlags.IgnoreOffScreenDeletion,
                AliveUntil = int.MaxValue,
                IsTrail = false,
                
                Source = sprayHash,
                Forward = Up,
                Normal = Norm,
                Position = Pos,
                RenderObjectIds = ent.Render.RenderObjectIDs,
            };

            decalCache.Clear();
            MyDecals.AddDecal(ref myDecalRenderInfo, decalCache);

            if (decalCache.Count == 0 || decalCache[0] == 0)
            {
                if (IsAnimated)
                    Animated.Remove(this);
                return;
            }
            Id = decalCache[0];

            MyDecalPositionUpdate update = new MyDecalPositionUpdate
            {
                ID = Id,
                Transform = Matrix.CreateFromDir(Norm, Up),
            };

            update.Transform.Translation = Pos - (Norm * 0.1f); // .25 * 2.5
            update.Transform.Forward *= .25f;
            update.Transform.Right *= Size;
            update.Transform.Up *= Size;
            MyDecals.UpdateDecals(new List<MyDecalPositionUpdate> { update });
        }

    }
}
