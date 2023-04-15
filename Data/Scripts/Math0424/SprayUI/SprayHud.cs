using RichHudFramework.Client;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace Sprays.Math0424
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class SprayHud : MySessionComponentBase
    {

        public static SprayWindow Window;
        private static IBindGroup sprayBinds;

        public static void Init()
        {
            if (Window == null && !MyAPIGateway.Utilities.IsDedicated)
                new SprayHud();
        }

        protected SprayHud()
        {
            RichHudClient.Init("SpraysMod", HudInit, ClientReset);
        }

        private void HudInit()
        {
            RichHudTerminal.Root.Enabled = true;
            new SprayWindow(HudMain.Root);

            var binds = GlobalStorage.LoadKeybinds();

            sprayBinds = BindManager.GetOrCreateGroup("Sprays Mod");
            sprayBinds.RegisterBinds(binds);

            RichHudTerminal.Root.Add(new RebindPage()
            {
                Name = "Binds",
                GroupContainer = { { sprayBinds, binds } }
            });

            sprayBinds[0].NewPressed += (a, b) => ToggleSprayWindow();

            sprayBinds[1].NewPressed += (a, b) => CancelSpray();
            sprayBinds[2].NewPressed += (a, b) => EraserSpray();
            sprayBinds[3].NewPressed += (a, b) => PreviousSpray();
            sprayBinds[4].NewPressed += (a, b) => IncreaseSpraySize();
            sprayBinds[5].NewPressed += (a, b) => DecreaseSpraySize();

            sprayBinds[6].NewPressed += (a, b) => FlipSpray();
        }

        private void ToggleSprayWindow()
        {
            if (CanUseInput())
            {
                Options.Current = null;
                Window?.ToggleVisibility();
            }
        }

        private void CancelSpray()
        {
            if (CanUseInput())
            {
                Options.Current = null;
            }
        }

        private void EraserSpray()
        {
            if (CanUseInput())
            {
                Options.Current = new SprayDef(SprayDef.EraserGuid, "GenericEraser", 0);
            }
        }

        private void PreviousSpray()
        {
            if (CanUseInput())
            {
                Options.Current = Options.Previous;
            }
        }

        public void IncreaseSpraySize()
        {
            if (CanUseInput() && Options.Current != null)
            {
                if (Options.NoLimits)
                {
                    if (Options.Size > 15)
                        Options.Size *= 1.05f;
                    else
                        Options.Size = Options.Size + .5f;
                }
                else
                    Options.Size = Math.Min(Options.Size + .5f, 10);
            }
        }

        public void DecreaseSpraySize()
        {
            if (CanUseInput() && Options.Current != null)
            {
                Options.Size = Math.Max(Options.Size - .5f, .1f);
            }
        }

        public void FlipSpray()
        {
            if (CanUseInput() && Options.Current != null)
            {
                Options.IsFlipped = !Options.IsFlipped;
            }
        }

        private bool CanUseInput()
        {
            return !MyAPIGateway.Gui.IsCursorVisible && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None;
        }

        private void ClientReset() {}


    }
}
