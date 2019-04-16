using System;
using System.ComponentModel;
using System.Windows;
using Carnac.Logic.Enums;

namespace Carnac.Logic.Models
{
    public class PopupSettings : NotifyPropertyChanged
    {
        [DefaultValue(350)]
        public int ItemMaxWidth { get; set; }

        [DefaultValue(0.5)]
        public double ItemOpacity { get; set; }

        [DefaultValue(5)]
        public double ItemFadeDelay { get; set; }

        [DefaultValue("Black")]
        public string ItemBackgroundColor { get; set; }

        [DefaultValue("White")]
        public string FontColor { get; set; }

        [DefaultValue(40)]
        public int FontSize { get; set; }

        public int Screen { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "ScaleTransform", "Alignment" })]
        public NotificationPlacement Placement { get; set; }

        [DefaultValue(false)]
        public bool AutoUpdate { get; set; }

        //Used to determine which from it's leftmost co-ord
        double left;
        public double Left
        {
            get { return left; }
            set
            {
                left = value;
                OnLeftChanged(EventArgs.Empty);
            }
        }

        public event EventHandler LeftChanged;

        protected void OnLeftChanged(EventArgs e)
        {
            var handler = LeftChanged;
            if (handler != null) handler(this, e);
        }

        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int TopOffset { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int BottomOffset { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int LeftOffset { get; set; }

        [NotifyProperty(AlsoNotifyFor = new[] { "Margins" })]
        public int RightOffset { get; set; }

        [DefaultValue("")]
        public string ProcessFilterExpression { get; set;  }

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

        public bool DetectShortcutsOnly { get; set; }
        public bool ShowApplicationIcon { get; set; }
        public bool SettingsConfigured { get; set; }
        public bool ShowOnlyModifiers { get; set; }
        public bool ShowSpaceAsUnicode { get; set; }
    }
}
