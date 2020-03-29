using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class OBV:BaseIndicator
    {
        private int _length;
        private AverageS averageS;
        private double preSumOBV, sumOBV;

        public OBV(List<BarData> barDatas, int length = 20, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", _length);
            Name = string.Format("OBV{0}", paramTag);
            Description = "能量潮";
            valueDict.Add("OBV", new List<double>());
            valueDict.Add("MAOBV", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("OBV", new IndicatorGraph() { Name = "OBV", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("MAOBV", new IndicatorGraph() { Name = "MAOBV", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            averageS = new AverageS();
            sumOBV = 0;
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length)
        {
            if (length != _length)
            {
                string paramTag = string.Format("({0})", length);
                Name = string.Format("OBV{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["OBV"].Tag = paramTag;
                    graphDict["MAOBV"].Tag = paramTag;
                }
                _length = length;
                sumOBV = 0;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["OBV"].Clear();
            valueDict["MAOBV"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["OBV"].Clear();
                graphDict["MAOBV"].Clear();
            }
            averageS.SetParameters(_length);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateOBV(i);
                }
            }
            base.Caculate();
        }

        private void GenerateOBV(int i)
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
                if (curData.Close > curData.Open)
                { preSumOBV = sumOBV = curData.Volume; }
                else
                { preSumOBV = sumOBV = -curData.Volume; }
            }
            else
            {
                preSumOBV = sumOBV;
                if (curData.Close > preData.Close)
                { sumOBV += curData.Volume; }
                else
                { sumOBV -= curData.Volume; }
            }
            double value = averageS.AddValue(sumOBV);
            valueDict["OBV"].Add(sumOBV);
            valueDict["MAOBV"].Add(value);
            if (!IsSimpleMode)
            {
                graphDict["OBV"].AddValue(sumOBV, Color.White);
                graphDict["MAOBV"].AddValue(value, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["OBV"].RemoveAt(Count - 1);
            valueDict["MAOBV"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["OBV"].RemoveLast();
                graphDict["MAOBV"].RemoveLast();
            }
            averageS.RemoveLast();
            sumOBV = preSumOBV;
            GenerateOBV(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateOBV(Count - 1);
            
            
        }

        public List<double> GetOBVValues()
        {
            return valueDict["OBV"];
        }
        public List<double> GetMAOBVValues()
        {
            return valueDict["MAOBV"];
        }

        public double GeOBVValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["OBV"][index];
            else
                return JPR.NaN;
        }
        public double GetMAOBVValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MAOBV"][index];
            else
                return JPR.NaN;
        }


        public double GetLastOBV()
        {
            if (Count != 0)
                return valueDict["OBV"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastMAOBV()
        {
            if (Count != 0)
                return valueDict["MAOBV"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
