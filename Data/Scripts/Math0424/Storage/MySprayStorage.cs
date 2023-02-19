using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;

namespace Sprays.Math0424
{
    internal class MySprayStorage : MyEntityStorageComponent
    {

        public readonly static Guid guid = new Guid("3ffd5fcf-e241-4f07-99ef-907509d40268");

        private List<ActiveSpray> sprays = new List<ActiveSpray>();

        public void AddToSprays(ActiveSpray s)
        {
            if (s.IsEraser)
            {
                return;
            }
            sprays.Add(s);
            Save();
        }

        public void RemoveFromSprays(ActiveSpray s)
        {
            ActiveSpray.Animated.Remove(s);
            sprays.Remove(s);
            Save();
        }

        private void Save()
        {
            if (Entity.Storage != null)
            {
                Entity.Storage[guid] = Convert.ToBase64String(MyAPIGateway.Utilities.SerializeToBinary(sprays));
            } 
        }

        public override sealed bool IsSerialized()
        {
            if (Entity.Storage != null)
            {
                Save();
            }
            return false;
        }

        public override sealed void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (Entity.InScene)
            {
                OnAddedToScene();
         
                if (!MyAPIGateway.Utilities.IsDedicated)
                    Entity.LoadSprays();
            }
        }

        public override sealed void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (Entity.Storage != null)
            {
                if (Entity.Storage.ContainsKey(guid))
                {
                    try
                    {
                        sprays = MyAPIGateway.Utilities.SerializeFromBinary<List<ActiveSpray>>(Convert.FromBase64String(Entity.Storage[guid]));
                    }
                    catch (Exception)
                    {
                        Entity.Storage.Remove(guid);
                    }
                }
            }
        }
        
        public List<ActiveSpray> GetSprays() => sprays;
        public override string ComponentTypeDebugString => "SpraysModStorage";

    }
}
