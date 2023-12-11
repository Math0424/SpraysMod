using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Sprays.Math0424
{
    internal static class Extensions
    {
        public static bool HasSprayStorage(this IMyEntity e)
        {
            if (e == null || e.Storage == null)
            {
                return false;
            }
            else
            {
                return e.Storage.ContainsKey(MySprayStorage.guid);
            }
        }

        public static MySprayStorage GetOrSetSprayStorage(this IMyEntity e)
        {
            if (e.Storage == null)
            {
                e.Storage = new MyModStorageComponent();
            }

            if (!e.Storage.Container.Contains(typeof(MySprayStorage)))
            {
                e.Storage.Container.Add(new MySprayStorage());
            }

            return e.Storage.Container.Get<MySprayStorage>();
        }

        public static void LoadSprays(this IMyEntity grid)
        {
            if (!grid.HasSprayStorage())
                return;

            MySprayStorage sprays = grid.GetOrSetSprayStorage();
            foreach (ActiveSpray s in sprays.GetSprays())
            {
                s.PlaceSpray(grid);
            }
        }

        public static void RemoveNearbySprays(this IMyEntity grid, Vector3 pos)
        {
            if (grid.HasSprayStorage())
            {
                MySprayStorage storage = grid.GetOrSetSprayStorage();

                List<ActiveSpray> remove = new List<ActiveSpray>();
                foreach (ActiveSpray spray in storage.GetSprays())
                {
                    if (Vector3.DistanceSquared(spray.Pos, pos) <= (1 + Math.Pow(spray.Size - 1, 2)) / 10.0)
                    {
                        remove.Add(spray);
                    }
                }
                foreach (ActiveSpray spray in remove)
                {
                    spray.Clear();
                    storage.RemoveFromSprays(spray);
                }
            }
        }

        public static bool CanSprayGrid(this IMyCubeGrid grid, long myId, ulong steamID)
        {
            MyAdminSettingsEnum flags;
            MyAPIGateway.Session.TryGetAdminSettings(steamID, out flags);
            if ((flags & MyAdminSettingsEnum.UseTerminals) != 0)
                return true;
            
            if (grid == null || (grid.BigOwners.Count == 0 && grid.SmallOwners.Count == 0) || grid.BigOwners.Contains(myId))
            {
                return true;
            }
            else
            {
                try
                { // you have broken me keen, screw you ill try catch this code
                    foreach (long member in MyAPIGateway.Session?.Factions?.TryGetPlayerFaction(myId)?.Members.Keys)
                    {
                        if (grid.BigOwners.Contains(member))
                        {
                            return true;
                        }
                    }
                }
                catch { }
            }
            return false;
        }

    }
}
