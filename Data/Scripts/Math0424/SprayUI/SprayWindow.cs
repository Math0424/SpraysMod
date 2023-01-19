using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sprays.Math0424.SprayUI.Contents;
using VRageMath;

namespace Sprays.Math0424
{
    class SprayWindow : HudElementBase
    {
        private readonly EmptyHudElement content;

        private readonly WindowScrollContainer scrollContainer;

        public SprayWindow(HudParentBase parent) : base(parent)
        {
            SprayHud.Window = this;

            Size = new Vector2(250, HudMain.ScreenHeight);
            Offset = new Vector2((HudMain.ScreenWidth / 2) - (Size.X / 2), 0);

            content = new EmptyHudElement(this)
            {
                ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner,
                DimAlignment = DimAlignments.Height,
                Width = 200,
            };

            scrollContainer = new WindowScrollContainer(content);
           
            new WindowToolbar(this);

            Visible = false;

            UseCursor = true;
            ShareCursor = true;
        }

        public void ToggleVisibility()
        {
            Visible = !Visible;
            HudMain.EnableCursor = Visible;
        }

        public void ToggleContentVisibility(bool value)
        {
            content.Visible = value;
        }

        public void SetContentToScroll()
        {
            scrollContainer.Visible = true;
        }

    }
}
