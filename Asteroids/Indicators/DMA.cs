using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Asteroids.Functions;
using EPI.Model;
using System.Drawing;

namespace EPI.Asteroids.Indicators
{
    public class DMA : BaseIndicator
    {
        private int _num1;
        private int _num2;
        private int _numMADMA;
        private AverageS MAC1;
        private AverageS MAC2;
        private double MAC1Value;
        private double MAC2Value;
        private double DMAValue;
        private AverageS MADMA;
        private double MADMAValue;

        public DMA(List<BarData> barDatas, int num1 = 10, int num2 = 50, int numMADMA = 6, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _num1 = num1;
            _num2 = num2;
            _numMADMA = numMADMA;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1},{2})", _num1, _num2, _numMADMA);
            Name = string.Format("DMA{0}", paramTag);
            Description = "平行线差";
            valueDict.Add("DMA", new List<double>());
            valueDict.Add("MADMA", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("DMA", new IndicatorGraph() { Name = "DMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("MADMA", new IndicatorGraph() { Name = "MADMA", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            
            MAC1 = new AverageS();
            MAC2 = new AverageS();
            MADMA = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int num1, int num2, int numMADMA)
        { 
            if(num1!=_num1 || num2!=_num2 || numMADMA!=_numMADMA)
            {
                string paramTag = string.Format("({0},{1},{2})", num1, num2, numMADMA);
                Name = string.Format("DMA{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["DMA"].Tag = paramTag;
                    graphDict["MADMA"].Tag = paramTag;
                }
                _num1 = num1;
                _num2 = num2;
                _numMADMA = numMADMA;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["DMA"].Clear();
            valueDict["MADMA"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["DMA"].Clear();
                graphDict["MADMA"].Clear();
            }
            MAC1.SetParameters(_num1);
            MAC2.SetParameters(_num2);
            MADMA.SetParameters(_numMADMA);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateDMA(i);
                }
            }
            base.Caculate();
        }

        private void GenerateDMA(int i)
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
            if (i < _num1 || i < _num2)
            {
                MAC1.AddValue(curData.Close);
                MAC2.AddValue(curData.Close);
                valueDict["DMA"].Add(JPR.NaN);
                valueDict["MADMA"].Add(JPR.NaN);
                if (!IsSimpleMode)
                {
                    graphDict["DMA"].AddValue(JPR.NaN, Color.White);
                    graphDict["MADMA"].AddValue(JPR.NaN, Color.Yellow);
                }
            }
            else if (i >= _num1 && i >= _num2)
            {
                MAC1Value = MAC1.AddValue(curData.Close);
                MAC2Value = MAC2.AddValue(curData.Close);
                DMAValue = MAC1Value - MAC2Value;
                MADMAValue = MADMA.AddValue(DMAValue);
                valueDict["DMA"].Add(DMAValue);
                valueDict["MADMA"].Add(MADMAValue);
                if (!IsSimpleMode)
                {
                    graphDict["DMA"].AddValue(DMAValue, Color.White);
                    graphDict["MADMA"].AddValue(MADMAValue, Color.Yellow);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["DMA"].RemoveAt(Count - 1);
            valueDict["MADMA"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["DMA"].RemoveLast();
                graphDict["MADMA"].RemoveLast();
            }
            MAC1.RemoveLast();
            MAC2.RemoveLast();
            MADMA.RemoveLast();
            GenerateDMA(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateDMA(Count - 1);
            
            
        }

        public List<double> GetDMAValues()
        {
            return valueDict["DMA"];
        }
        public List<double> GetMADMAValues()
        {
            return valueDict["MADMA"];
        }

        public double GetDMAValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["DMA"][index];
            else
                return JPR.NaN;
        }

        public double GetMADMAValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MADMA"][index];
            else
                return JPR.NaN;
        }

        public double GetLastDMA()
        {
            if (Count != 0)
                return valueDict["DMA"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastMADMA()
        {
            if (Count != 0)
                return valueDict["MADMA"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Num1
        {
            get { return _num1; }
        }
        public int Num2
        {
            get { return _num2; }
        }
        public int NumMADMA
        {
            get { return _numMADMA; }
        }
    }
}
