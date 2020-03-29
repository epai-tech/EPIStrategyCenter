using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class BIAS:BaseIndicator
    {
        private int _num;
        private AverageS MaC;
        private double MaCValue;
        private double BIASValue;

        public BIAS(List<BarData> barDatas, int num = 10, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _num = num;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _num);
            Name = string.Format("BIAS{0}", paramTag);
            Description = "乖离率";
            valueDict.Add("BIAS", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("BIAS", new IndicatorGraph() { Name = "BIAS", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            
            MaC = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int num)
        {
            if (num != _num)
            {
                string paramTag = string.Format("({0})", num);
                Name = string.Format("BIAS{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["BIAS"].Tag = paramTag;
                }
                _num = num;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["BIAS"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["BIAS"].Clear();
            }
            MaC.SetParameters(_num);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateBIAS(i);
                }
            }
            base.Caculate();
        }

        private void GenerateBIAS(int i)
        {
            BarData curData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
            }
            else
            {
                curData = barDatas[i];
            }
            if (i < _num)
            {
                MaCValue = MaC.AddValue(curData.Close);
                valueDict["BIAS"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["BIAS"].AddValue(JPR.NaN, Color.White);
                }
            }
            else
            {
                MaCValue = MaC.AddValue(curData.Close);
                BIASValue = (curData.Close - MaCValue) / MaCValue;
                valueDict["BIAS"].Add(BIASValue);
                if (!IsSimpleMode)
                {
                    graphDict["BIAS"].AddValue(BIASValue, Color.White);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["BIAS"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["BIAS"].RemoveLast();
            }
            MaC.RemoveLast();
            GenerateBIAS(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateBIAS(Count - 1);
            
            
        }

        public List<double> GetValues()
        {
            return valueDict["BIAS"];
        }

        public double GetValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["BIAS"][index];
            else
                return JPR.NaN;
        }

        public double GetLast()
        {
            if (Count != 0)
                return valueDict["BIAS"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Num
        {
            get { return _num; }
        }
    }
}
