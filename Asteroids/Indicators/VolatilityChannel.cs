using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Common;

namespace EPI.Asteroids.Indicators
{
    public class VolatilityChannel:BaseIndicator
    {
        private int _length;
        private int _predictedLength;
        private List<BarData> barList;
        private List<double> list;
        private int _theme;
        Color upColor;
        Color downColor;
        
        
        public VolatilityChannel(List<BarData> barDatas, int length = 20, int predictedLength = 5,int theme = 1,
             bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _predictedLength = predictedLength;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _length, _predictedLength);
            Name = string.Format("VolatilityChannel{0}", paramTag);
            Description = "波动率通道";
            ChangeTheme(theme);
            valueDict.Add("Up", new List<double>());
            valueDict.Add("Down", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("Up", new IndicatorGraph() { Name = "Up", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("Down", new IndicatorGraph() { Name = "Down", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            barList = new List<BarData>();
            list = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            upColor = Color.Magenta;
            downColor = Color.SpringGreen;
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

        public void SetParameters(int length, int predictedLength, int theme)
        {
            if (length != _length || predictedLength != _predictedLength || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0},{1})", length, predictedLength);
                Name = string.Format("VolatilityChannel{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["Up"].Tag = paramTag;
                    graphDict["Down"].Tag = paramTag;
                }
                _length = length;
                _predictedLength = predictedLength;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["Up"].Clear();
            valueDict["Down"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["Up"].Clear();
                graphDict["Down"].Clear();
            }
            barList.Clear();
            list.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateVolatilityChannel(i);
                }
            }
            base.Caculate();
        }

        private void GenerateVolatilityChannel(int i)
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
            if (barList.Count == 2)
            { barList.RemoveAt(0); }
            barList.Add(curData);
            if (list.Count == _length)
            { list.RemoveAt(0); }
            if (barList.Count == 1)
            {
                if (barList[0].Open != 0)
                    list.Add(Math.Log(barList[0].Close / barList[0].Open));
                else
                    list.Add(0);
            }
            else
            {
                if (barList[0].Close != 0)
                    list.Add(Math.Log(barList[1].Close / barList[0].Close));
                else
                    list.Add(0);
            }
            if (i < _length - 1)
            {
                valueDict["Up"].Add(JPR.NaN);
                valueDict["Down"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["Up"].AddValue(JPR.NaN, upColor);
                    graphDict["Down"].AddValue(JPR.NaN, downColor);
                }
            }
            else
            {
                double stv = MathHelper.Stdev(list) * Math.Sqrt(_predictedLength);
                double upValue = barList[1].Close * (1 + stv);
                double downValue = barList[1].Close * (1 - stv);
                valueDict["Up"].Add(upValue);
                valueDict["Down"].Add(downValue);
                if (!IsSimpleMode)
                {
                    graphDict["Up"].AddValue(upValue, upColor);
                    graphDict["Down"].AddValue(downValue, downColor);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["Up"].RemoveAt(Count - 1);
            valueDict["Down"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["Up"].RemoveLast();
                graphDict["Down"].RemoveLast();
            }
            list.RemoveAt(list.Count - 1);
            barList.RemoveAt(1);
            GenerateVolatilityChannel(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateVolatilityChannel(Count - 1);
            
            
        }

        public List<double> GetUpValues()
        {
            return valueDict["Up"];
        }
        public List<double> GetDownValues()
        {
            return valueDict["Down"];
        }

        public double GetUpValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Up"][index];
            else
                return JPR.NaN;
        }

        public double GetDownValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Down"][index];
            else
                return JPR.NaN;
        }

        public double GetLastUp()
        {
            if (Count != 0)
                return valueDict["Up"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastDown()
        {
            if (Count != 0)
                return valueDict["Down"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

        public int PredictedLength
        {
            get { return _predictedLength; }
        }

        public int Theme
        {
            get { return _theme; }
        }
    }
}
