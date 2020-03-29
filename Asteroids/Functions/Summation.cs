using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;

namespace EPI.Asteroids.Functions
{
    public class Summation:BaseFunction
    {
        private int _length;
        private List<double> windowsList;

        public Summation()
        {
            Name = "Summation";
            Description="求和";
            windowsList = new List<double>();
        }

        public void SetParameters(int length)
        {
            _length = length;
            windowsList.Clear();
        }

        public double AddValue(double data)
        {
            double result;
            if (windowsList.Count == _length)
            { windowsList.RemoveAt(0); }
            else if (windowsList.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            windowsList.Add(data);
            if (windowsList.Count == _length)
            { result = windowsList.Sum(); }
            else
            { result = JPR.NaN; }
            return result;
        }

        public void RemoveLast()
        {
            windowsList.RemoveAt(windowsList.Count - 1);
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
