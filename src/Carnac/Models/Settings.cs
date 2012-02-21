using System;
using Caliburn.Micro;
using System.Windows;
using Carnac.Enum;
using Carnac.Logic;

namespace Carnac.Models
{
    [Serializable]
    public class Settings : PropertyChangedBase
    {
        public int ItemMaxWidth { get; set; }
        public double ItemOpacity { get; set; }
        public string ItemBackgroundColor { get; set; }

        public string FontColor { get; set; }
        public int FontSize { get; set; }

        public int Screen { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "ScaleTransform", "Alignment" })]
        public NotificationPlacement Placement { get; set; }

        //Used to determine which from it's leftmost co-ord
        public double Left { get; set; }

        [NotifyProperty(AlsoNotifyFor = new [] { "Margins" } )]
        public int TopOffset { get; set; }
        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int BottomOffset { get; set; }
        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int LeftOffset { get; set; }
        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int RightOffset { get; set; }

        public double ScaleTransform
        {
            get { return Placement == NotificationPlacement.TopLeft || Placement == NotificationPlacement.TopRight ? 1 : -1; }
        }

        public string Alignment
        {
            get { return Placement == NotificationPlacement.TopLeft || Placement == NotificationPlacement.BottomLeft ? "Left" : "Right"; }
        }


        public Thickness Margins
        {
            get { return new Thickness(LeftOffset, TopOffset, RightOffset, BottomOffset); }
        }

        public string SortDescription
        {
            get { return Placement == NotificationPlacement.TopLeft || Placement == NotificationPlacement.TopRight ? "Ascending" : "Descending"; }
        }
    }
}
