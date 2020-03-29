using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class BSM : BaseIndicator
    {
        private double _step;
        private double _limit;

        private List<double> afList;
        private List<double> parOpenList;
        private List<double> positionList;
        private List<double> hhValueList;
        private List<double> llValueList;

        Color v1Color;
        Color v2Color;
        Color v3Color;
        Color v4Color;

        private int _length;
        List<double> valList;
        List<double> midList;
        List<double> sarList;


        public BSM(List<BarData> barDatas, int length = 26, double step = 0.04, double limit = 0.2, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            _step = step;
            _limit = limit;
            IsSimpleMode = isSimpleMode;
            if (_length == 0) _length = 26;
            string paramTag = string.Format("({0},{1},{2})",_length, _step, _limit);
            Name = string.Format("BSM{0}", paramTag);
            Description = "BSM";
            valueDict.Add("V1", new List<double>());
            valueDict.Add("V2", new List<double>());
            valueDict.Add("V3", new List<double>());
            valueDict.Add("V4", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("V1", new IndicatorGraph() { Name = "V1", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("V2", new IndicatorGraph() { Name = "V2", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("V3", new IndicatorGraph() { Name = "V3", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("V4", new IndicatorGraph() { Name = "V4", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            v1Color = Color.Red;
            v2Color = Color.Blue;
            v3Color = Color.Green;
            v4Color = Color.Yellow;
            IsShowInMain = isShowInMain;
            valList = new List<double>();
            midList = new List<double>();
            sarList = new List<double>();
            Caculate();
        }

        public void SetParameters(int length = 26, double step = 0.02, double limit = 0.2)
        {
            if (_length != length || _step != step || _limit != limit)
            {
                _length = length;
                if (_length == 0) _length = 26;
                _step = step;
                _limit = limit;
                string paramTag = string.Format("({0},{1},{2})", _length, _step, _limit);
                Name = string.Format("BSM{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["V1"].Tag = paramTag;
                    graphDict["V2"].Tag = paramTag;
                    graphDict["V3"].Tag = paramTag;
                    graphDict["V4"].Tag = paramTag;
                }
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valList.Clear();
            midList.Clear();
            sarList.Clear();

            valueDict["V1"].Clear();
            valueDict["V2"].Clear();
            valueDict["V3"].Clear();
            valueDict["V4"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["V3"].Clear();
                graphDict["V2"].Clear();
                graphDict["V1"].Clear();
                graphDict["V4"].Clear();
            }
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    Generate(i);
                }
            }
            base.Caculate();
        }

        private void Generate(int i)
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
            if (valList.Count == _length)
            { valList.RemoveAt(0); }
            else if (valList.Count > _length)
            { throw new Exception("计算数量超出范围"); }
            valList.Add(curData.Close);

            double ma = valList.Average();
            midList.Add(ma);

            double oParClose;
            //double oTransition = 0;
            double high = curData.High;
            double low = curData.Low;
            if (i == 0)
            {
                positionList = new List<double>() { 1 };
                //oTransition = 1;
                afList = new List<double>() { Step };

                hhValueList = new List<double>() { high };
                oParClose = low;
                llValueList = new List<double>() { oParClose };
                var parOpen = oParClose + Step * (high - oParClose);
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

                        afList.Add(Step);
                        var parOpen = oParClose + Step * (llValueList[i] - oParClose);
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
                        var af = Limit;
                        if (hhValueList[i] > hhValueList[i - 1] && afList[i - 1] < Limit)
                        {
                            if (afList[i - 1] + Step <= Limit)
                                af = afList[i - 1] + Step;
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
                        afList.Add(Step);
                        var parOpen = oParClose + Step * (hhValueList[i] - oParClose);
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
                        var af = Limit;
                        if (llValueList[i] < llValueList[i - 1] && afList[i - 1] < Limit)
                        {
                            if (afList[i - 1] + Step <= Limit)
                                af = afList[i - 1] + Step;
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
            sarList.Add(parOpenList[i]);

            var v1 = ((curData.Close - ma) / ma) * 10000;
            valueDict["V1"].Add(v1);
            var v2 = ((parOpenList[i] - curData.Close) / curData.Close) * 10000;
            valueDict["V2"].Add(v2);
            var v3 = ((curData.Close - ma) / ma + (parOpenList[i] - curData.Close) / curData.Close) * 10000;
            valueDict["V3"].Add(v3);
            var v4 = ((curData.Close - ma) / ma - (parOpenList[i] - curData.Close) / curData.Close) * 10000;
            valueDict["V4"].Add(v4);
            if (!IsSimpleMode)
            {
                graphDict["V3"].AddValue(v3, v3Color);
                graphDict["V2"].AddValue(v2, v2Color);
                graphDict["V1"].AddValue(v1, v1Color);
                graphDict["V4"].AddValue(v4, v4Color);
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

            valList.RemoveAt(valList.Count - 1);
            sarList.RemoveAt(sarList.Count - 1);

            Generate(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            Generate(Count - 1);
            
            
        }

        public double GetLastValue(string name)
        {
            if (valueDict[name].Count > 0)
                return valueDict[name][valueDict[name].Count - 1];
            else
                return JPR.NaN;
        }
        /// <summary>
        /// 获取BSM值
        /// </summary>
        /// <param name="name">V1,V2,V3,V4</param>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public double GetValue(string name, int index)
        {
            if (index >= 0 && index < Count)
                return valueDict[name][index];
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

        public double Step
        {
            get { return _step; }
        }
        public double Limit
        {
            get { return _limit; }
        }

    }
}
