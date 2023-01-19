using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage.Input;

namespace Sprays.Math0424
{
    internal class GlobalStorage
    {
        static string keybindFile = "SpraysMod-Keybinds.cfg";

        static BindGroupInitializer defaultBinds = new BindGroupInitializer
        {
            { "Toggle hud", MyKeys.Shift, MyKeys.OemPipe },
            { "Cancel", MyKeys.Control, MyKeys.Shift, MyKeys.Q },
            { "Eraser", MyKeys.Control, MyKeys.Shift, MyKeys.E },
            { "Previous", MyKeys.Control, MyKeys.Shift, MyKeys.R },

            { "Increase spray size", MyKeys.Control, MyKeys.Shift, MyKeys.OemPlus },
            { "Decrease spray size", MyKeys.Control, MyKeys.Shift, MyKeys.OemMinus },

            { "Flip", MyKeys.F },
        };

        public static void SaveKeybinds(BindDefinition[] binds)
        {
            if (binds == null)
                return;

            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(keybindFile))
                MyAPIGateway.Utilities.DeleteFileInGlobalStorage(keybindFile);

            using (var x = MyAPIGateway.Utilities.WriteFileInGlobalStorage(keybindFile))
            {
                x.Write(MyAPIGateway.Utilities.SerializeToXML(binds));
            }
        }

        public static BindDefinition[] LoadKeybinds()
        {
            if (MyAPIGateway.Utilities.FileExistsInGlobalStorage(keybindFile))
            {
                using (var x = MyAPIGateway.Utilities.ReadFileInGlobalStorage(keybindFile))
                {
                    return MyAPIGateway.Utilities.SerializeFromXML<BindDefinition[]>(x.ReadToEnd());
                }
            } 
            else
            {
                return defaultBinds.GetBindDefinitions();
            }
        }

    }
}
