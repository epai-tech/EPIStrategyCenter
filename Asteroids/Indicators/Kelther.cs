using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class Kelther:BaseIndicator
    {
        public enum EnumKeltherLine
        { 
            Mid=0,
            Up=1,
            Down=2
        }

        private int _length;
        private double _offset;
        private AverageS maClose;
        private TrueRange tr;
        private AverageS atr;
        private int _theme;
        Color midColor;
        Color upColor;
        Color downColor;

        public Kelther(List<BarData> bars, int length = 20, double offset = 1.5, int theme = 1, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            :base(bars)
        {
            Tag = tag;
            _length = length;
            _offset = offset;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _length, _offset);
            Name = string.Format("Kelther{0}", paramTag);
            Description = "肯特纳通道";
            valueDict.Add(EnumKeltherLine.Mid.ToString(), new List<double>());
            valueDict.Add(EnumKeltherLine.Up.ToString(), new List<double>());
            valueDict.Add(EnumKeltherLine.Down.ToString(), new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add(EnumKeltherLine.Mid.ToString(), new IndicatorGraph() { Name = EnumKeltherLine.Mid.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumKeltherLine.Up.ToString(), new IndicatorGraph() { Name = EnumKeltherLine.Up.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add(EnumKeltherLine.Down.ToString(), new IndicatorGraph() { Name = EnumKeltherLine.Down.ToString(), Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            ChangeTheme(theme);
            maClose=new AverageS();
            tr=new TrueRange();
            atr=new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length, double offset, int theme)
        {
            if (length != _length || offset != _offset || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0},{1})", length, offset);
                Name = string.Format("Kelther{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict[EnumKeltherLine.Mid.ToString()].Tag = paramTag;
                    graphDict[EnumKeltherLine.Up.ToString()].Tag = paramTag;
                    graphDict[EnumKeltherLine.Down.ToString()].Tag = paramTag;
                }
                _length = length;
                _offset = offset;
                Caculate();
            }
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            midColor = Color.Yellow;
            upColor = Color.Magenta;
            downColor = Color.SpringGreen;
            if (theme == 2)
            {
                midColor = Color.OrangeRed;
                upColor = Color.OrangeRed;
                downColor = Color.OrangeRed;
            }
            else if (theme == 3)
            {
                midColor = Color.Cyan;
                upColor = Color.Cyan;
                downColor = Color.Cyan;
            }
        }

        protected override void Caculate()
        {
            valueDict[EnumKeltherLine.Mid.ToString()].Clear();
            valueDict[EnumKeltherLine.Up.ToString()].Clear();
            valueDict[EnumKeltherLine.Down.ToString()].Clear();
            if (!IsSimpleMode)
            {
                graphDict[EnumKeltherLine.Mid.ToString()].Clear();
                graphDict[EnumKeltherLine.Up.ToString()].Clear();
                graphDict[EnumKeltherLine.Down.ToString()].Clear();
            }
            maClose.SetParameters(_length);
            atr.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateKelther(i);
                }
            }
            base.Caculate();
        }

        private void GenerateKelther(int i)
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
            double trValue;
            double atrValue;
            double UpValue;
            double DownValue;
            double MidValue = maClose.AddValue(curData.Close);
            if (i == 0)
            { trValue = (curData.High - curData.Low); }
            else
            { trValue = tr.Caculate(preData.Close, curData.High, curData.Low); }
            atrValue = atr.AddValue(trValue);
            if (MidValue != JPR.NaN)
            {
                UpValue = MidValue + _offset * atrValue;
                DownValue = MidValue - _offset * atrValue;
            }
            else
            { 
                UpValue = JPR.NaN; DownValue = JPR.NaN; 
            }
            valueDict[EnumKeltherLine.Mid.ToString()].Add(MidValue);
            valueDict[EnumKeltherLine.Up.ToString()].Add(UpValue);
            valueDict[EnumKeltherLine.Down.ToString()].Add(DownValue);
            if (!IsSimpleMode)
            {
                graphDict[EnumKeltherLine.Mid.ToString()].AddValue(MidValue, midColor);
                graphDict[EnumKeltherLine.Up.ToString()].AddValue(UpValue, upColor);
                graphDict[EnumKeltherLine.Down.ToString()].AddValue(DownValue, downColor);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict[EnumKeltherLine.Mid.ToString()].RemoveAt(Count - 1);
            valueDict[EnumKeltherLine.Up.ToString()].RemoveAt(Count - 1);
            valueDict[EnumKeltherLine.Down.ToString()].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict[EnumKeltherLine.Mid.ToString()].RemoveLast();
                graphDict[EnumKeltherLine.Up.ToString()].RemoveLast();
                graphDict[EnumKeltherLine.Down.ToString()].RemoveLast();
            }
            maClose.RemoveLast();
            atr.RemoveLast();
            GenerateKelther(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateKelther(Count - 1);
            
            
        }

        public List<double> GetUpValues()
        {
            return valueDict[EnumKeltherLine.Up.ToString()];
        }

        public List<double> GetDownValues()
        {
            return valueDict[EnumKeltherLine.Down.ToString()];
        }

        public List<double> GetMidValues()
        {
            return valueDict[EnumKeltherLine.Mid.ToString()];
        }

        public double GetUpValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict[EnumKeltherLine.Up.ToString()][index];
            else
                return JPR.NaN;
        }

        public double GetMidValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict[EnumKeltherLine.Mid.ToString()][index];
            else
                return JPR.NaN;
        }

        public double GetDownValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict[EnumKeltherLine.Down.ToString()][index];
            else
                return JPR.NaN;
        }

        public double GetLast(EnumKeltherLine kl)
        {
            if (Count != 0)
                return valueDict[kl.ToString()][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

        public double Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }
    }
}
