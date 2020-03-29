using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;

namespace EPI.Asteroids.Functions
{
    public class AverageW : BaseFunction
    {
        private int _length;
        private double divNum;
        private List<double> tmpList;

        public AverageW()
        {
            Name = "AverageW";
            Description = "权重求平均";
            tmpList = new List<double>();
        }

        public void SetParameters(int length)
        {
            _length = length;
            divNum = (1 + _length) * _length * 0.5d;
            tmpList.Clear();
        }

        public double AddValue(double data)
        {
            double result=0;
            if (tmpList.Count == _length)
            { tmpList.RemoveAt(0); }
            else if (tmpList.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            tmpList.Add(data);
            if (tmpList.Count == _length)
            {
                for (int i = 0; i < tmpList.Count; i++)
                {
                    result += (i + 1) * tmpList[i];
                }
                result /= divNum;
            }
            else
            { result = JPR.NaN; }
            return result;
        }

        public void RemoveLast()
        {
            tmpList.RemoveAt(tmpList.Count - 1);
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
