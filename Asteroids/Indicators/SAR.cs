using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class SAR : BaseIndicator
    {
        private double _afStep;
        private double _afLimit;

        private List<double> afList;
        private List<double> parOpenList;
        private List<double> positionList;
        private List<double> hhValueList;
        private List<double> llValueList;
        private int _theme;
        Color upColor;
        Color downColor;

        public SAR(List<BarData> barDatas, double step = 0.04, double limit = 0.2, int theme = 1, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _afStep = step;
            _afLimit = limit;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _afStep, _afLimit);
            Name = string.Format("SAR{0}", paramTag);
            Description = "抛物线转向";
            valueDict.Add("SAR", new List<double>());
            graphDict.Add("SAR", new IndicatorGraph() { Name = "SAR", Tag = paramTag, LineStyle = EnumLineStyle.Circle });
            ChangeTheme(theme);
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(double afStep = 0.02, double afLimit = 0.2, int theme = 1)
        {
            if (_afStep != afStep || _afLimit != afLimit || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0},{1})", afStep, afLimit);
                _afStep = afStep;
                _afLimit = afLimit;
                Name = string.Format("SAR{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["SAR"].Tag = paramTag;
                }
                Caculate();
            }
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            upColor = Color.Red;
            downColor = Color.Cyan;
            if (theme == 2)
            {
                upColor = Color.Blue;
                downColor = Color.Blue;
            }
            else if (theme == 3)
            {
                upColor = Color.Yellow;
                downColor = Color.Yellow;
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
            if (!IsSimpleMode)
            {
                if (positionList[i] > 0)

                    graphDict["SAR"].AddValue(parOpenList[i], upColor);

                else

                    graphDict["SAR"].AddValue(parOpenList[i], downColor);
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
            if (!IsSimpleMode)
            {
                graphDict["SAR"].RemoveLast();
            }
            GenerateSAR(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateSAR(Count - 1);
           
            
        }

        public List<double> GetValues()
        {
            return valueDict["SAR"];
        }

        public double GetLast()
        {
            return valueDict["SAR"][Count - 1];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SAR"][index];
            else
                return JPR.NaN;
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

        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }
    }
}
