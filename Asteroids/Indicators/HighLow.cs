using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class HighLow:BaseIndicator
    {
        private int _length;
        private double highestValue;
        private double lowestValue;
        private MaxValue highest;
        private MinValue lowest;
        private int _theme;
        Color upColor;
        Color downColor;

        public HighLow(List<BarData> barDatas, int length = 20, int theme = 1, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("HighLow{0}", paramTag);
            Description = "最高最低价";
            valueDict.Add("Highest", new List<double>());
            valueDict.Add("Lowest",new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("Highest", new IndicatorGraph() { Name = "Highest", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("Lowest", new IndicatorGraph() { Name = "Lowest", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            ChangeTheme(theme);
            highest = new MaxValue();
            lowest = new MinValue();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length, int theme = 1)
        {
            if (length != _length || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0})", length);
                Name = string.Format("HighLow{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["Highest"].Tag = paramTag;
                    graphDict["Lowest"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            upColor = Color.Red;
            downColor = Color.Green;
            if (theme == 2)
            {
                upColor = Color.OrangeRed;
                downColor = Color.OrangeRed;
            }
            else if (theme == 3)
            {
                upColor = Color.Cyan;
                downColor = Color.Cyan;
            }
        }

        protected override void Caculate()
        {
            valueDict["Highest"].Clear();
            valueDict["Lowest"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["Highest"].Clear();
                graphDict["Lowest"].Clear();
            }
            highest.SetParameters(_length);
            lowest.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateHighLow(i);
                }
            }
            base.Caculate();
        }

        private void GenerateHighLow(int i)
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
            highestValue = highest.AddValue(curData.High);
            lowestValue = lowest.AddValue(curData.Low);
            valueDict["Highest"].Add(highestValue);
            valueDict["Lowest"].Add(lowestValue);
            if (!IsSimpleMode)
            {
                graphDict["Highest"].AddValue(highestValue, upColor);
                graphDict["Lowest"].AddValue(lowestValue, downColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["Highest"].RemoveAt(Count - 1);
            valueDict["Lowest"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["Highest"].RemoveLast();
                graphDict["Lowest"].RemoveLast();
            }
            highest.RemoveLast();
            lowest.RemoveLast();
            GenerateHighLow(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateHighLow(Count - 1);
            
            
        }

        public List<double> GetHighestValues()
        { 
            return valueDict["Highest"];
        }
        public List<double> GetLowestValues()
        {
            return valueDict["Lowest"];
        }

        public double GetHighestValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Highest"][index];
            else
                return JPR.NaN;
        }

        public double GetLowestValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Lowest"][index];
            else
                return JPR.NaN;
        }

        public double GetLastHighest()
        {
            if (Count != 0)
                return valueDict["Highest"][Count - 1];
            else
                return JPR.NaN;
        }

        //public double GetHighestValue(DateTime dateTime)
        //{
        //    var index = GetIndex(dateTime);
        //    if (valueDict["Highest"].Count > index)
        //        return valueDict["Highest"][index];
        //    else
        //        return JPR.NaN;
        //}

        public double GetLastLowest()
        {
            if (Count != 0)
                return valueDict["Lowest"][Count - 1];
            else
                return JPR.NaN;
        }

        //public double GetLowestValue(DateTime dateTime)
        //{
        //    var index = GetIndex(dateTime);
        //    if (valueDict["Lowest"].Count > index)
        //        return valueDict["Lowest"][index];
        //    else
        //        return JPR.NaN;
        //}

        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }
    }
}
