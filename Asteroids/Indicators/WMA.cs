using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class WMA : BaseIndicator
    {
        private EnumBarStruct barStruct;
        private int _length;
        private AverageW averageW;
        private Color _lineColor;

        public WMA(List<BarData> barDatas, Color lineColor, EnumBarStruct objBarStruct = EnumBarStruct.Close, int length = 20
            , bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _lineColor = lineColor;
            barStruct = objBarStruct;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", barStruct.ToString(), _length);
            Name = string.Format("WMA{0}", paramTag);
            Description = "权重移动平均";
            valueDict.Add("WMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("WMA", new IndicatorGraph() { Name = "WMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            _length = length;
            _lineColor = lineColor;
            averageW = new AverageW();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(Color lineColor, EnumBarStruct objBarStruct, int length)
        {
            if (objBarStruct != barStruct || length != _length || lineColor != _lineColor)
            {
                _lineColor = lineColor;
                string paramTag = string.Format("({0},{1})", objBarStruct.ToString(), length);
                Name = string.Format("WMA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["WMA"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["WMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["WMA"].Clear();
            }
            averageW.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateWMA(i);
                }
            }
            base.Caculate();
        }

        private void GenerateWMA(int i)
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
            double value = averageW.AddValue(GetData(curData));
            valueDict["WMA"].Add(value);
            if (!IsSimpleMode)
            {
                graphDict["WMA"].AddValue(value, _lineColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["WMA"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["WMA"].RemoveLast();
            }
            averageW.RemoveLast();
            GenerateWMA(Count - 1);
            

            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateWMA(Count - 1);
            
            
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

        public Color LineColor { get { return _lineColor; } }

        public List<double> GetValues()
        {
            return valueDict["WMA"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["WMA"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["WMA"][Count - 1];
            else
                return JPR.NaN;
        }

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
