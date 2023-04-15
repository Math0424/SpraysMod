using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Sprays.Math0424
{
    internal static class SprayUtil
    {

        public static void DrawDebugLine(Vector3D pos, Vector3D dir, int r, int g, int b)
        {
            Vector4 color = new Vector4(r / 255, g / 255, b / 255, 1);
            MySimpleObjectDraw.DrawLine(pos, pos + dir * 10, MyStringId.GetOrCompute("Square"), ref color, 0.01f);
        }

        public static void Log(object message)
        {
            MyLog.Default.WriteLineAndConsole("SpraysMod: " + (message ?? "null").ToString());
        }

        public static void Message(object message)
        {
            MyAPIGateway.Utilities.ShowMessage("SpraysMod:", (message ?? "Null").ToString());
        }

        public static void Notify(object message, int ms = 2000, string color = "White")
        {
            MyAPIGateway.Utilities.ShowNotification((message ?? "null").ToString(), ms, color);
            MyVisualScriptLogicProvider.PlayHudSoundLocal();
        }


        public static Matrix? GetRotation(IHitInfo hit, float rotation, out Vector3 up)
        {
            if (hit.HitEntity != null)
            {
                if (hit.HitEntity is IMyCubeGrid)
                {
                    return GetGridRotation(hit, rotation, out up);
                } 
                else if(hit.HitEntity is MyVoxelBase)
                {
                    return GetWorldRotation(hit, rotation, out up);
                }
            }
            up = Vector3.Zero;
            return null;
        }

        private static Matrix? GetGridRotation(IHitInfo hit, float rotation, out Vector3 up)
        {
            MyCubeGrid grid = hit.HitEntity as MyCubeGrid;

            IMyEntity proxy = grid;
            Vector3D newPos = Vector3D.Transform(hit.Position, grid.PositionComp.WorldMatrixInvScaled);
            Vector3 newNormal = Vector3.TransformNormal(hit.Normal, grid.PositionComp.WorldMatrixInvScaled);

            Vector3 vector = Vector3.CalculatePerpendicularVector(newNormal);
            if (rotation != 0)
            {
                Quaternion quaternion = Quaternion.CreateFromAxisAngle(newNormal, MathHelper.ToRadians(rotation * 2));
                vector = new Vector3((new Quaternion(vector, 0f) * quaternion).ToVector4());
            }
            vector = Vector3.Normalize(vector);
            up = vector;

            return Matrix.Identity * Matrix.CreateWorld((Vector3)newPos - newNormal * 0.45f, newNormal, vector) * proxy.PositionComp.WorldMatrixRef;
        }

        private static Matrix GetWorldRotation(IHitInfo hit, float rotation, out Vector3 up)
        {
            MyVoxelBase map = hit.HitEntity as MyVoxelBase;

            Vector3D newPos = Vector3D.Transform(hit.Position, map.PositionComp.WorldMatrixInvScaled);
            Vector3 newNormal = Vector3.TransformNormal(hit.Normal, map.PositionComp.WorldMatrixInvScaled);

            Vector3 vector = Vector3.CalculatePerpendicularVector(newNormal);
            if (rotation != 0)
            {
                Quaternion quaternion = Quaternion.CreateFromAxisAngle(newNormal, MathHelper.ToRadians(rotation * 2));
                vector = new Vector3((new Quaternion(vector, 0f) * quaternion).ToVector4());
            }
            vector = Vector3.Normalize(vector);
            up = vector;

            return Matrix.Identity * Matrix.CreateWorld((Vector3)newPos - newNormal * 0.45f, newNormal, vector) * map.PositionComp.WorldMatrixRef;
        }

    }
}
