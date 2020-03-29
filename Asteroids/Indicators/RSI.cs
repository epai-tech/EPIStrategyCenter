using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class RSI :BaseIndicator
    {
        private int _length;
        List<double> netChgAvgList;
        List<double> totChgAvgList;

        public RSI(List<BarData> bars, int length = 14, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("RSI{0}", paramTag);
            Description = "相对强弱指数";
            valueDict.Add("RSI", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("RSI", new IndicatorGraph() { Name = "RSI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            netChgAvgList = new List<double>();
            totChgAvgList = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("RSI{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["RSI"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["RSI"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["RSI"].Clear();
            }
            netChgAvgList.Clear();
            totChgAvgList.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateRSI(i);
                }
            }
            base.Caculate();
        }

        private void GenerateRSI(int i)
        {
            BarData curData = null;
            BarData preData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(1);
            }
            else
            {
                curData = barDatas[i];
                if (i > 0 && i < barDatas.Count)
                {
                    preData = barDatas[i - 1];
                }
            }
            if (i > 0)
            {
                if (netChgAvgList.Count == _length)
                {
                    netChgAvgList.RemoveAt(0);
                    totChgAvgList.RemoveAt(0);
                }
                else if (netChgAvgList.Count > _length)
                { throw new Exception("计算数量超出范围"); }
                netChgAvgList.Add(Math.Max(curData.Close - preData.Close, 0));
                totChgAvgList.Add(Math.Abs(curData.Close - preData.Close));
            }
            if (i < _length)
            {
                valueDict["RSI"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["RSI"].AddValue(JPR.NaN, Color.Yellow);
                }
            }
            else
            {
                double value = 50;
                if (totChgAvgList.Average() != 0)
                    value = netChgAvgList.Average() / totChgAvgList.Average() * 100;
                valueDict["RSI"].Add(value);
                if (!IsSimpleMode)
                {
                    graphDict["RSI"].AddValue(value, Color.Yellow);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["RSI"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["RSI"].RemoveLast();
            }
            netChgAvgList.RemoveAt(netChgAvgList.Count - 1);
            totChgAvgList.RemoveAt(totChgAvgList.Count - 1);
            GenerateRSI(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateRSI(Count - 1);
            
            
        }

        public List<double> GetValues()
        {
            return valueDict["RSI"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["RSI"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["RSI"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
