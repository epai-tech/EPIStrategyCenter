using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class Williams:BaseIndicator
    {
        private int _num;
        private MaxValue highest;
        private MinValue Lowest;
        private double highestValue;
        private double lowestValue;
        private double WillValue;

        public Williams(List<BarData> barDatas, int num = 7, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _num = num;
            string paramTag = string.Format("{0}", _num);
            Name = string.Format("Williams{0}", paramTag);
            IsSimpleMode = isSimpleMode;
            Description = "威廉指标";
            valueDict.Add("Williams", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("Williams", new IndicatorGraph() { Name = "Williams", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            _num = num;
            highest = new MaxValue();
            Lowest = new MinValue();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int num)
        {
            if (num != _num)
            {
                string paramTag = string.Format("{0}", num);
                Name = string.Format("Williams{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["Williams"].Tag = paramTag;
                }
                _num = num;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["Williams"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["Williams"].Clear();
            }
            highest.SetParameters(_num);
            Lowest.SetParameters(_num);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateWilliams(i);
                }
            }
            base.Caculate();
        }

        private void GenerateWilliams(int i)
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
            if (i < _num)
            {
                highestValue = highest.AddValue(curData.High);
                lowestValue = Lowest.AddValue(curData.Low);
                valueDict["Williams"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["Williams"].AddValue(JPR.NaN, Color.White);
                }
            }
            else
            {
                highestValue = highest.AddValue(curData.High);
                lowestValue = Lowest.AddValue(curData.Low);
                WillValue = 100 * (highestValue - curData.Close) / (highestValue - lowestValue);
                valueDict["Williams"].Add(WillValue);
                if (!IsSimpleMode)
                {
                    graphDict["Williams"].AddValue(WillValue, Color.White);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["Williams"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["Williams"].RemoveLast();
            }
            highest.RemoveLast();
            Lowest.RemoveLast();
            GenerateWilliams(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateWilliams(Count - 1);
           
            
        }

        public List<double> GetValues()
        {
            return valueDict["Williams"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Williams"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["Williams"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Num
        {
            get { return _num; }
        }
    }
}
