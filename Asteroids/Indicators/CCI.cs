using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class CCI : BaseIndicator
    {
        private int _length;
        List<double> tmpList;

        public CCI(List<BarData> bars, int length = 14, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("CCI{0}", paramTag);
            Description = "商品通道指标";
            valueDict.Add("CCI", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("CCI", new IndicatorGraph() { Name = "CCI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            tmpList = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("CCI{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["CCI"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }
  
        protected override void Caculate()
        {
            valueDict["CCI"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["CCI"].Clear();
            }
            tmpList.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateCCI(i);
                }
            }
            base.Caculate();
        }

        private void GenerateCCI(int i)
        {
            BarData curData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
            }
            else
            {
                curData = barDatas[i];
            }
            if (tmpList.Count == _length)
            {tmpList.RemoveAt(0);}
            else if (tmpList.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            tmpList.Add((curData.High + curData.Low + curData.Close));
            if (i < _length - 1)
            {
                valueDict["CCI"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["CCI"].AddValue(JPR.NaN, Color.Yellow);
                }
            }
            else
            {
                double mean = tmpList.Average();
                double avgDev = 0;
                for (int j = 0; j < _length; j++)
                {
                    avgDev = avgDev + Math.Abs(tmpList[j] - mean);
                }
                avgDev = avgDev / _length;
                double value = 0;
                if (avgDev != 0)
                    value = (tmpList[_length - 1] - mean) / (0.015 * avgDev);
                valueDict["CCI"].Add(value);
                if (!IsSimpleMode)
                {
                    graphDict["CCI"].AddValue(value, Color.Yellow);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["CCI"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["CCI"].RemoveLast();
            }
            tmpList.RemoveAt(tmpList.Count - 1);
            GenerateCCI(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateCCI(Count - 1);
            
            
        }

        public List<double> GetValues()
        {
            return valueDict["CCI"]; 
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["CCI"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["CCI"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
