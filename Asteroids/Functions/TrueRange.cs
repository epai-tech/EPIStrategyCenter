using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPI.Asteroids.Functions
{
    public class TrueRange:BaseFunction
    {
        TrueHigh trueHigh;
        TrueLow trueLow;

        public TrueRange()
        {
            Name = "TrueRange";
            Description = "求真实区间";
            trueHigh = new TrueHigh();
            trueLow = new TrueLow();
        }

        public double Caculate(double preClose, double curHigh, double curLow)
        {
            return trueHigh.Caculate(preClose, curHigh) - trueLow.Caculate(preClose, curLow);
        }
    }
}
