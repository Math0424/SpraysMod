using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sprays.Math0424.SprayUI.Contents
{
    class WindowScrollContainer : HudElementBase
    {

        private readonly TextBox input, hint;

        private readonly List<SprayFolder> folders = new List<SprayFolder>();
        private readonly List<SprayIcon> search = new List<SprayIcon>();

        private readonly ScrollBar scrollbar;

        public WindowScrollContainer(HudParentBase parent) : base(parent)
        {
            DimAlignment = DimAlignments.Both;

            new TexturedBox(this)
            {
                Color = new Color(41, 54, 62, 150),
                DimAlignment = DimAlignments.Both,
                ParentAlignment = ParentAlignments.Center,
            };

            //Search
            new TexturedBox(this)
            {
                Color = new Color(41, 54, 100),
                Offset = new Vector2(0, -30),
                Size = new Vector2(200, 25),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                ZOffset = 2,
                UseCursor = true,
                ShareCursor = false,
            };

            input = new TextBox(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                Offset = new Vector2(0, -30),
                Size = new Vector2(200, 25),
                AutoResize = false,
                BuilderMode = TextBuilderModes.Unlined,
                Format = GlyphFormat.Blueish,
                Text = "",
                ZOffset = 2,
                EnableHighlighting = false,
            };
            input.InputEnabled = true;
            input.TextBoard.FixedSize = new Vector2(180, 20);

            hint = new TextBox(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                Offset = new Vector2(0, -30),
                Size = new Vector2(200, 25),
                AutoResize = false,
                BuilderMode = TextBuilderModes.Unlined,
                Format = GlyphFormat.Blueish,
                Text = "Search...",
                ZOffset = 2,
                EnableHighlighting = false,
            };
            //endSearch


            new LabelBox(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                DimAlignment = DimAlignments.Width,
                Height = 30f,
                Color = new Color(41, 54, 62),
                Format = new GlyphFormat(new Color(220, 235, 242), TextAlignment.Center),
                VertCenterText = true,
                AutoResize = false,
                Text = "Sprays",
                ZOffset = 2,
            };

            scrollbar = new ScrollBar(this)
            {
                Vertical = true,
                ParentAlignment = ParentAlignments.Right | ParentAlignments.Inner | ParentAlignments.Bottom,
                Padding = new Vector2(8f),
                Width = 20f,
                Height = HudMain.ScreenHeight - 60,
            };
            scrollbar.slide.SliderHeight = 50;

            foreach (string s in SpraysMod.RegistedSprays.Keys)
            {
                folders.Add(new SprayFolder(s, this));
            }

            UpdateIcons();
        }

        string previousSearch = "";
        public void UpdateIcons()
        {
            string searchInput = input.Text.ToString().ToLower();
            bool searching = (searchInput.Trim().Length != 0);

            float offset = scrollbar.Current - 65;
            foreach (SprayFolder folder in folders)
            {
                folder.Visible = !searching;
                if (folder.Visible)
                {
                    folder.Offset = new Vector2(-8, offset);
                    if (offset - folder.GetHeight() > 0 || offset < -HudMain.ScreenHeight)
                        folder.Visible = false;
                    else
                        folder.Visible = true;
                    offset -= folder.GetHeight() + 10;
                }
            }

            if (!searching) 
            {
                ClearSearch();
            } 
            else if(!previousSearch.Equals(searchInput))
            {
                previousSearch = searchInput;
                ClearSearch();
                foreach (var s in SpraysMod.RegistedSprays.Values)
                {
                    foreach (SprayDef spray in s)
                    {
                        if (search.Count <= 50)
                        {
                            if (spray.MyEnumFlags.HasFlag(SprayDef.SprayFlags.Hidden))
                            {
                                if (spray.Name.ToLower().Equals(searchInput))
                                    search.Add(new SprayIcon(spray, this));
                            }
                            else if (spray.Name.ToLower().Contains(input.Text.ToString().ToLower()))
                            {
                                if (spray.MyEnumFlags.HasFlag(SprayDef.SprayFlags.AdminOnly))
                                {
                                    if ((int)MyAPIGateway.Session.Player.PromoteLevel >= 3)
                                        search.Add(new SprayIcon(spray, this));
                                }
                                else
                                {
                                    search.Add(new SprayIcon(spray, this));
                                }
                            }
                        } 
                        else
                        {
                            break;
                        }
                    }
                }
            }

            foreach (SprayIcon icon in search)
            {
                icon.Offset = new Vector2(-8, offset);
                offset -= icon.Size.Y + 10;
            }

        }

        private void ClearSearch()
        {
            while (search.Count > 0)
            {
                search[0].Unregister();
                search.RemoveAt(0);
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            base.HandleInput(cursorPos);
            IMouseInput control = scrollbar.slide.MouseInput;

            if (Visible)
            {
                float size = 0;
                if (search.Count != 0) {
                    search.ForEach((e) => size += e.Size.Y + 10);
                } else {
                    folders.ForEach((e) => size += e.GetHeight() + 10);
                }
                scrollbar.Max = Math.Max(0, size - HudMain.ScreenHeight + 40);


                hint.Visible = (input.Text.ToString().Length == 0);
                if (control.IsLeftClicked || !input.Text.ToString().Equals(previousSearch))
                    UpdateIcons();


                int scroll = MyAPIGateway.Input.DeltaMouseScrollWheelValue();
                if (scroll != 0)
                {
                    scrollbar.Current -= scroll / 3f;
                    UpdateIcons();
                }

            }
        }


    }
}
