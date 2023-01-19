using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sprays.Math0424.SprayUI.Contents
{
    class SprayFolder : HudElementBase
    {
        private string groupName;
        private bool dropDown;
        private float dropDownOffset = 50;

        private readonly Label indicator;
        private readonly Button button;

        private readonly TexturedBox background;
        private readonly List<SprayIcon> icons = new List<SprayIcon>();

        public SprayFolder(string groupName, HudParentBase parent = null) : base(parent)
        {
            this.groupName = groupName;

            Size = new Vector2(175, 50);
            ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner;

            button = new Button(this)
            {
                Color = new Color(41, 54, 100, 200),
                DimAlignment = DimAlignments.Both,
            };

            button.MouseInput.LeftReleased += DropDownMenu;

            background = new TexturedBox(this)
            {
                Color = new Color(41, 54, 80, 175),
                Visible = false,
                ParentAlignment = ParentAlignments.Bottom,
                Size = new Vector2(175, 100),
            };

            new Label(this)
            {
                Text = groupName,
                DimAlignment = DimAlignments.Height,
                Offset = new Vector2(8, 0),
                BuilderMode = TextBuilderModes.Unlined,
                VertCenterText = false,
                Format = GlyphFormat.White,
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner
            };

            indicator = new Label(this)
            {
                Text = "^",
                DimAlignment = DimAlignments.Height,
                Offset = new Vector2(-8, 0),
                BuilderMode = TextBuilderModes.Unlined,
                VertCenterText = false,
                Format = GlyphFormat.White,
                ParentAlignment = ParentAlignments.Right | ParentAlignments.Inner
            };

        }

        public override bool Visible
        {
            get { return base.Visible; }

            set { 
                
                if (value && icons.Count == 0 && background != null && background.Visible)
                {

                    SpraysMod.RegistedSprays[groupName].ForEach(e => {
                        if (!e.MyEnumFlags.HasFlag(SprayDef.SprayFlags.Hidden))
                        {
                            if (e.MyEnumFlags.HasFlag(SprayDef.SprayFlags.Tebex))
                            {
                                //TODO: check user has tebex permission
                            }
                            if (e.MyEnumFlags.HasFlag(SprayDef.SprayFlags.AdminOnly))
                            {
                                if ((int)MyAPIGateway.Session.Player.PromoteLevel >= 3)
                                    icons.Add(new SprayIcon(e, background));
                            }
                            else
                            {
                                icons.Add(new SprayIcon(e, background));
                            }
                        }
                    });

                    float offset = -10;
                    icons.ForEach(e => {
                        e.Offset = new Vector2(0, offset);
                        offset -= e.Size.Y + 10;
                    });
                    background.Size = new Vector2(175, -offset);
                    dropDownOffset = -offset + 50;

                }
                else if(!value)
                {
                    foreach(SprayIcon i in icons)
                    {
                        i?.Unregister();
                    }
                    icons.Clear();
                }

                base.Visible = value;  
            }
        }

        public void DropDownMenu(object sender, EventArgs e)
        {
            dropDown = !dropDown;
            indicator.Text = (dropDown ? "v" : "^");
            background.Visible = dropDown;

            (Parent as WindowScrollContainer).UpdateIcons();
        }

        public float GetHeight()
        {
            return (dropDown ? dropDownOffset : Size.Y);
        }

        public int GetSize()
        {
            return icons.Count;
        }

    }
}
