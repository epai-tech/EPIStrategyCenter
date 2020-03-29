
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class ADTM : BaseIndicator
    {
        /*
        Summation
        1 < N 23, M 8 < 100
        DTM:=IFELSE(OPEN<=REF(OPEN,1),0,MAX((HIGH-OPEN),(OPEN-REF(OPEN,1))));
        DBM:=IFELSE(OPEN>=REF(OPEN,1),0,MAX((OPEN-LOW),(OPEN-REF(OPEN,1))));
        STM:=SUM(DTM, N);
        SBM:=SUM(DBM, N);
        ADTM:IFELSE(STM>SBM,(STM-SBM)/STM,IFELSE(STM= SBM,0,(STM-SBM)/SBM));
        ADTMMA:MA(ADTM, M);
        */

        private Summation STM;
        private Summation SBM;
        //private AverageS ADTM;
        private AverageS ADTMMA;
        private int _n;
        private int _m;

        public ADTM(List<BarData> bars, int n = 23, int m = 8, bool isSimpleMode = true, bool isShowInMain = false, string tag = "")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            _m = m;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", n, m);
            Name = string.Format("ADTM{0}", paramTag);
            Description = "动态买卖器指标:\r\n"
                          +"原理：\r\n"
                          +"1、如果开盘价 <= 昨日开盘价，DTM = 0，如果开盘价 > 昨日开盘价，DTM = \r\n"
                          +"(最高价 - 开盘价)和(开盘价 - 昨日开盘价)的较大值。\r\n"
                          +"2、如果开盘价 >= 昨日开盘价，DBM = 0，如果开盘价 < 昨日开盘价，DBM = \r\n"
                          +"(开盘价 - 最低价)和（开盘价 - 昨日开盘价）的较大值。\r\n"
                          +"3、STM = DTM在N个周期内的和。\r\n"
                          +"4、SBM = DBM在N个周期内的和。\r\n"
                          +"5、如果STM > SBM,ADTM = (STM - SBM) / STM,如果STM = SBM,ADTM = 0,如果STM < SBM,ADTM = (STM - SBM) / SBM。\r\n"
                          +"6、ADTMMA为ADTM在某周期内的简单移动平均。\r\n"
                          +"用法： \r\n"
                          +"1、该指标在 + 1到 - 1之间波动；\r\n"
                          +"2、低于 - 0.5时为很好的买入点,高于 + 0.5时需注意风险。\r\n";

            valueDict.Add("ADTM", new List<double>());
            valueDict.Add("ADTMMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("ADTM", new IndicatorGraph() { Name = "ADTM", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("ADTMMA", new IndicatorGraph() { Name = "ADTMMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            STM = new Summation();
            SBM = new Summation();
            ADTMMA = new AverageS();
            IsShowInMain = isShowInMain;
            MaxCacheCount = Math.Max(Math.Max(n, m) + 1, MaxCacheCount);
            Caculate();
        }

        public void SetParameters(int n, int m)
        {
            if (n != _n || m != _m)
            {
                string paramTag = string.Format("({0},{1})", n, m);
                Name = string.Format("ADTM{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["ADTM"].Tag = paramTag;
                    graphDict["ADTMMA"].Tag = paramTag;
                }
                _n = n;
                _m = m;
                MaxCacheCount = Math.Max(Math.Max(n, m) + 1, MaxCacheCount);
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["ADTM"].Clear();
            valueDict["ADTMMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["ADTM"].Clear();
                graphDict["ADTMMA"].Clear();
            }
            STM.SetParameters(_n);
            SBM.SetParameters(_n);
            ADTMMA.SetParameters(_m);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateADTM(i);
                }
            }
            base.Caculate();
        }

        public void ReleaseCache()
        {
            IsSimpleMode = true;
            if (Count - MaxCacheCount > 0)
            {
                foreach (var key in valueDict.Keys)
                {
                    valueDict[key].RemoveRange(0, Count - MaxCacheCount);
                    if (!IsSimpleMode)
                    {
                        graphDict[key].Values.RemoveRange(0, Count - MaxCacheCount);
                        graphDict[key].Colors.RemoveRange(0, Count - MaxCacheCount);
                    }
                    Count = MaxCacheCount;
                }
            }
        }

        private void GenerateADTM(int i)
        {
            /*代码基本跟文华代码一致
              DTM:= IFELSE(OPEN <= REF(OPEN, 1), 0, MAX((HIGH - OPEN), (OPEN - REF(OPEN, 1))));
              DBM:= IFELSE(OPEN >= REF(OPEN, 1), 0, MAX((OPEN - LOW), (OPEN - REF(OPEN, 1))));
              STM:= SUM(DTM, N);
              SBM:= SUM(DBM, N);
              ADTM: IFELSE(STM > SBM, (STM - SBM) / STM, IFELSE(STM = SBM, 0, (STM - SBM) / SBM));
              ADTMMA: MA(ADTM, M);
            */
            BarData curData = null;
            BarData preData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(1);
            }
            else {
                curData = barDatas[i];
                if (i > 0 && i < barDatas.Count)
                {
                    preData = barDatas[i - 1];
                }
            }
            double dtm=JPR.NaN, dbm=JPR.NaN, stm=JPR.NaN, sbm=JPR.NaN, adtm=JPR.NaN, adtmma=JPR.NaN;
            if (i > 0)
            {
                dtm = (curData.Open <= preData.Open) ? (0) :
                     (Math.Max(curData.High - curData.Open, curData.Open - preData.Open));
                dbm = (curData.Open >= preData.Open) ? (0) :
                     (Math.Max(curData.Open - curData.Low, curData.Open - preData.Open));
                stm = STM.AddValue(dtm);//may be JPR.NaN
                sbm = SBM.AddValue(dbm);//may be JPR.NaN

                if (stm != JPR.NaN && sbm != JPR.NaN)
                {
                    if (stm > sbm && stm != 0)
                    {
                        adtm = (stm - sbm) / stm;
                    } else if (stm < sbm && sbm != 0)
                    {
                        adtm = (stm - sbm) / sbm;
                    } else if (stm == sbm)
                    {
                        adtm = 0;
                    }
                }

                if (i >= _n )
                {
                    //已经满足求和长度了，应该已经是正常值了。
                    adtmma = ADTMMA.AddValue(adtm);
                }
            }
            valueDict["ADTM"].Add(adtm);
            valueDict["ADTMMA"].Add(adtmma);
            if (!IsSimpleMode)
            {
                graphDict["ADTM"].AddValue(adtm, Color.White);
                graphDict["ADTMMA"].AddValue(adtmma, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["ADTM"].RemoveAt(Count - 1);
            valueDict["ADTMMA"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["ADTM"].RemoveLast();
                graphDict["ADTMMA"].RemoveLast();
            }
            
            ADTMMA.RemoveLast();
            GenerateADTM(Count - 1);
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateADTM(Count - 1);        
        }

        public List<double> GetADTMValues()
        {
            return valueDict["ADTM"];
        }
        public List<double> GetADTMMAValues()
        {
            return valueDict["ADTMMA"];
        }

        public double GetADTMValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ADTM"][index];
            else
                return JPR.NaN;
        }
        public double GetADTMMAValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ADTMMA"][index];
            else
                return JPR.NaN;
        }

        public double GetLastADTM()
        {
            if (Count != 0)
                return valueDict["ADTM"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastADTMMA()
        {
            if (Count != 0)
                return valueDict["ADTMMA"][Count - 1];
            else
                return JPR.NaN;
        }

        //用于界面设置指标参数
        public int N { get { return _n; } }
        public int M { get { return _m; } }
    }


}
