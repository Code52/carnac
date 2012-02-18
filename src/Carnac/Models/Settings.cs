using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

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
        public int Placement { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public double Height { get; set; }

        public string SortDescription
        {
            get { return Placement == 1 || Placement == 3 ? "Ascending" : "Descending"; }
        }
    }
}
