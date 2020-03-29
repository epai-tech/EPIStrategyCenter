using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class SAR2 : BaseIndicator
    {
        private double _ap;
        private double nowap;
        private double _mp;
        private int trend;
        private List<BarData> barlist4;
        private double highest4;
        private double lowest4;
        private double nextBullSAR;
        private double nextBearSAR;

        public SAR2(List<BarData> barDatas, double AP = 0.02, double MP = 0.2, bool isSimpleMode = true, bool isShowInMain = true)
            : base(barDatas)
        {
            _ap = AP;
            _mp = MP;
            string tag = string.Format("({0},{1})", _ap, _mp);
            Name = string.Format("SAR{0}", tag);
            IsSimpleMode = isSimpleMode;
            Description = "抛物线2";
            valueDict.Add("BullSAR", new List<double>());
            valueDict.Add("BearSAR", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("BullSAR", new IndicatorGraph() { Name = "BullSAR", Tag = tag, LineStyle = EnumLineStyle.Circle });
                graphDict.Add("BearSAR", new IndicatorGraph() { Name = "BearSAR", Tag = tag, LineStyle = EnumLineStyle.Circle });
            }
            barlist4 = new List<BarData>();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(double AP, double MP)
        {
            if (AP != _ap && MP != _mp)
            {
                string tag = string.Format("({0},{1})", AP, MP);
                Name = string.Format("SAR{0}", tag);
                if (!IsSimpleMode)
                {
                    graphDict["BullSAR"].Tag = tag;
                    graphDict["BearSAR"].Tag = tag;
                }
                _ap = AP;
                _mp = MP;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["BullSAR"].Clear();
            valueDict["BearSAR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["BullSAR"].Clear();
                graphDict["BearSAR"].Clear();
            }
            barlist4.Clear();
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateSAR(i);
                }
            }
            base.Caculate();
        }

        private void GenerateSAR(int i)
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
            if (barlist4.Count == 4)
            { barlist4.RemoveAt(0); }
            else if (barlist4.Count > 4)
            { throw new Exception("计算数量超出范围"); }
            barlist4.Add(curData);
            if (i < 3)
            {
                valueDict["BullSAR"].Add(JPR.NaN);
                valueDict["BearSAR"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["BullSAR"].AddValue(JPR.NaN, Color.Red);
                    graphDict["BearSAR"].AddValue(JPR.NaN, Color.Green);
                }
            }
            else if (i == 3)
            {
                highest4 = barlist4.Max(o => o.High);
                lowest4 = barlist4.Min(o => o.Low);
                if (curData.Close >= (highest4 - lowest4) / 2)
                {
                    trend = 1;
                    nowap = _ap;
                    valueDict["BullSAR"].Add(lowest4);
                    valueDict["BearSAR"].Add(JPR.NaN);
                    if (!IsSimpleMode)
                    {
                        graphDict["BullSAR"].AddValue(lowest4, Color.Red);
                        graphDict["BearSAR"].AddValue(JPR.NaN, Color.Green);
                    }
                    if (curData.High > preData.High)
                        nextBullSAR = lowest4 + nowap * (curData.High - lowest4);
                    else
                        nextBullSAR = lowest4;
                }
                else
                {
                    trend = -1;
                    nowap = _ap;
                    valueDict["BullSAR"].Add(JPR.NaN);
                    valueDict["BearSAR"].Add(highest4);
                    if (!IsSimpleMode)
                    {
                        graphDict["BullSAR"].AddValue(JPR.NaN, Color.Red);
                        graphDict["BearSAR"].AddValue(highest4, Color.Green);
                    }
                    if (curData.Low < preData.Low)
                        nextBearSAR = highest4 + nowap * (curData.Low - highest4);
                    else
                        nextBearSAR = highest4;
                }
            }
            else
            {
                if (trend == 1)
                {
                    if (curData.Close < nextBullSAR)
                    {
                        highest4 = barlist4.Max(o => o.High);
                        trend = -1;
                        nowap = _ap;
                        valueDict["BullSAR"].Add(nextBullSAR);
                        valueDict["BearSAR"].Add(highest4);
                        if (!IsSimpleMode)
                        {
                            graphDict["BullSAR"].AddValue(nextBullSAR, Color.Red);
                            graphDict["BearSAR"].AddValue(highest4, Color.Green);
                        }
                        if (curData.Low < preData.Low)
                            nextBearSAR = highest4 + nowap * (curData.Low - highest4);
                        else
                            nextBearSAR = highest4;
                    }
                    else if (curData.High > preData.High)
                    {
                        nowap = nowap < _mp ? +_ap : _mp;
                        valueDict["BullSAR"].Add(nextBullSAR);
                        valueDict["BearSAR"].Add(JPR.NaN);
                        if (!IsSimpleMode)
                        {
                            graphDict["BullSAR"].AddValue(nextBullSAR, Color.Red);
                            graphDict["BearSAR"].AddValue(JPR.NaN, Color.Green);
                        }
                        nextBullSAR = (1 - nowap) * nextBullSAR + nowap * curData.High;
                    }
                    else
                    {
                        valueDict["BullSAR"].Add(nextBullSAR);
                        valueDict["BearSAR"].Add(JPR.NaN);
                        if (!IsSimpleMode)
                        {
                            graphDict["BullSAR"].AddValue(nextBullSAR, Color.Red);
                            graphDict["BearSAR"].AddValue(JPR.NaN, Color.Green);
                        }
                    }
                }
                else
                {
                    if (curData.Close > nextBearSAR)
                    {
                        lowest4 = barlist4.Min(o => o.Low);
                        trend = 1;
                        nowap = _ap;
                        valueDict["BullSAR"].Add(lowest4);
                        valueDict["BearSAR"].Add(nextBearSAR);
                        if (!IsSimpleMode)
                        {
                            graphDict["BullSAR"].AddValue(lowest4, Color.Red);
                            graphDict["BearSAR"].AddValue(nextBearSAR, Color.Green);
                        }
                        if (curData.High > preData.High)
                            nextBullSAR = lowest4 + nowap * (curData.High - lowest4);
                        else
                            nextBullSAR = lowest4;
                    }
                    else if (curData.Low < preData.Low)
                    {
                        nowap = nowap < _mp ? +_ap : _mp;
                        valueDict["BullSAR"].Add(JPR.NaN);
                        valueDict["BearSAR"].Add(nextBearSAR);
                        if (!IsSimpleMode)
                        {
                            graphDict["BullSAR"].AddValue(JPR.NaN, Color.Red);
                            graphDict["BearSAR"].AddValue(nextBearSAR, Color.Green);
                        }
                        nextBearSAR = (1 - nowap) * nextBearSAR + nowap * curData.Low;
                    }
                    else
                    {
                        valueDict["BullSAR"].Add(JPR.NaN);
                        valueDict["BearSAR"].Add(nextBearSAR);
                        if (!IsSimpleMode)
                        {
                            graphDict["BullSAR"].AddValue(JPR.NaN, Color.Red);
                            graphDict["BearSAR"].AddValue(nextBearSAR, Color.Green);
                        }
                    }
                }
            }
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateSAR(Count - 1);
        }

        public List<double> GetBullSARValues()
        {
            return valueDict["BullSAR"];
        }
        public List<double> GetBearSARValues()
        {
            return valueDict["BearSAR"];
        }

        public double GetLastBullSAR()
        {
            if (Count != 0)
                return valueDict["BullSAR"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastBearSAR()
        {
            if (Count != 0)
                return valueDict["BearSAR"][Count - 1];
            else
                return JPR.NaN;
        }

        public double AP
        {
            get { return _ap; }
        }
        public double MP
        {
            get { return _mp; }
        }
    }
}
