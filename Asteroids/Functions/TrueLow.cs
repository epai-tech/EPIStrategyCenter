using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPI.Asteroids.Functions
{
    public class TrueLow:BaseFunction
    {
        public TrueLow()
        {
            Name = "TrueLow";
            Description = "求真实低点";
        }

        public double Caculate(double preClose, double thisLow)
        {
            return thisLow <= preClose ? thisLow : preClose;
        }
    }
}
