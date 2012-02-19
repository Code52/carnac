namespace Carnac.Logic.Native
{
    public class DetailedScreen
    {
        public int Index { get; set; }
        public string FriendlyName { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double RelativeHeight { get; set; }
        public double RelativeWidth { get; set; }

        public double Top { get; set; }
        public double Left { get; set; }

        public bool Placement1 { get; set; }
        public bool Placement2 { get; set; }
        public bool Placement3 { get; set; }
        public bool Placement4 { get; set; }
    }
}