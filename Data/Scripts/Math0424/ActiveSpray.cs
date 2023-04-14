using ProtoBuf;
using Sandbox.Game;
using Sandbox.Game.Entities;
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
        public static Dictionary<ActiveSpray, IMyEntity> Animated = new Dictionary<ActiveSpray, IMyEntity>();

        [ProtoMember(1)] public Vector3 Pos;
        [ProtoMember(2)] public Vector3 Norm;
        [ProtoMember(3)] public Vector3 Up;
        [ProtoMember(4)] public float Size;
        [ProtoMember(5)] public string SprayId;
        [ProtoMember(6)] public uint Flags;

        [ProtoIgnore] public uint Id { protected set; get; }
        [ProtoIgnore] public int CurrentFrame { protected set; get; }

        public bool IsAnimated => ((SprayDef.SprayFlags)Flags).HasFlag(SprayDef.SprayFlags.Animated);
        public bool IsEraser => SprayId.Equals(SprayDef.EraserGuid.ToString());

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

        public void IncrementFrame(IMyEntity ent)
        {
            CurrentFrame++;
            if (CurrentFrame >= (Flags >> 24))
            {
                CurrentFrame = 0;
            }
            Spray(ent);
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
                    Animated.Add(this, ent);
                }

                Spray(ent);
            }
        }

        private static List<uint> decalCache = new List<uint>(1);
        private void Spray(IMyEntity ent)
        {
            MyDecals.RemoveDecal(Id, true);

            var decalHit = new MyHitInfo()
            {
                Position = Vector3.Transform(Pos, ent.WorldMatrix),
                Normal = Vector3.TransformNormal(Norm, ent.WorldMatrix),
            };

            MyStringHash sprayHash;
            string HashValue = $"{SprayId}";

            if (IsAnimated)
                HashValue += $"_{CurrentFrame}";

            if (!MyStringHash.TryGet(HashValue, out sprayHash))
            {
                SprayUtil.Log($"Corrupt or missing spray {HashValue}");
                if(IsAnimated)
                    Animated.Remove(this);
                return;
            }

            decalCache.Clear();
            MyDecals.HandleAddDecal(ent,
                            decalHit,
                            Up,
                            MyStringHash.GetOrCompute("Default"),
                            sprayHash,
                            flags: MyDecalFlags.IgnoreRenderLimits | MyDecalFlags.IgnoreOffScreenDeletion,
                            decals: decalCache);

            if (decalCache.Count == 0 || decalCache[0] == 0)
            {
                //SprayUtil.Notify($"Failed to place spray {HashValue}", 2000, "Red");
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

            float depth = .25f;
            if (ent is IMyCubeGrid && ((IMyCubeGrid)ent).GridSizeEnum == VRage.Game.MyCubeSize.Large)
            {
                depth = 1f;
            }
            update.Transform.Translation = Pos - (Norm * (depth / 2.5f));
            
            
            //start fatblock logic
            if (ent is IMyCubeGrid) 
            {
                IMySlimBlock mySlimBlock = ((MyCubeGrid)ent).GetTargetedBlock(decalHit.Position - 0.001f * decalHit.Normal);
                if (mySlimBlock?.FatBlock != null)
                {
                    Vector3D pos = Vector3D.Transform(decalHit.Position, mySlimBlock.FatBlock.PositionComp.WorldMatrixInvScaled);
                    Vector3D normal = Vector3D.TransformNormal(decalHit.Normal, mySlimBlock.FatBlock.PositionComp.WorldMatrixInvScaled);

                    update.Transform = Matrix.CreateFromDir(normal, Up);
                    update.Transform.Translation = pos - (normal * (depth / 2.5f));
                }
            }
            //end fatblock logic


            update.Transform.Forward *= depth;
            update.Transform.Right *= Size;
            update.Transform.Up *= Size;
            
            MyDecals.UpdateDecals(new List<MyDecalPositionUpdate> { update });
        }

    }
}
