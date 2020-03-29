using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class ATR:BaseIndicator
    {
        private int _length;
        private TrueRange _tr;
        private AverageS _atr;

        public ATR(List<BarData> barDatas, int length = 14, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("ATR{0}", paramTag);
            Description = "平均真实区域";
            valueDict.Add("TR", new List<double>());
            valueDict.Add("ATR", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("TR", new IndicatorGraph() { Name = "TR", Tag = paramTag, LineStyle = EnumLineStyle.Steam });
                graphDict.Add("ATR", new IndicatorGraph() { Name = "ATR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            _tr = new TrueRange();
            _atr = new AverageS();
			IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("ATR{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["TR"].Tag = paramTag;
                    graphDict["ATR"].Tag = paramTag;
                }
                _length = length;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["TR"].Clear();
            valueDict["ATR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["TR"].Clear();
                graphDict["ATR"].Clear();
            }
            _atr.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateATR(i);
                }
            }
            base.Caculate();
        }

        private void GenerateATR(int i)
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
            if (i == 0)
            { trValue = (curData.High - curData.Low); }
            else
            { trValue = _tr.Caculate(preData.Close, curData.High, curData.Low); } 
            valueDict["TR"].Add(trValue);
			atrValue=_atr.AddValue(trValue);
            valueDict["ATR"].Add(atrValue);
            if (!IsSimpleMode)
            {
                graphDict["TR"].AddValue(trValue, Color.White);
                graphDict["ATR"].AddValue(atrValue, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["TR"].RemoveAt(Count - 1);
            valueDict["ATR"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["TR"].RemoveLast();
                graphDict["ATR"].RemoveLast();
            }
            _atr.RemoveLast();
            GenerateATR(Count - 1);
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateATR(Count - 1);
            
        }

        public List<double> GetTRValues()
        {
            return valueDict["TR"];
        }

        public List<double> GetATRValues()
        {
            return valueDict["ATR"];
        }

        public double GetLastTR()
        {
            if (Count != 0)
                return valueDict["TR"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastATR()
        {
            if (Count != 0)
                return valueDict["ATR"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetATRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ATR"][index];
            else
                return JPR.NaN;
        }

        public double GetTRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["TR"][index];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        } 
    }
}
