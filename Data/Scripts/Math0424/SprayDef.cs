namespace Sprays.Math0424
{
    public class SprayDef
    {

        public static readonly string EraserGuid = "03a37764-64f1-45d5-8e49-b665e9bb3dff";

        public string Id { get; protected set; }
        public string Name { get; protected set; }

        public SprayFlags MyEnumFlags { get; protected set; }
        public uint Flags { get; protected set; }

        public string ImageBillboard => Id + "_b";
        public string ImageAlpha => Id + "_a";
        public string ImageColor => Id + "_c";
        public bool IsEraser => Id.Equals(EraserGuid);

        public SprayDef(string id, string name, uint flags)
        {
            this.Id = id;
            this.Name = name;

            this.Flags = flags;
            this.MyEnumFlags = (SprayFlags)flags;
        }

        public enum SprayFlags
        {
            None = 0,
            Hidden = 1,
            AdminOnly = 2,
            Animated = 4,
            Tebex = 8,
        }

        public enum FPS
        {
            None = 0,
            Fps_30 = 1,
            Fps_20 = 2,
            Fps_15 = 4,
            Fps_5 = 8,
            Fps_1 = 16,
        }


        public override string ToString()
        {
            return $"MySpray:{Name}-{Flags}";
        }


    }
}
