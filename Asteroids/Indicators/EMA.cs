using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class EMA : BaseIndicator
    {
        private EnumBarStruct barStruct;
        private int _length;
        private double value;
        private AverageE averageE;
        private Color _lineColor;

        public EMA(List<BarData> barDatas, Color lineColor, EnumBarStruct objBarStruct = EnumBarStruct.Close,
            int length = 20, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            IsSimpleMode = isSimpleMode;
            _lineColor = lineColor;
            _length = length;
            barStruct = objBarStruct;
            string paramTag = string.Format("({0},{1})", barStruct.ToString(), _length);
            Name = string.Format("EMA{0}", paramTag);
            Description = "指数移动平均";
            valueDict.Add("EMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("EMA", new IndicatorGraph() { Name = "EMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            averageE = new AverageE();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(Color lineColor, EnumBarStruct objBarStruct, int length)
        {
            if (objBarStruct != barStruct || length != _length || lineColor != _lineColor)
            {
                _lineColor = lineColor;
                string paramTag = string.Format("({0},{1})", objBarStruct.ToString(), length);
                Name = string.Format("EMA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["EMA"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["EMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["EMA"].Clear();
            }
            averageE.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateEMA(i);
                }
            }
            base.Caculate();
        }

        private void GenerateEMA(int i)
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
            value = averageE.Caculate(GetData(curData));
            valueDict["EMA"].Add(value);
            if (!IsSimpleMode)
            {
                graphDict["EMA"].AddValue(value, _lineColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["EMA"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["EMA"].RemoveLast();
            }
            averageE.ResetValue();
            GenerateEMA(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateEMA(Count - 1);
            
            
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
            return valueDict["EMA"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["EMA"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["EMA"][Count - 1];
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
