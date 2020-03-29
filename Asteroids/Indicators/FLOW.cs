using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using EPI.Common;
using System.Drawing;
using EPI.Asteroids.Functions;


namespace EPI.Asteroids.Indicators
{
    public class FLOW:BaseIndicator
    {
        private int _length;
        private Summation sumFlow;

        public FLOW(List<BarData> bars, int length = 20, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            :base(bars)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})",_length);
            Name = string.Format("FLOW{0}", paramTag);
            Description = "资金流向";
            valueDict.Add("FLOW", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("FLOW", new IndicatorGraph() { Name = "FLOW", Tag = paramTag, LineStyle = EnumLineStyle.Steam });
            }

            sumFlow = new Summation();
            IsShowInMain=isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("FLOW{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["FLOW"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["FLOW"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["FLOW"].Clear();
            }
            sumFlow.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateFLOW(i);
                }
            }
            base.Caculate();
        }

        private void GenerateFLOW(int i)
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
            double value = sumFlow.AddValue(curData.FloatMoney);
            valueDict["FLOW"].Add(value);
            if (value < 0)
            {
                if (!IsSimpleMode)
                {
                    graphDict["FLOW"].AddValue(value, Color.Green);
                }
            }
            else
            {
                if (!IsSimpleMode)
                {
                    graphDict["FLOW"].AddValue(value, Color.Red);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["FLOW"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["FLOW"].RemoveLast();
            }
            sumFlow.RemoveLast();
            GenerateFLOW(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateFLOW(Count - 1);
            
            
        }

        public List<double> GetValues()
        {
            return valueDict["FLOW"];
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["FLOW"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

    }
}
