using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace Projekt_Inz
{
    public class Gesture
    {
        public List<int> depths { set; get; }
        public List<int> xList { set; get; }
        public string type { set; get; }

        public Gesture(List<int> depths,List<int> xList)
        {
            this.depths = depths;
            this.xList = xList;
        }
    }
}
