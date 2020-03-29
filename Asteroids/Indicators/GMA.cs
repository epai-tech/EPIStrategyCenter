using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class GMA : BaseIndicator
    {
        private EnumBarStruct barStruct;
        private int _n1;
        private int _n2;
        private int _n3;
        private int _n4;
        private int _n5;
        private AverageS averageS1;
        private AverageS averageS2;
        private AverageS averageS3;
        private AverageS averageS4;
        private AverageS averageS5;
        private Color _lineColor1;
        private Color _lineColor2;
        private Color _lineColor3;
        private Color _lineColor4;
        private Color _lineColor5;
        private int _theme;

        public GMA(List<BarData> barDatas, EnumBarStruct objBarStruct = EnumBarStruct.Close,
            int n1 = 5, int n2 = 10, int n3 = 20, int n4 = 40, int n5 = 60, int theme = 1
            , bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _theme = theme;
            _n1 = n1;
            _n2 = n2;
            _n3 = n3;
            _n4 = n4;
            _n5 = n5;
            IsSimpleMode = isSimpleMode;
            barStruct = objBarStruct;
            string paramTag = string.Format("({0},{1},{2},{3},{4},{5})", barStruct.ToString(), _n1, _n2, _n3, _n4, _n5);
            Name = string.Format("GMA{0}", paramTag);
            Description = "组合平均线";
            valueDict.Add("GMA1", new List<double>());
            valueDict.Add("GMA2", new List<double>());
            valueDict.Add("GMA3", new List<double>());
            valueDict.Add("GMA4", new List<double>());
            valueDict.Add("GMA5", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("GMA1", new IndicatorGraph() { Name = "GMA1", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("GMA2", new IndicatorGraph() { Name = "GMA2", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("GMA3", new IndicatorGraph() { Name = "GMA3", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("GMA4", new IndicatorGraph() { Name = "GMA4", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("GMA5", new IndicatorGraph() { Name = "GMA5", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            averageS1 = new AverageS();
            averageS2 = new AverageS();
            averageS3 = new AverageS();
            averageS4 = new AverageS();
            averageS5 = new AverageS();
            IsShowInMain = isShowInMain;
            _lineColor1 = Color.White;
            _lineColor2 = Color.Yellow;
            _lineColor3 = Color.Magenta;
            _lineColor4 = Color.Green;
            _lineColor5 = Color.GreenYellow;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct, int n1 = 10, int n2 = 10, int n3 = 10, int n4 = 10, int n5 = 10, int theme = 1)
        {
            if (objBarStruct != barStruct || n1 != _n1 || n2 != _n2 || n3 != _n3 || n4 != _n4 || n5 != _n5 || theme != _theme)
            {
                ChangeTheme(theme);
                barStruct = objBarStruct;
                _n1 = n1;
                _n2 = n2;
                _n3 = n3;
                _n4 = n4;
                _n5 = n5;
                string paramTag = string.Format("({0},{1},{2},{3},{4},{5})", barStruct.ToString(), _n1, _n2, _n3, _n4, _n5);
                Name = string.Format("GMA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["GMA1"].Tag = paramTag;
                    graphDict["GMA2"].Tag = paramTag;
                    graphDict["GMA3"].Tag = paramTag;
                    graphDict["GMA4"].Tag = paramTag;
                    graphDict["GMA5"].Tag = paramTag;
                }
                Caculate();
            }
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            _lineColor1 = Color.White;
            _lineColor2 = Color.Yellow;
            _lineColor3 = Color.Magenta;
            _lineColor4 = Color.Green;
            _lineColor5 = Color.GreenYellow;
            //if (theme == 2)
            //{
            //    midColor = Color.OrangeRed;
            //    upColor = Color.OrangeRed;
            //    downColor = Color.OrangeRed;
            //}
            //else if (theme == 3)
            //{
            //    midColor = Color.Cyan;
            //    upColor = Color.Cyan;
            //    downColor = Color.Cyan;
            //}
        }

        protected override void Caculate()
        {
            valueDict["GMA1"].Clear();
            valueDict["GMA2"].Clear();
            valueDict["GMA3"].Clear();
            valueDict["GMA4"].Clear();
            valueDict["GMA5"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["GMA1"].Clear();
                graphDict["GMA2"].Clear();
                graphDict["GMA3"].Clear();
                graphDict["GMA4"].Clear();
                graphDict["GMA5"].Clear();
            }
            averageS1.SetParameters(_n1);
            averageS2.SetParameters(_n2);
            averageS3.SetParameters(_n3);
            averageS4.SetParameters(_n4);
            averageS5.SetParameters(_n5);
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
            double value1 = averageS1.AddValue(GetData(barDatas[i]));
            valueDict["GMA1"].Add(value1);

            double value2 = averageS2.AddValue(GetData(barDatas[i]));
            valueDict["GMA2"].Add(value2);

            double value3 = averageS3.AddValue(GetData(barDatas[i]));
            valueDict["GMA3"].Add(value3);

            double value4 = averageS4.AddValue(GetData(barDatas[i]));
            valueDict["GMA4"].Add(value4);

            double value5 = averageS5.AddValue(GetData(barDatas[i]));
            valueDict["GMA5"].Add(value5);
            if (!IsSimpleMode)
            {
                graphDict["GMA1"].AddValue(value1, _lineColor1);
                graphDict["GMA2"].AddValue(value2, _lineColor2);
                graphDict["GMA3"].AddValue(value3, _lineColor3);
                graphDict["GMA4"].AddValue(value4, _lineColor4);
                graphDict["GMA5"].AddValue(value5, _lineColor5);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["GMA1"].RemoveAt(Count - 1);
            valueDict["GMA2"].RemoveAt(Count - 1);
            valueDict["GMA3"].RemoveAt(Count - 1);
            valueDict["GMA4"].RemoveAt(Count - 1);
            valueDict["GMA5"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["GMA1"].RemoveLast();
                graphDict["GMA2"].RemoveLast();
                graphDict["GMA3"].RemoveLast();
                graphDict["GMA4"].RemoveLast();
                graphDict["GMA5"].RemoveLast();
            }
            averageS1.RemoveLast();
            averageS2.RemoveLast();
            averageS3.RemoveLast();
            averageS4.RemoveLast();
            averageS5.RemoveLast();
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

        public List<double> GetValues(string key)
        {
            return valueDict[key]; 
        }

        public double GetValue(string key, int index)
        {
            if (index >= 0 && index < Count)
                return valueDict[key][index];
            else
                return JPR.NaN;
        }

        public double GetLast(string key)
        {
            if (Count != 0)
                return valueDict[key][Count - 1];
            else
                return JPR.NaN;
        }


        public int N1
        {
            get { return _n1; }
        }

        public int N2
        {
            get { return _n2; }
        }

        public int N3
        {
            get { return _n3; }
        }

        public int N4
        {
            get { return _n4; }
        }

        public int N5
        {
            get { return _n5; }
        }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }

        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
    }
}
