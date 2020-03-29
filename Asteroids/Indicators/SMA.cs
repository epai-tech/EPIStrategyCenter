using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class SMA : BaseIndicator
    {
        private EnumBarStruct barStruct;
        private int _length;
        private AverageS averageS;
        private Color _lineColor;

        public SMA(List<BarData> barDatas, Color lineColor, EnumBarStruct objBarStruct = EnumBarStruct.Close, 
            int length = 10, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _lineColor = lineColor;
            barStruct = objBarStruct;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", barStruct.ToString(), _length);
            Name = string.Format("SMA{0}", paramTag);
            Description = "简单移动平均";
            valueDict.Add("SMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("SMA", new IndicatorGraph() { Name = "SMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            averageS = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(Color lineColor, EnumBarStruct objBarStruct, int length)
        {
            if (objBarStruct != barStruct || length != _length || _lineColor != lineColor)
            {
                _lineColor = lineColor;
                string paramTag = string.Format("({0},{1})", objBarStruct.ToString(), length);
                Name = string.Format("SMA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["SMA"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["SMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["SMA"].Clear();
            }
            averageS.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateSMA(i);
                }
            }
            base.Caculate();
        }

        private void GenerateSMA(int i)
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
            valueDict["SMA"].Add(value);
            if (!IsSimpleMode)
            {
                graphDict["SMA"].AddValue(value, _lineColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["SMA"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["SMA"].RemoveLast();
            }
            averageS.RemoveLast();
            GenerateSMA(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateSMA(Count - 1);
            
            
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

        public List<double> GetValues()
        { 
            return valueDict["SMA"]; 
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SMA"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["SMA"][Count - 1];
            else
                return JPR.NaN;
        }

        public Color LineColor { get { return _lineColor; } }

        public int Length
        {
            get { return _length; }
        }

        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
    }
}
