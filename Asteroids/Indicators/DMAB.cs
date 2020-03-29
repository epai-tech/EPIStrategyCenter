using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
using EPI.Common;

namespace EPI.Asteroids.Indicators
{
    public class DMAB : BaseIndicator
    {
        private int _length;
        private double _validRange;
        private int _isFullDay;

        private int _theme;
        //private AverageS longAvgS;
        //private AverageS shortAvgS;
        private List<double> longAvgList;
        private List<double> shortAvgList;
        private AverageS totalLongAvgS;
        private AverageS totalShortAvgS;
        private List<double> dayAvgList;
        private double volumemultiple;
        private int cycleMinute;
        private bool isStartCalcAvg;
        private double dayAvgPrice;//计算日内均价
        private double dayAvgVolume;//计算日内均价
        private double dayMaxPrice;//大于均线上的最大值
        private double dayMinPrice;//小于均线下的最小值
        private double tmpAvgPrice;
        private double tmpAvgVolume;
        private bool isLongBack;
        private bool isShortBack;
        private double preLongAvgVal;
        private double preShortAvgVal;
        //private double preDayAvgPrice;

        private double preDayLongVal;
        private double preDayShortVal;

        private Color upColor;
        private Color downColor;
        private Color midColor;


        public DMAB(List<BarData> barDatas, int length = 3, double validRange = 0.2, int isFullDay = 0, int theme = 1, bool isSimpleMode = true,
            bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _validRange = validRange;
            _isFullDay = isFullDay;
            ChangeTheme(theme);
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1},{2})", _length, _validRange, _isFullDay);
            Name = string.Format("DMAB{0}", tag);
            Description = "日均线回归";
            valueDict.Add("LV", new List<double>());
            valueDict.Add("SV", new List<double>());
            valueDict.Add("DV", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("LV", new IndicatorGraph() { Name = "LV", Tag = paramTag, LineStyle = EnumLineStyle.DashLine });
                graphDict.Add("SV", new IndicatorGraph() { Name = "SV", Tag = paramTag, LineStyle = EnumLineStyle.DashLine });
                graphDict.Add("DV", new IndicatorGraph() { Name = "DV", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            //longAvgS = new AverageS();
            //shortAvgS = new AverageS();
            longAvgList = new List<double>();
            shortAvgList = new List<double>();
            totalLongAvgS = new AverageS();
            totalShortAvgS = new AverageS();
            dayAvgList = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length, double validRange, int isFullDay, int theme = 1)
        {
            if (length != _length || validRange != _validRange || isFullDay != _isFullDay || theme != _theme)
            {
                ChangeTheme(theme);
                _validRange = validRange;
                _isFullDay = isFullDay;
                _length = length;
                string paramTag = string.Format("({0},{1},{2})", _length, _validRange, _isFullDay);
                Name = string.Format("DMAB{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["LV"].Tag = paramTag;
                    graphDict["SV"].Tag = paramTag;
                    graphDict["DV"].Tag = paramTag;
                }
                preLongAvgVal = 0;
                preShortAvgVal = 0;
                preDayLongVal = 0;
                preDayShortVal = 0;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            ResetValue();
            valueDict["LV"].Clear();
            valueDict["SV"].Clear();
            valueDict["DV"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["LV"].Clear();
                graphDict["SV"].Clear();
                graphDict["DV"].Clear();
            }
            //longAvgS.SetParameters(_length);
            //shortAvgS.SetParameters(_length);
            totalLongAvgS.SetParameters(_length);
            totalShortAvgS.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateDMAB(i);
                }
            }
            base.Caculate();
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            upColor = Color.Pink;
            downColor = Color.SeaGreen;
            midColor = Color.Yellow;
            if (theme == 2)
            {
                upColor = Color.LightSkyBlue;
                downColor = Color.Lime;
                midColor = Color.Orange;
            }
            else if (theme == 3)
            {
                upColor = Color.DeepPink;
                downColor = Color.Purple;
                midColor = Color.WhiteSmoke;
            }
        }

        private void GenerateDMAB(int i)
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
            if (i == 0)
            {
                volumemultiple = curData.Amount / curData.Volume / curData.Close;
                var cycle = curData.Cycle.Substring(curData.Cycle.Length - 1, 1);
                var number = int.Parse(curData.Cycle.Substring(0, curData.Cycle.Length - 1));
                switch (cycle)
                {
                    case "M": cycleMinute = number; break;
                    case "H": cycleMinute = number * 60; break;
                }
            }
            else
            {
                if (curData.TradingDay != preData.TradingDay)
                {
                    ResetValue();
                    preDayLongVal = totalLongAvgS.AddValue(preLongAvgVal);
                    preDayShortVal = totalShortAvgS.AddValue(preShortAvgVal);
                }
            }
            var contractType = StringHelper.RemoveNumber(curData.Contract);
            if (_isFullDay==1)
                isStartCalcAvg = true;
            else
            {
                if (contractType == "T" || contractType == "TF")
                {
                    if (curData.RealDateTime.AddMinutes(-cycleMinute).TimeOfDay == TimeSpan.Parse("14:15"))
                        //开始计算日均线
                        isStartCalcAvg = true;
                }
                else
                {
                    if (curData.RealDateTime.AddMinutes(-cycleMinute).TimeOfDay == TimeSpan.Parse("09:00"))
                        //开始计算日均线
                        isStartCalcAvg = true;
                }
            }

            if (isStartCalcAvg)
            {
                dayAvgPrice = (dayAvgPrice * dayAvgVolume + curData.Close * curData.Volume) / (dayAvgVolume + curData.Volume);
                dayAvgVolume += curData.Volume;
                tmpAvgPrice = curData.Close;
                tmpAvgVolume = curData.Volume;
                dayAvgList.Add(dayAvgPrice);
                //preDayAvgPrice = dayAvgPrice;
                valueDict["DV"].Add(dayAvgPrice);
                if (!IsSimpleMode)
                {
                    graphDict["DV"].AddValue(dayAvgPrice, midColor);
                }
            }
            else
            {
                dayAvgList.Add(JPR.NaN);
                valueDict["DV"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["DV"].AddValue(JPR.NaN, midColor);
                }
            }

            double longValue = 0;
            double shortValue = 0;

            var lastDayAvgValue = dayAvgList[dayAvgList.Count - 1];

            if (i > 2 && !JPR.IsNaN(lastDayAvgValue) && dayAvgList.Count>1 && !JPR.IsNaN(dayAvgList[dayAvgList.Count - 2]))
            {
                //处于日均线上方
                if (curData.Close > lastDayAvgValue)
                {
                    if (dayMaxPrice < curData.Close)
                    {
                        dayMaxPrice = curData.Close;
                        isLongBack = false;//突破前高重新统计回归
                    }
                    //从高点回落
                    double longRate = (dayMaxPrice - lastDayAvgValue) / dayMaxPrice;
                    if (longRate * 100 >= _validRange && !isLongBack)
                    {
                        longValue = longRate;
                        isLongBack = true;
                    }
                }
                //处于日均线下方
                if (curData.Close < lastDayAvgValue)
                {
                    if (dayMinPrice > curData.Close)
                    {
                        dayMinPrice = curData.Close;
                        isShortBack = false;
                    }
                    //从低点回归
                    double shortRate = (lastDayAvgValue - dayMinPrice) / dayMinPrice;
                    if (shortRate * 100 >= _validRange && !isShortBack)
                    {
                        shortValue = shortRate;
                        isShortBack = true;
                    }
                }
            }
            if (i > 1)
            {
                if (longValue > 0)
                {
                    longAvgList.Add(longValue);
                    double longAvgValue = longAvgList.Average(); //longAvgS.AddValue(longValue);
                    if (longAvgValue > 0 && !JPR.IsNaN(lastDayAvgValue))
                        preLongAvgVal = longAvgValue;
                }
                if (shortValue > 0)
                {
                    shortAvgList.Add(shortValue);
                    double shortAvgValue = shortAvgList.Average(); //shortAvgS.AddValue(shortValue);
                    if (shortAvgValue > 0 && !JPR.IsNaN(lastDayAvgValue))
                    {
                        preShortAvgVal = shortAvgValue;
                    }
                }
            }
            if (i > 0)
            {
                double realLongValue = 0;
                double realShortValue = 0;
                if (lastDayAvgValue != 0 && !JPR.IsNaN(lastDayAvgValue))
                {
                    if (preDayLongVal != 0 && !JPR.IsNaN(preDayLongVal))
                    {
                        realLongValue = lastDayAvgValue * (1 + preDayLongVal);
                        if (realLongValue != 0)
                        {
                            valueDict["LV"].Add(realLongValue);
                            if (!IsSimpleMode)
                            {
                                graphDict["LV"].AddValue(realLongValue, upColor);
                            }
                        }
                    }
                    if (preDayShortVal != 0 && !JPR.IsNaN(preDayLongVal))
                    {
                        realShortValue = lastDayAvgValue * (1 - preDayShortVal);
                        if (realShortValue != 0)
                        {
                            valueDict["SV"].Add(realShortValue);
                            if (!IsSimpleMode)
                            {
                                graphDict["SV"].AddValue(realShortValue, downColor);
                            }
                        }
                    }
                }
                if (realLongValue == 0)
                {
                    valueDict["LV"].Add(valueDict["LV"][valueDict["LV"].Count - 1]);
                    if (!IsSimpleMode)
                    {
                        graphDict["LV"].AddValue(valueDict["LV"][valueDict["LV"].Count - 1], upColor);
                    }
                }
                if (realShortValue == 0)
                {
                    valueDict["SV"].Add(valueDict["SV"][valueDict["SV"].Count - 1]);
                    if (!IsSimpleMode)
                    {
                        graphDict["SV"].AddValue(valueDict["SV"][valueDict["SV"].Count - 1], downColor);
                    }
                }
            }
            else
            {
                valueDict["LV"].Add(JPR.NaN);
                valueDict["SV"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["LV"].AddValue(JPR.NaN, upColor);
                    graphDict["SV"].AddValue(JPR.NaN, downColor);
                }
            }
        }

        private void ResetValue()
        {
            //longAvgS.Clear();
            //shortAvgS.Clear();
            longAvgList.Clear();
            shortAvgList.Clear();
            isLongBack = false;
            isShortBack = false;
            isStartCalcAvg = false;
            dayAvgList.Clear();
            dayAvgPrice = 0;
            dayAvgVolume = 0;
            dayMaxPrice = double.MinValue;
            dayMinPrice = double.MaxValue;
            tmpAvgPrice=0;
            tmpAvgVolume=0;
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["LV"].RemoveAt(Count - 1);
            valueDict["SV"].RemoveAt(Count - 1);
            valueDict["DV"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["LV"].RemoveLast();
                graphDict["SV"].RemoveLast();
                graphDict["DV"].RemoveLast();
            }
            dayAvgPrice = (dayAvgPrice * dayAvgVolume  -tmpAvgPrice * tmpAvgVolume) / (dayAvgVolume - tmpAvgVolume);
            dayAvgVolume -= tmpAvgVolume;

            GenerateDMAB(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateDMAB(Count - 1);
            
            
        }

        public double GetLVValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["LV"][index];
            else
                return JPR.NaN;
        }
        public double GetSVValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SV"][index];
            else
                return JPR.NaN;
        }
        public double GetDVValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["DV"][index];
            else
                return JPR.NaN;
        }

        public List<double> GetLVValues()
        {
            return valueDict["LV"];
        }
        public List<double> GetDVValues()
        {
            return valueDict["DV"];
        }
        public List<double> GetSVValues()
        {
            return valueDict["SV"];
        }

        public double GetLastLV()
        {
            if (Count != 0)
                return valueDict["LV"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastSV()
        {
            if (Count != 0)
                return valueDict["SV"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastDV()
        {
            if (Count != 0)
                return valueDict["DV"][Count - 1];
            else
                return JPR.NaN;
        }


        public int Length
        {
            get { return _length; }
        }

        public double ValidRange { get { return _validRange; } }

        public int IsFullDay { get { return _isFullDay; } }

        public int Theme { get { return _theme; } }
    }
}
