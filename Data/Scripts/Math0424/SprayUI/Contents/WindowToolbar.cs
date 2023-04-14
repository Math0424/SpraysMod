using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Sprays.Math0424.SprayUI.Contents
{
    class WindowToolbar : HudElementBase
    {
        private int hoverTime = 0;

        private readonly Button sidebar;
        private readonly TexturedBox menu;

        private readonly LabelBoxButton eraser, continous, refresh, noLimits;

        private readonly ScrollBar sizeSlider;
        private readonly LabelBox sizeLabel;

        public WindowToolbar(HudParentBase parent) : base(parent)
        {
            Size = new Vector2(50, HudMain.ScreenHeight);
            ParentAlignment = ParentAlignments.Right | ParentAlignments.Inner;

            //sidebar stuff
            sidebar = new Button(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                DimAlignment = DimAlignments.Both,
                Color = new Color(41, 54, 100, 200),
                HighlightEnabled = false,
            };
            sidebar.MouseInput.LeftClicked += ClickedSidebar;

            new Label(sidebar)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                BuilderMode = TextBuilderModes.Lined,
                AutoResize = true,
                VertCenterText = true,
                Format = new GlyphFormat(Color.White, TextAlignment.Center, 3),
                Text = "T\nO\nO\nL\nK\nI\nT",
            };

            
            //toolbar stuff
            menu = new TexturedBox(this)
            {
                Visible = false,
                Color = new Color(41, 54, 100, 150),
                Size = new Vector2(200, HudMain.ScreenHeight),
                ParentAlignment = ParentAlignments.Left,
                UseCursor = true,
                ShareCursor = true,
            };

            
            eraser = new LabelBoxButton(menu)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, -10),
                Color = new Color(41, 54, 100),
                Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1f),
                AutoResize = false,
                Text = "Eraser",
                Size = new Vector2(180, 50),
            };
            eraser.MouseInput.LeftReleased += ClickedEraser;

            sizeLabel = new LabelBox(menu)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, -80),
                Color = new Color(41, 54, 100),
                Format = new GlyphFormat(Color.White, TextAlignment.Center),
                AutoResize = false,
                Text = "Size: 1",
                Size = new Vector2(180, 30),
            };

            sizeSlider = new ScrollBar(menu)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Center,
                Size = new Vector2(180, 20),
                Offset = new Vector2(0, -110),
                Vertical = false,
                Max = 10,
                Min = .1f,
            };
            sizeSlider.slide.BarColor = new Color(17, 23, 43);
            sizeSlider.slide.SliderColor = new Color(40, 30, 50);
            sizeSlider.Current = 1f;

            continous = new LabelBoxButton(menu)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, -360),
                Color = new Color(41, 54, 100),
                Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1f),
                AutoResize = false,
                Text = "Single mode",
                Size = new Vector2(180, 50),
            };
            continous.MouseInput.LeftReleased += ClickedContinuous;
           
            if (MyAPIGateway.Session.HasCreativeRights)
            {
                noLimits = new LabelBoxButton(menu)
                {
                    ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Center,
                    Offset = new Vector2(0, -480),
                    Color = new Color(41, 54, 100),
                    Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1f),
                    AutoResize = false,
                    Text = "NoLimits: False",
                    Size = new Vector2(180, 50),
                };
                noLimits.MouseInput.LeftReleased += ClickedNoLimits;
            }

            refresh = new LabelBoxButton(menu)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, -420),
                Color = new Color(41, 54, 100),
                Format = new GlyphFormat(GlyphFormat.Blueish.Color, TextAlignment.Center, 1f),
                AutoResize = false,
                Text = "Refresh sprays",
                Size = new Vector2(180, 50),
            };
            refresh.MouseInput.LeftReleased += ClickedRefresh;

            new LabelBox(menu)
            {
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, 90),
                Format = new GlyphFormat(Color.White, TextAlignment.Center),
                Color = new Color(41, 54, 100),
                AutoResize = false,
                Text = SpraysMod.RegistedSprays.Keys.Count + " folders",
                Size = new Vector2(180, 30),
            };

            var x = new LabelBox(menu)
            {
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, 50),
                Format = new GlyphFormat(Color.White, TextAlignment.Center),
                Color = new Color(41, 54, 100),
                AutoResize = false,
                Size = new Vector2(180, 30),
            };

            int y = 0;
            foreach(var i in SpraysMod.RegistedSprays.Values)
            {
                y += i.Count;
            }
            x.Text = y + " sprays";


            new LabelBox(menu)
            {
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner | ParentAlignments.Center,
                Offset = new Vector2(0, 10),
                Color = new Color(41, 54, 100),
                Format = new GlyphFormat(Color.White, TextAlignment.Center),
                AutoResize = false,
                Text = "By: Math0424",
                Size = new Vector2(180, 30),
            };

        }

        public void ClickedNoLimits(object sender, EventArgs e)
        {
            Options.NoLimits = !Options.NoLimits;
            noLimits.Text = new RichText($"NoLimits: {Options.NoLimits}");
        }

        public void ClickedEraser(object sender, EventArgs e)
        {
            Options.Current = new SprayDef(SprayDef.EraserGuid, "GenericEraser", 0);
            SprayHud.Window.Visible = false;
        }

        private void ClickedSidebar(object sender = null, EventArgs e = null)
        {
            if (hoverTime == -1)
            {
                sidebar.Color = new Color(41, 54, 100, 200);
                SetToolkitVisibility(false);
            } 
            else
            {
                SetToolkitVisibility(true);
                hoverTime = -1;
                sidebar.Color = new Color(50, 60, 180, 210);
            }
        }

        private void SetToolkitVisibility(bool value)
        {
            SprayHud.Window.ToggleContentVisibility(!value);
            hoverTime = 0;
            menu.Visible = value;
        }
        
        public void ClickedContinuous(object sender, EventArgs e)
        {
            Options.IsContinuousMode = !Options.IsContinuousMode;
            if (Options.IsContinuousMode)
            {
                continous.Text = "Continuous mode";
            } 
            else
            {
                continous.Text = "Single mode";
            }
        }

        public void ClickedRefresh(object sender, EventArgs e)
        {
            foreach(IMyEntity ent in MyEntities.GetEntities())
            {
                if (ent != null && ent is IMyCubeGrid)
                    ent.LoadSprays();
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            base.HandleInput(cursorPos);

            if (Visible)
            {

                if (sizeSlider.Visible)
                {
                    Options.Size = (float)Math.Round(sizeSlider.Current, 1);
                    sizeLabel.Text = "Size: " + (int)Options.Size;
                }

                if (hoverTime != -1)
                {
                    if (sidebar.IsMousedOver || menu.IsMousedOver)
                    {
                        hoverTime = Math.Min(hoverTime + 1, 55);
                    }
                    else if(hoverTime <= 20)
                    {
                        SetToolkitVisibility(false);
                        hoverTime = 0;
                    }
                    else
                    {
                        hoverTime--;
                    }
                }

                if (hoverTime == 50)
                {
                    SetToolkitVisibility(true);
                }

            } 
            else
            {
                hoverTime = 0;
            }

        }

    }
}
