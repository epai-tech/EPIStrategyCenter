using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class Envalops :  BaseIndicator
    {
        //UPPER : MA(CLOSE,N1)*(1+N2/100);//N1个周期收盘价均值与(1+N2/100)之积，定义为UPPER
        //LOWER : MA(CLOSE, N1)*(1-N2/100);//N1个周期收盘价均值与(1-N2/100)之积，定义为LOWER
        //N1 2-100-14 N2 1-100-6
        private AverageS averageS;
        private EnumBarStruct barStruct;
        private int _n1;
        private int _n2;

        public Envalops(List<BarData> bars,EnumBarStruct objBarStruct = EnumBarStruct.Close,int n1 = 14, int n2 = 6, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars) {
            Tag = tag;
            _n1 = n1;
            _n2 = n2;
            IsSimpleMode = isSimpleMode;
            barStruct = objBarStruct;
            string paramTag = string.Format("({0},{1},{2})", barStruct.ToString(), n1,n2);
            Name = string.Format("ENV{0}", paramTag);
            Description = "轨道线\r\n"
                         + "原理：\r]n"
                         + "收盘价在N1个周期内的简单移动平均向上浮动N2%得UPPER线，向下浮动N2%得LOWER线。\r\n"
                         + "参数：N1设定计算移动平均的天数，一般为14天\r\n"
                         + "用法：\r\n"
                         + "UPPER线为压力线， LOWER线起支撑作用。当价格上试UPPER线时应考虑卖出；\r\n" 
                         + "反之，当价格下穿LOWER线或得到支撑时应考虑买入。;\r\n";

            //EnvUpper 上轨， EnvLower 下轨
            valueDict.Add("EnvUpper", new List<double>());
            valueDict.Add("EnvLower", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("EnvUpper", new IndicatorGraph() { Name = "EnvUpper", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("EnvLower", new IndicatorGraph() { Name = "EnvLower", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            averageS = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct, int n1, int n2)
        {
            if (objBarStruct != barStruct || n1 != _n1 || n2 != _n2)
            {
                string paramTag = string.Format("({0},{1},{2})", objBarStruct.ToString(),n1,n2);
                Name = string.Format("ENV{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["EnvUpper"].Tag = paramTag;
                    graphDict["EnvLower"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _n1 = n1;
                _n2 = n2;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            /*清理掉上轨和下轨之前留存的数据
              清理掉用于辅助计算的均线数据
            */
            valueDict["EnvUpper"].Clear();
            valueDict["EnvLower"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["EnvUpper"].Clear();
                graphDict["EnvLower"].Clear();
            }
            averageS.SetParameters(_n1);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateENV(i);
                }
            }
            base.Caculate();
        }

        private void GenerateENV(int i)
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
            double value = averageS.AddValue(GetData(curData));
            double upper = value * (1 + _n2 / 100.0);
            double lower = value * (1 - _n2 / 100.0);
            valueDict["EnvUpper"].Add(upper);
            valueDict["EnvLower"].Add(lower);
            if (!IsSimpleMode)
            {
                graphDict["EnvUpper"].AddValue(upper, Color.Yellow);
                graphDict["EnvLower"].AddValue(lower, Color.Red);
            }
        }

        //当前k线数据持续的实时更新。需要删除上个Tick的数据，并且再次计算当前指标
        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["EnvUpper"].RemoveAt(Count - 1);
            valueDict["EnvLower"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["EnvUpper"].RemoveLast();
                graphDict["EnvLower"].RemoveLast();
            }
            averageS.RemoveLast();
            GenerateENV(Count - 1);
            
            
        }

        //来了新的K线数据，更新指标
        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateENV(Count - 1);
            
            
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

        //获得上轨的序列数据
        public List<double> GetUValues()
        {
            return valueDict["EnvUpper"];
        }

        //获得下轨的序列数据
        public List<double> GetLValues()
        {
            return valueDict["EnvLower"];
        }

        //获得下轨某个固定位置 index 的数据
        public double GetUValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["EnvUpper"][index];
            else
                return JPR.NaN;
        }

        //获得上轨某个固定位置 index 的数据
        public double GetLValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["EnvLower"][index];
            else
                return JPR.NaN;
        }

        //获得上轨的最后一个数据
        public double GetLastU()
        {
            if (Count != 0)
                return valueDict["EnvUpper"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得下轨的最后一个数据
        public double GetLastL()
        {
            if (Count != 0)
                return valueDict["EnvLower"][Count - 1];
            else
                return JPR.NaN;
        }

        //用于界面设置指标参数
        public int N1 { get { return _n1; } }

        public int N2 { get { return _n2; } }

        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
    }
}
