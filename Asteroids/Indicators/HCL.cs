
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class HCL : BaseIndicator
    {
        //N 1-1000-10
        //MAH:MA(HIGH, N);//最高价的N周期平均
        //MAL:MA(LOW, N);//最低价的N周期平均
        //MAC:MA(CLOSE, N);//收盘价的N周期平均
        private AverageS mah;
        private AverageS mal;
        private AverageS mac;
        private int _n;

        public HCL(List<BarData> bars,int n = 10, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})",n);
            Name = string.Format("HCL{0}", paramTag);
            Description = "均线通道:\r\n"
                         + "原理：\r\n"
                         + "K线图叠加三条均线：最高价均线、最低价均线、收盘价均线;";

            valueDict.Add("mah", new List<double>());
            valueDict.Add("mal", new List<double>());
            valueDict.Add("mac", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("mah", new IndicatorGraph() { Name = "mah", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("mal", new IndicatorGraph() { Name = "mal", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("mac", new IndicatorGraph() { Name = "mac", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            mah = new AverageS();
            mal = new AverageS();
            mac = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int n)
        {
            if (n != _n)
            {
                string paramTag = string.Format("({0})", n);
                Name = string.Format("HCL{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["mah"].Tag = paramTag;
                    graphDict["mac"].Tag = paramTag;
                    graphDict["mal"].Tag = paramTag;
                }
                _n = n;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["mah"].Clear();
            valueDict["mac"].Clear();
            valueDict["mal"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["mah"].Clear();
                graphDict["mac"].Clear();
                graphDict["mal"].Clear();
            }
            mah.SetParameters(_n);
            mac.SetParameters(_n);
            mal.SetParameters(_n);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateHCL(i);
                }
            }
            base.Caculate();
        }

        private void GenerateHCL(int i)
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
            double hv = mah.AddValue(curData.High);
            double cv = mac.AddValue(curData.Close);
            double lv = mal.AddValue(curData.Low);

            valueDict["mah"].Add(hv);
            valueDict["mac"].Add(cv);
            valueDict["mal"].Add(lv);
            if (!IsSimpleMode)
            {
                graphDict["mah"].AddValue(hv, Color.Yellow);
                graphDict["mac"].AddValue(cv, Color.White);
                graphDict["mal"].AddValue(lv, Color.Red);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["mah"].RemoveAt(Count - 1);
            mah.RemoveLast();
            valueDict["mac"].RemoveAt(Count - 1);
            mac.RemoveLast();

            valueDict["mal"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["mac"].RemoveLast();
                graphDict["mah"].RemoveLast();
                graphDict["mal"].RemoveLast();
            }
            mal.RemoveLast();

            GenerateHCL(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateHCL(Count - 1);
            
            
        }

        public List<double> GetHValues()
        {
            return valueDict["mah"];
        }

        public List<double> GetLValues()
        {
            return valueDict["mal"];
        }

        public List<double> GetCValues()
        {
            return valueDict["mac"];
        }

        public double GetHValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["mah"][index];
            else
                return JPR.NaN;
        }

        public double GetCValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["mac"][index];
            else
                return JPR.NaN;
        }

        public double GetLValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["mal"][index];
            else
                return JPR.NaN;
        }

        public double GetLastH()
        {
            if (Count != 0)
                return valueDict["mah"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastC()
        {
            if (Count != 0)
                return valueDict["mac"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastL()
        {
            if (Count != 0)
                return valueDict["mal"][Count - 1];
            else
                return JPR.NaN;
        }
        
        public int N { get { return _n; } }
    }
}
 