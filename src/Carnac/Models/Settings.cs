using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Windows;
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
        public int Placement { get; set; }

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
            get { return Placement == 1 || Placement == 3 ? 1 : -1; }
        }

        public string Alignment
        {
            get { return Placement == 1 || Placement == 2 ? "Left" : "Right"; }
        }


        public Thickness Margins
        {
            get { return new Thickness(LeftOffset, TopOffset, RightOffset, BottomOffset); }
        }

        public string SortDescription
        {
            get { return Placement == 1 || Placement == 3 ? "Ascending" : "Descending"; }
        }
    }
}
