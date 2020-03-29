using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPI.Asteroids.Functions
{
    public class TrueHigh:BaseFunction
    {
        public TrueHigh()
        {
            Name = "TrueHigh";
            Description = "求真实高点";
        }

        public double Caculate(double preClose, double thisHigh)
        {
            return thisHigh >= preClose ? thisHigh : preClose;
        }
    }
}
