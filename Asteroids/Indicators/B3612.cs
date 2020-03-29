
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{



    public class B3612 : BaseIndicator
    {
        //B36 : MA(CLOSE,3)-MA(CLOSE,6);
        //B612 : MA(CLOSE,6)-MA(CLOSE,12);
        private AverageS MA3;
        private AverageS MA6;
        private AverageS MA12;
        private EnumBarStruct barStruct;
        public B3612(List<BarData> bars, EnumBarStruct objBarStruct = EnumBarStruct.Close, bool isSimpleMode = true, bool isShowInMain = false, string tag = "")
            : base(bars)
        {
            Tag = tag;
            barStruct = objBarStruct;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", barStruct.ToString());
            Name = string.Format("B3612{0}", paramTag);
            Description = "三减六日乖离:\r\n"
                         + "原理：\r\n"
                         + "B36 收盘价的3日均线与6日均线的差\r\n"
                         + "B612 收盘价的6日均线与12日均线的差\r\n"
                         + "注：日为周期中的一个，也可以用其他周期。\r\n"
                         + "用法：\r\n"
                         + "乖离值围绕多空平衡点零上下波动，正数达到某个程度无法再往上升时，是卖出时机；\r\n"
                         + "反之，是买进时机。多头走势中，行情回档多半在三减六日乖离达到零附近获得支撑，\r\n"
                         + "即使跌破，也很快能够拉回。\r\n";
            valueDict.Add("B36", new List<double>());
            valueDict.Add("B612", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("B36", new IndicatorGraph() { Name = "B36", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("B612", new IndicatorGraph() { Name = "B612", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            MA3 = new AverageS();
            MA6 = new AverageS();
            MA12 = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct)
        {
            if (objBarStruct != barStruct)
            {
                string paramTag = string.Format("({0})", objBarStruct.ToString());
                Name = string.Format("B3612{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["B36"].Tag = paramTag;
                    graphDict["B612"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["B36"].Clear();
            valueDict["B612"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["B36"].Clear();
                graphDict["B612"].Clear();
            }
            MA3.SetParameters(3);
            MA6.SetParameters(6);
            MA12.SetParameters(12);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateB3612(i);
                }
            }
            base.Caculate();
        }

        private void GenerateB3612(int i)
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
            double ma3 = MA3.AddValue(GetData(curData));
            double ma6 = MA6.AddValue(GetData(curData));
            double ma12 = MA12.AddValue(GetData(curData));

            valueDict["B36"].Add(ma3 - ma6);
            valueDict["B612"].Add(ma6 - ma12);
            if (!IsSimpleMode)
            {
                graphDict["B36"].AddValue(ma3 - ma6, Color.White);
                graphDict["B612"].AddValue(ma6 - ma12, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["B36"].RemoveAt(Count - 1);
            valueDict["B612"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["B36"].RemoveLast();
                graphDict["B612"].RemoveLast();
            }
            MA3.RemoveLast();
            MA6.RemoveLast();
            MA12.RemoveLast();
            GenerateB3612(Count - 1);
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateB3612(Count - 1);
            
        }

        private double GetData(BarData bar)
        {
            double data;
            switch (barStruct)
            {
                case EnumBarStruct.Open:
                    data = bar.Open;
                    break;
                case EnumBarStruct.High:
                    data = bar.High;
                    break;
                case EnumBarStruct.Low:
                    data = bar.Low;
                    break;
                case EnumBarStruct.Close:
                    data = bar.Close;
                    break;
                case EnumBarStruct.Volume:
                    data = bar.Volume;
                    break;
                case EnumBarStruct.OpenInterest:
                    data = bar.OpenInterest;
                    break;
                case EnumBarStruct.Amount:
                    data = bar.Amount;
                    break;
                default:
                    data = 0;
                    break;
            }
            return data;
        }

        public List<double> GetB36Values()
        {
            return valueDict["B36"];
        }
        public List<double> GetB612Values()
        {
            return valueDict["B36"];
        }

        public double GetB36Value(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["B36"][index];
            else
                return JPR.NaN;
        }
        public double GetB612Value(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["B612"][index];
            else
                return JPR.NaN;
        }

        public double GetLastB36()
        {
            if (Count != 0)
                return valueDict["B36"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastB612()
        {
            if (Count != 0)
                return valueDict["B612"][Count - 1];
            else
                return JPR.NaN;
        }
        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
    }
}
