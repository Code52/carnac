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

        public bool NotificationPlacementTopLeft { get; set; }
        public bool NotificationPlacementBottomLeft { get; set; }
        public bool NotificationPlacementTopRight { get; set; }
        public bool NotificationPlacementBottomRight { get; set; }
    }
}