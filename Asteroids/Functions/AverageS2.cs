using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;

namespace EPI.Asteroids.Functions
{
    public class AverageS2 : BaseFunction
    {
        private int _n;
        private int _m;
        private double preLastValue;
        private double lastValue;

        public AverageS2()
        {
            Name = "AverageS2";
            Description = "扩展指数加权移动平均";
            lastValue = JPR.NaN;
        }

        public void SetParameters(int n,int m)
        {
            _n = n;
            _m = m;
            lastValue = JPR.NaN;
        }

        public double Caculate(double data)
        {
            double result;
            if (lastValue == JPR.NaN && data != JPR.NaN)
            {
                preLastValue = lastValue = data;
                result = lastValue;
            }
            else
            {
                preLastValue = lastValue;
                lastValue = (_m * data + (_n - _m) * lastValue) / _n;
                result = lastValue;
            }
            return result;
        }

        public void ResetValue()
        {
            lastValue = preLastValue;
        }

        public int N
        {
            get { return _n; }
        }

        public int M
        {
            get { return _m; }
        }
    }
}