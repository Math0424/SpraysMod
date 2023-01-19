using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace Sprays.Math0424.SprayUI.Contents
{
    class SprayIcon : HudElementBase
    {

        public readonly SprayDef mySpray;
        private readonly Material image;
        private readonly Button selection;

        public SprayIcon(SprayDef spray, HudElementBase parent) : base(parent)
        {
            mySpray = spray;

            ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner;
            Size = new Vector2(150, 150);

            image = new Material(spray.ImageBillboard, Size);

            new TexturedBox(this)
            {
                Color = GetColor(),
                Size = new Vector2(-Size.X, Size.Y),
                ParentAlignment = ParentAlignments.Center,
            };

            new TexturedBox(this)
            {
                Material = image,
                Size = new Vector2(Size.X - 10, Size.Y - 10),
                ParentAlignment = ParentAlignments.Center,
            };

            selection = new Button(this)
            {
                DimAlignment = DimAlignments.Both,
                Color = new Color(0, 0, 0, 0),
                HighlightColor = new Color(41, 54, 62, 100),
                ParentAlignment = ParentAlignments.Center,
            };

            new LabelBox(this)
            {
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner,
                DimAlignment = DimAlignments.Width,
                Height = 20f,
                Color = new Color(41, 54, 62, 150),
                Format = new GlyphFormat(new Color(220, 235, 242), TextAlignment.Center),
                AutoResize = false,
                Text = spray.Name,
            };

            selection.MouseInput.LeftClicked += LeftClicked;
        }

        private void LeftClicked(object sender, EventArgs e)
        {
            Options.Current = mySpray;
            SprayHud.Window.ToggleVisibility();
        }

        private Color GetColor()
        {
            if (mySpray.MyEnumFlags.HasFlag(SprayDef.SprayFlags.AdminOnly))
            {
                return new Color(255, 51, 51, 150);
            }
            else if (mySpray.MyEnumFlags.HasFlag(SprayDef.SprayFlags.Animated))
            {
                return new Color(110, 26, 135, 150);
            }
            else if(mySpray.MyEnumFlags.HasFlag(SprayDef.SprayFlags.Hidden))
            {
                return new Color(100, 255, 102, 150);
            }
            else
            {
                return new Color(41, 54, 62, 150);
            }
        }


    }
}
