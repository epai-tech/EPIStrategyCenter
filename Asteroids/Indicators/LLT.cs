using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Common;

namespace EPI.Asteroids.Indicators
{
    public class LLT : BaseIndicator
    {
        private int _length;
        private double factor;
        private double factorP2;
        private double LLTValue;
        private Color _lineColor;
        private EnumBarStruct barStruct;

        public LLT(List<BarData> barDatas, Color lineColor, EnumBarStruct objBarStruct = EnumBarStruct.Close, int length = 20, bool isSimpleMode = true,
            bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            IsSimpleMode = isSimpleMode;
            _length = length;
            _lineColor = lineColor;
            barStruct = objBarStruct;
            factor = 2d / (length + 1);
            factorP2 = Math.Pow(factor, 2);
            string paramTag = string.Format("({0},{1})", objBarStruct.ToString(), length);
            Name = string.Format("LLT{0}", paramTag);
            Description = "低延迟趋势线";
            valueDict.Add("LLT",new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("LLT", new IndicatorGraph() { Name = "LLT", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(Color lineColor, EnumBarStruct objBarStruct, int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0},{1})", objBarStruct.ToString(), length);
                Name = string.Format("LLT{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["LLT"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _length = length;
                _lineColor = lineColor;
                factor = 2d / (length + 1);
                factorP2 = Math.Pow(factor, 2);
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["LLT"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["LLT"].Clear();
            }
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateLLT(i);
                }
            }
            base.Caculate();
        }

        private void GenerateLLT(int i)
        {
            BarData curData = null;
            BarData preData = null;
            BarData preData2 = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(1);
                preData2 = GetBarData(2);
            }
            else
            {
                curData = barDatas[i];
                if (i > 0 && i < barDatas.Count)
                {
                    preData = barDatas[i - 1];
                }
                if (i > 1 && i < barDatas.Count)
                {
                    preData2 = barDatas[i - 2];
                }
            }
            var value1 = GetData(curData);
            if (i <= 1)
                LLTValue = value1;
            else
            {
                var value2 = GetData(preData);
                var value3 = GetData(preData2);
                LLTValue = (factor - factorP2 / 4d) * value1 + factorP2 / 2d * value2 - (factor - 3d * factorP2 / 4d) * value3 + 2 * (1 - factor) * valueDict["LLT"][i - 1] - Math.Pow(1 - factor, 2) * valueDict["LLT"][i - 2];
            }
            valueDict["LLT"].Add(LLTValue);
            if (!IsSimpleMode)
            {
                graphDict["LLT"].AddValue(LLTValue, _lineColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["LLT"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["LLT"].RemoveLast();
            }
            GenerateLLT(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateLLT(Count - 1);
            
            
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
            return valueDict["LLT"];
        }
        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["LLT"][index];
            else
                return JPR.NaN;
        }
        public double GetLast()
        {
            if (Count != 0)
                return valueDict["LLT"][Count - 1];
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
