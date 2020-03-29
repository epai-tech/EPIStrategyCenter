using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class SART : BaseIndicator
    {
        private double _afStep;
        private double _afLimit;

        private List<double> afList;
        private List<double> parOpenList;
        private List<double> positionList;
        private List<double> hhValueList;
        private List<double> llValueList;


        public SART(List<BarData> barDatas, double step = 0.02, double limit = 0.2, bool isSimpleMode = true, bool isShowInMain = false)
            : base(barDatas)
        {
            _afStep = step;
            _afLimit = limit;
            IsSimpleMode = isSimpleMode;
            string tag = string.Format("({0},{1})", _afStep, _afLimit);
            Name = string.Format("SAR{0}", tag);
            Description = "抛物线转向";
            valueDict.Add("SAR", new List<double>());
            valueDict.Add("SARAP", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("SAR", new IndicatorGraph() { Name = "SAR", Tag = tag, LineStyle = EnumLineStyle.Circle });
                graphDict.Add("SARAP", new IndicatorGraph() { Name = "SARAP", Tag = tag, LineStyle = EnumLineStyle.Circle });
            }
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(double afStep = 0.02, double afLimit = 0.2)
        {
            if (_afStep != afStep || _afLimit != afLimit)
            {
                string tag = string.Format("({0},{1})", afStep, afLimit);
                _afStep = afStep;
                _afLimit = afLimit;
                Name = string.Format("SAR{0}", tag);
                if (!IsSimpleMode)
                {
                    graphDict["SAR"].Tag = tag;
                }
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["SAR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["SAR"].Clear();
            }
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
            double oParClose;
            //double oTransition = 0;
            double high = curData.High;
            double low = curData.Low;
            if (i == 0)
            {
                positionList = new List<double>() { 1 };
                //oTransition = 1;
                afList = new List<double>() { AfStep };

                hhValueList = new List<double>() { high };
                oParClose = low;
                llValueList = new List<double>() { oParClose };
                var parOpen = oParClose + AfStep * (high - oParClose);
                if (parOpen > low)
                {
                    parOpen = low;
                }
                parOpenList = new List<double>() { parOpen };
            }
            else
            {
                //oTransition = 0;
                if (high > hhValueList[i - 1])
                    hhValueList.Add(high);
                else
                    hhValueList.Add(hhValueList[i - 1]);

                if (low < llValueList[i - 1])
                    llValueList.Add(low);
                else
                    llValueList.Add(llValueList[i - 1]);
                if (positionList[i - 1] == 1)
                {
                    if (low <= parOpenList[i - 1])
                    {
                        positionList.Add(-1);
                        //oTransition = -1;
                        oParClose = hhValueList[i];
                        hhValueList[i] = high;
                        llValueList[i] = low;

                        afList.Add(AfStep);
                        var parOpen = oParClose + AfStep * (llValueList[i] - oParClose);
                        if (parOpen < high)
                            parOpen = high;
                        if (parOpen < preData.High)
                            parOpen = preData.High;
                        parOpenList.Add(parOpen);
                    }
                    else
                    {
                        positionList.Add(positionList[i - 1]);
                        oParClose = parOpenList[i - 1];
                        var af = AfLimit;
                        if (hhValueList[i] > hhValueList[i - 1] && afList[i - 1] < AfLimit)
                        {
                            if (afList[i - 1] + AfStep <= AfLimit)
                                af = afList[i - 1] + AfStep;
                        }
                        else
                            af = afList[i - 1];
                        afList.Add(af);
                        var parOpen = oParClose + af * (hhValueList[i] - oParClose);
                        if (parOpen > low)
                            parOpen = low;
                        if (parOpen > preData.Low)
                            parOpen = preData.Low;

                        parOpenList.Add(parOpen);
                    }
                }
                else
                {
                    if (high >= parOpenList[i - 1])
                    {
                        positionList.Add(1);
                        //oTransition = 1;
                        oParClose = llValueList[i];
                        hhValueList[i] = high;
                        llValueList[i] = low;
                        afList.Add(AfStep);
                        var parOpen = oParClose + AfStep * (hhValueList[i] - oParClose);
                        if (parOpen > low)
                            parOpen = low;
                        if (parOpen > preData.Low)
                            parOpen = preData.Low;

                        parOpenList.Add(parOpen);
                    }
                    else
                    {
                        positionList.Add(positionList[i - 1]);
                        oParClose = parOpenList[i - 1];
                        var af = AfLimit;
                        if (llValueList[i] < llValueList[i - 1] && afList[i - 1] < AfLimit)
                        {
                            if (afList[i - 1] + AfStep <= AfLimit)
                                af = afList[i - 1] + AfStep;
                        }
                        else
                            af=afList[i-1];
                        afList.Add(af);
                        var parOpen = oParClose + af * (llValueList[i] - oParClose);
                        if (parOpen < high)
                            parOpen = high;
                        if (parOpen < preData.High)
                            parOpen = preData.High;

                        parOpenList.Add(parOpen);
                    }
                }
            }
            valueDict["SAR"].Add(parOpenList[i]);
            valueDict["SARAP"].Add(afList[i]);
            if (!IsSimpleMode)
            {
                if (positionList[i] > 0)
                    graphDict["SAR"].AddValue(parOpenList[i], Color.Red);
                else
                    graphDict["SAR"].AddValue(parOpenList[i], Color.Green);
            }

            if (!IsSimpleMode)
            {
                graphDict["SARAP"].AddValue(afList[i], Color.White);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            afList.RemoveAt(afList.Count - 1);
            parOpenList.RemoveAt(parOpenList.Count - 1);
            positionList.RemoveAt(positionList.Count - 1);
            hhValueList.RemoveAt(hhValueList.Count - 1);
            llValueList.RemoveAt(llValueList.Count - 1);
            valueDict["SAR"].RemoveAt(Count - 1);
            valueDict["SARAP"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["SAR"].RemoveLast();
                graphDict["SARAP"].RemoveLast();
            }
            GenerateSAR(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateSAR(Count - 1);
            
            
        }

        public double GetSARValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SAR"][index];
            else
                return JPR.NaN;
        }

        public double GetSARAPValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SARAP"][index];
            else
                return JPR.NaN;
        } 

        public double GetLastSAR()
        {
            return parOpenList[parOpenList.Count - 1];
        }
        public double GetLastSARAP()
        {
            return afList[afList.Count - 1];
        }
        public double GetLastSAR(int i)
        {
            return valueDict["SAR"][i];
        }
        public double GetLastSARAP(int i)
        {
            return valueDict["SARAP"][i];
        }
        /// <summary>
        /// 获取持仓（0-多,1-空）
        /// </summary>
        /// <returns></returns>
        public int GetPosition(int index)
        {
            return positionList[index] > 0 ? 0 : 1;
        }

        public double AfStep
        {
            get { return _afStep; }
        }
        public double AfLimit
        {
            get { return _afLimit; }
        }


    }
}
