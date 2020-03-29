using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class MTM:BaseIndicator
    {
        private int _numMTM;
        private int _numMTMMA;
        private AverageS MTMMA;
        private double MTMValue;
        private double MTMMAValue;

        public MTM(List<BarData> barDatas, int numMTM = 12, int numMTMMA = 6, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _numMTM = numMTM;
            _numMTMMA = numMTMMA;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _numMTM, _numMTMMA);
            Name = string.Format("MTM{0}", paramTag);
            Description = "动力指标";
            valueDict.Add("MTM", new List<double>());
            valueDict.Add("MTMMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("MTM", new IndicatorGraph() { Name = "MTM", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("MTMMA", new IndicatorGraph() { Name = "MTMMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            MaxCacheCount = numMTM * 2 < 10 ? 10 : numMTM * 2;
            MTMMA = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int numMTM, int numMTMMA)
        {
            if (numMTM != _numMTM || numMTMMA != _numMTMMA)
            {
                string paramTag = string.Format("({0},{1})", numMTM, numMTMMA);
                Name = string.Format("MTM{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["MTM"].Tag = paramTag;
                    graphDict["MTMMA"].Tag = paramTag;
                }
                _numMTM = numMTM;
                _numMTMMA = numMTMMA;
                MaxCacheCount = numMTM * 2 < 10 ? 10 : numMTM * 2;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["MTM"].Clear();
            valueDict["MTMMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["MTM"].Clear();
                graphDict["MTMMA"].Clear();
            }
            MTMMA.SetParameters(_numMTMMA);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateMTM(i);
                }
            }
            base.Caculate();
        }

        private void GenerateMTM(int i)
        {
            BarData curData = null;
            BarData preData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(_numMTM);
            }
            else
            {
                curData = barDatas[i];
                if (i >= _numMTM && i < barDatas.Count)
                {
                    preData = barDatas[i - _numMTM];
                }
            }
            if (i < _numMTM)
            {
                valueDict["MTM"].Add(JPR.NaN);
                valueDict["MTMMA"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["MTM"].AddValue(JPR.NaN, Color.White);
                    graphDict["MTMMA"].AddValue(JPR.NaN, Color.Yellow);
                }
            }
            else
            {
                MTMValue = curData.Close - preData.Close;
                MTMMAValue = MTMMA.AddValue(MTMValue); 
                valueDict["MTM"].Add(MTMValue);
                valueDict["MTMMA"].Add(MTMMAValue);
                if (!IsSimpleMode)
                {
                    graphDict["MTM"].AddValue(MTMValue, Color.White);
                    graphDict["MTMMA"].AddValue(MTMMAValue, Color.Yellow);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            if (valueDict["MTM"].Count > 0)
            {
                valueDict["MTM"].RemoveAt(Count - 1);
                valueDict["MTMMA"].RemoveAt(Count - 1);
                if (!IsSimpleMode)
                {
                    graphDict["MTM"].RemoveLast();
                    graphDict["MTMMA"].RemoveLast();
                }
                MTMMA.RemoveLast();
                GenerateMTM(Count - 1);
            }
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateMTM(Count - 1);
            
            
        }

        public List<double> GetMTMValues()
        {
            return valueDict["MTM"];
        }
        public List<double> GetMTMMAValues()
        {
            return valueDict["MTMMA"];
        }

        public double GetMTMValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MTM"][index];
            else
                return JPR.NaN;
        }

        public double GetMTMMAValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MTMMA"][index];
            else
                return JPR.NaN;
        }

        public double GetMTMLast()
        {
            if (Count != 0)
                return valueDict["MTM"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetMTMMALast()
        {
            if (Count != 0)
                return valueDict["MTMMA"][Count - 1];
            else
                return JPR.NaN;
        }

        public int NumMTM
        {
            get { return _numMTM; }
        }
        public int NumMTMMA
        {
            get { return _numMTMMA; }
        }
    }
}
