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
    public class RJX : BaseIndicator
    {
        private int _length;
        private Color _dtLineColor = Color.Red;
        private Color _fdLineColor = Color.Yellow;

        private double dtAvgPrice;
        private double fdAvgPrice;
        private double dtAvgVolume;
        private double fdAvgVolume;

        private double dtTmpAvgPrice;
        private double dtTmpAvgVol;

        private double fdTmpAvgPrice;
        private double fdTmpAvgVol;

        private List<double> fdAvgList;
        private List<double> dtAvgList;
        private double volumemultiple;
        private int cycleMinute;
        private bool isStartCalcDtAvg;

        public RJX(List<BarData> barDatas, Color dtLineColor, Color fdLineColor, int length = 3, bool isSimpleMode = true,
            bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            IsSimpleMode = isSimpleMode;
            _length = length;
            _dtLineColor = dtLineColor;
            _fdLineColor = fdLineColor;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("RJX{0}", tag);
            Description = "日间均线";
            valueDict.Add("FD", new List<double>());
            valueDict.Add("DT", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("FD", new IndicatorGraph() { Name = "FD", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("DT", new IndicatorGraph() { Name = "DT", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            //longAvgS = new AverageS();
            //shortAvgS = new AverageS();
            fdAvgList = new List<double>();
            dtAvgList = new List<double>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(Color dtLineColor, Color fdLineColor, int length = 3)
        {
            if (length != _length || dtLineColor != _dtLineColor || fdLineColor != _fdLineColor)
            {
                _dtLineColor = dtLineColor;
                _fdLineColor = fdLineColor;
                _length = length;
                string paramTag = string.Format("({0})", _length);
                Name = string.Format("RJX{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["FD"].Tag = paramTag;
                    graphDict["DT"].Tag = paramTag;
                }
                Caculate();
            }
        }

        protected override void Caculate()
        {
            ResetValue();
            valueDict["FD"].Clear();
            valueDict["DT"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["FD"].Clear();
                graphDict["DT"].Clear();
            }

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateRJX(i);
                }
            }
            base.Caculate();
        }

        private void GenerateRJX(int i)
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
            if (!JPR.IsNaN(curData.Close))
            {
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
                    }
                }

                if (!isStartCalcDtAvg)
                {
                    var contractType = StringHelper.RemoveNumber(curData.Contract);
                    if (contractType == "T" || contractType == "TF")
                    {
                        if (curData.RealDateTime.AddMinutes(-cycleMinute).TimeOfDay == TimeSpan.Parse("14:15"))
                            //开始计算日均线
                            isStartCalcDtAvg = true;
                    }
                    else if (contractType == "IF" || contractType == "IH" || contractType == "IC")
                    {
                        if (curData.RealDateTime.AddMinutes(-cycleMinute).TimeOfDay == TimeSpan.Parse("14:00"))
                            //开始计算日均线
                            isStartCalcDtAvg = true;
                    }
                    else
                    {
                        if (curData.RealDateTime.AddMinutes(-cycleMinute).TimeOfDay == TimeSpan.Parse("09:00"))
                            //开始计算日均线
                            isStartCalcDtAvg = true;
                    }
                }
                if (isStartCalcDtAvg)
                {
                    dtAvgPrice = (dtAvgPrice * dtAvgVolume + curData.Close * curData.Volume) / (dtAvgVolume + curData.Volume);
                    dtAvgVolume += curData.Volume;
                    dtAvgList.Add(dtAvgPrice);
                    dtTmpAvgPrice = curData.Close;
                    dtTmpAvgVol = curData.Volume;
                    valueDict["DT"].Add(dtAvgPrice);
                    if (!IsSimpleMode)
                    {
                        graphDict["DT"].AddValue(dtAvgPrice, _dtLineColor);
                    }
                }
                else
                {
                    dtAvgList.Add(JPR.NaN);
                    valueDict["DT"].Add(JPR.NaN);
                    if (!IsSimpleMode)
                    {
                        graphDict["DT"].AddValue(JPR.NaN, _dtLineColor);
                    }
                }
                fdAvgPrice = (fdAvgPrice * fdAvgVolume + curData.Close * curData.Volume) / (fdAvgVolume + curData.Volume);
                fdAvgVolume += curData.Volume;
                fdAvgList.Add(fdAvgPrice);
                fdTmpAvgPrice = curData.Close;
                fdTmpAvgVol = curData.Volume;
                valueDict["FD"].Add(fdAvgPrice);
                if (!IsSimpleMode)
                {
                    graphDict["FD"].AddValue(fdAvgPrice, _fdLineColor);
                }
            }
            else
            {
                valueDict["FD"].Add(JPR.NaN);
                valueDict["DT"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["FD"].AddValue(JPR.NaN, _fdLineColor);
                    graphDict["DT"].AddValue(JPR.NaN, _dtLineColor);
                }
            }

        }

        private void ResetValue()
        {
            isStartCalcDtAvg = false;
            dtAvgPrice = 0;
            dtAvgVolume = 0;
            dtTmpAvgPrice = 0;
            dtTmpAvgVol = 0;
            fdAvgPrice = 0;
            fdAvgVolume = 0;
            fdTmpAvgPrice = 0;
            fdTmpAvgVol = 0;
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["FD"].RemoveAt(Count - 1);
            valueDict["DT"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["FD"].RemoveLast();
                graphDict["DT"].RemoveLast();
            }
            if (dtAvgVolume - dtTmpAvgVol > 0)
            {
                dtAvgPrice = (dtAvgPrice * dtAvgVolume - dtTmpAvgPrice * dtTmpAvgVol) / (dtAvgVolume - dtTmpAvgVol);
                dtAvgVolume -= dtTmpAvgVol;
            }

            fdAvgPrice = (fdAvgPrice * fdAvgVolume - fdTmpAvgPrice * fdTmpAvgVol) / (fdAvgVolume - fdTmpAvgVol);
            fdAvgVolume -= fdTmpAvgVol;

            GenerateRJX(Count - 1);
           
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateRJX(Count - 1);
            
            
        }

        public List<double> GetFDValues()
        {
            return valueDict["FD"];
        }
        public List<double> GetDTValues()
        {
            return valueDict["DT"];
        }

        public double GetFDValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["FD"][index];
            else
                return JPR.NaN;
        }
        public double GetDTValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["DT"][index];
            else
                return JPR.NaN;
        }

        public double GetLastFD()
        {
            if (Count != 0)
                return valueDict["FD"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastDT()
        {
            if (Count != 0)
                return valueDict["DT"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

        public Color FDLineColor { get { return _fdLineColor; } }

        public Color DTLineColor { get { return _dtLineColor; } }
    }
}
