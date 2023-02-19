using Math0424.Networking;
using RichHudFramework.UI.Client;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Input;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Sprays.Math0424
{

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    internal class SpraysMod : MySessionComponentBase
    {
        private const string configPath = "Sprays.txt";

        public static readonly Dictionary<string, List<SprayDef>> RegistedSprays = new Dictionary<string, List<SprayDef>>();

        public override void LoadData()
        {
            GetSprays();
            MyNetworkHandler.Init();

            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                MyEntities.OnEntityAdd += OnEntityAddEvent;
            }
        }

        public void GetSprays()
        {
            try
            {
                int registered = 0;
                foreach(var mod in MyAPIGateway.Session.Mods)
                {
                    if (MyAPIGateway.Utilities.FileExistsInModLocation(configPath, mod))
                    {
                        var data = MyAPIGateway.Utilities.ReadFileInModLocation(configPath, mod).ReadToEnd();
                        MyIni ini = new MyIni();
                        MyIniParseResult result;
                        SprayUtil.Log($"Parsing {mod.Name}");
                        if (ini.TryParse(data, out result))
                        {
                            List<string> variables = new List<string>();
                            ini.GetSections(variables);
                            foreach (var section in variables)
                            {
                                var id = ini.Get(section, "ID").ToString();
                                var group = ini.Get(section, "Group").ToString();
                                var flags = ini.Get(section, "Flags").ToUInt32();

                                if (!RegistedSprays.ContainsKey(group))
                                    RegistedSprays.Add(group, new List<SprayDef>());

                                RegistedSprays[group].Add(new SprayDef(id, section, flags));
                                registered++;
                            }
                        }
                        else
                        {
                            SprayUtil.Log($"ERROR Cannot parse: {result.Error}");
                        }
                    }
                }
                SprayUtil.Log($"Registered {registered} sprays");
            }
            catch (Exception ex)
            {
                SprayUtil.Log($"Could not load sprays!");
                SprayUtil.Log(ex.Message);
                SprayUtil.Log(ex.StackTrace);
            }
        }

        public void OnEntityAddEvent(IMyEntity ent)
        {
            if (ent != null && ent is IMyCubeGrid)
                ent.LoadSprays();
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Utilities.IsDedicated)
                GlobalStorage.SaveKeybinds(BindManager.GetBindGroup("Sprays Mod")?.GetBindDefinitions());

            MyNetworkHandler.Static.Dispose();
            soundEmitter = null;
            spray_sound = null;
        }

        public override void UpdateBeforeSimulation()
        {
            Options.Frame++;
            SprayDef.FPS fps = SprayDef.FPS.None;
            
            if (Options.Frame % 60 == 0) fps |= SprayDef.FPS.Fps_1;
            if (Options.Frame % 12 == 0) fps |= SprayDef.FPS.Fps_5;
            if (Options.Frame % 4 == 0) fps |= SprayDef.FPS.Fps_15;
            if (Options.Frame % 3 == 0) fps |= SprayDef.FPS.Fps_20;
            if (Options.Frame % 2 == 0) fps |= SprayDef.FPS.Fps_30;

            foreach (ActiveSpray s in ActiveSpray.Animated.Keys)
            {
                //a bit of bitshifting for the FPS
                if ((s.Flags << 8 >> 27 & (int)fps) != 0)
                {
                    if (ActiveSpray.Animated[s] != null && !(ActiveSpray.Animated[s].Closed || ActiveSpray.Animated[s].MarkedForClose))
                    {
                        s.IncrementFrame(ActiveSpray.Animated[s]);
                    }
                }
            }

        }

        //Does this need to be in Draw?
        public override void Draw()
        {
            if (!MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Session?.Player != null)
            {

                if (SprayHud.Window == null)
                {
                    SprayHud.Init();
                }

                if (Options.Current != null && !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None && !MyAPIGateway.Session.IsCameraUserControlledSpectator)
                {
                    var view = MyAPIGateway.Session.Camera.WorldMatrix;
                    var target = view.Translation + view.Forward * 10;

                    IHitInfo hit;
                    if (MyAPIGateway.Physics.CastRay(view.Translation, target, out hit, 30) && hit?.HitEntity is IMyCubeGrid)
                    {
                        int adjustment = 30;
                        if (MyAPIGateway.Input.IsKeyPress(MyKeys.Control))
                            adjustment = 1;
                        else if (MyAPIGateway.Input.IsKeyPress(MyKeys.Shift))
                            adjustment = 10;

                        int scroll = MyAPIGateway.Input.DeltaMouseScrollWheelValue();
                        if (scroll != 0)
                        {
                            Options.Angle += scroll > 0 ? adjustment : -adjustment;
                            if (Options.Angle < 0)
                                Options.Angle = 0;
                            else if (Options.Angle > 720)
                                Options.Angle -= 360;
                        }

                        Vector3 up;
                        Matrix? mat = SprayUtil.GetRotation(hit, Options.Angle, out up);
                        if (!mat.HasValue)
                            return;
                        Matrix matrix = mat.Value;

                        MyTransparentGeometry.AddBillboardOriented(
                               MyStringId.GetOrCompute(Options.Current.ImageBillboard),
                               Vector4.One,
                               hit.Position + 0.01f * hit.Normal,
                               matrix.Right * (Options.IsFlipped ? -1 : 1),
                               matrix.Up,
                               Options.Size * .5f, -1);
                        
                        if (MyAPIGateway.Input.IsNewRightMousePressed())
                        {
                            IMyEntity ent = hit.HitEntity;
                            if (ent is MyCubeGrid && !((MyCubeGrid)ent).CanSprayGrid(MyAPIGateway.Session.Player.IdentityId))
                            {
                                MyAPIGateway.Utilities.ShowNotification("You do not own this grid!", 10000, MyFontEnum.Red);
                                return;
                            }
                            
                            PlaySpraySound();

                            //Will send spray packet to everyone - even self
                            //high pings may experience a slight delay but it will be in the right place
                            Vector3D pos = Vector3D.Transform(hit.Position, ent.PositionComp.WorldMatrixNormalizedInv);
                            Vector3D normal = Vector3D.TransformNormal(Options.IsFlipped ? -hit.Normal : hit.Normal, ent.PositionComp.WorldMatrixNormalizedInv);
                            MyNetworkHandler.Static.MyNetwork.TransmitToPlayersWithinRange(ent.PositionComp.GetPosition(), 
                                new PacketNewSpray(ent.EntityId, new ActiveSpray(pos, normal, up, Options.Current, Options.Size)), EasyNetworker.TransmitFlag.AllPlayers);

                            if (!Options.Current.IsEraser)
                                Options.Previous = Options.Current;

                            if (!Options.IsContinuousMode)
                                Options.Current = null;
                        }

                    }
                    else if(Options.Current != null && MyAPIGateway.Input.IsNewRightMousePressed())
                    {
                        Options.Current = null;
                    }
                }
            }
        }

        private MyEntity3DSoundEmitter soundEmitter;
        private MySoundPair spray_sound;
        private void PlaySpraySound()
        {
            if (soundEmitter == null)
            {
                soundEmitter = new MyEntity3DSoundEmitter((MyEntity)MyAPIGateway.Session.Player.Character);
                spray_sound = new MySoundPair("spray_sound");
            }

            if (soundEmitter != null && spray_sound != null)
            {
                soundEmitter.PlaySound(spray_sound);
            }
        }

    }
}
