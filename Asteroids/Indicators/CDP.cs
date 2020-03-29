
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class CDP : BaseIndicator
    {
        /*  1 < n=3 < 100 
        PT  := REF(HIGH,1)-REF(LOW,1);
        CDP := (REF(HIGH,1) + REF(LOW,1) + REF(CLOSE,1))/3;
        AH  : MA(CDP + PT, N);
        AL  : MA(CDP - PT, N);
        NH  : MA(2*CDP-LOW, N);
        NL  : MA(2*CDP-HIGH, N);
        */
        private AverageS AH;
        private AverageS AL;
        private AverageS NH;
        private AverageS NL;
        private int _n;

        public CDP(List<BarData> bars, int n=3, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", n);
            Name = string.Format("CDP{0}", paramTag);
            Description = "CDP逆势操作\r\n"
                         + "原理:\r\n"
                         + "CDP 为最高价、最低价、收盘价的均值，称中价；中价与前一天的振幅的和、差分别记为AH(最高值)、AL(最低值)；两倍中价与最低价的差称NH(近高值)，与最高价的差称NL(近低值)。\r\n"
                         + "用法:\r\n"
                         + "1.一种极短线的操作方法。在波动并不很大的情况下，即开市价处在近高值与近低值之间，通\r\n"
                         + "常交易者可以在近低值的价们买进，而在近高期的价位卖出；或在近高值的价位卖出，近低值的价位买进。\r\n"
                         + "2.在波动较大的情况下，即开市价开在最高值或最低值附近时，意味着跳空开高或跳空开低，\r\n"
                         + "是一个大行情的发动开始，因此交易者可在最高值的价位去追买，最低值的价位去追卖。通常一个\r\n"
                         + "跳空，意味着一个强烈的涨跌，应有相当的利润。\r\n";

            valueDict.Add("AH", new List<double>());
            valueDict.Add("AL", new List<double>());
            valueDict.Add("NH", new List<double>());
            valueDict.Add("NL", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("AH", new IndicatorGraph() { Name = "AH", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("AL", new IndicatorGraph() { Name = "AL", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("NH", new IndicatorGraph() { Name = "NH", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("NL", new IndicatorGraph() { Name = "NL", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            AH = new AverageS();
            AL = new AverageS();
            NH = new AverageS();
            NL = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int n)
        {
            if (n != _n)
            {
                string paramTag = string.Format("({0})", n);
                Name = string.Format("CDP{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["AH"].Tag = paramTag;
                    graphDict["AL"].Tag = paramTag;
                    graphDict["NH"].Tag = paramTag;
                    graphDict["NL"].Tag = paramTag;
                }
                _n = n;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["AH"].Clear();
            valueDict["AL"].Clear();
            valueDict["NH"].Clear();
            valueDict["NL"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["AH"].Clear();
                graphDict["AL"].Clear();
                graphDict["NH"].Clear();
                graphDict["NL"].Clear();
            }
            AH.SetParameters(_n);
            AL.SetParameters(_n);
            NH.SetParameters(_n);
            NL.SetParameters(_n);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateCDP(i);
                }
            }
            base.Caculate();
        }

        private void GenerateCDP(int i)
        {
            /*
             PT:= REF(HIGH, 1) - REF(LOW, 1);
             CDP:= (REF(HIGH, 1) + REF(LOW, 1) + REF(CLOSE, 1)) / 3;
             AH: MA(CDP + PT, N);
             AL: MA(CDP - PT, N);
             NH: MA(2 * CDP - LOW, N);
             NL: MA(2 * CDP - HIGH, N);
            */
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
            double ah=JPR.NaN,al=JPR.NaN,nh=JPR.NaN,nl=JPR.NaN;
            if (i > 0) {
                double pt = preData.High - preData.Low;
                double cdp = (preData.High + preData.Low + preData.Close) / 3;
                ah = AH.AddValue(cdp + pt);
                al = AL.AddValue(cdp - pt);
                nh = NH.AddValue(2 * cdp - curData.Low);
                nl = NL.AddValue(2 * cdp - curData.High);

            }

            valueDict["AH"].Add(ah);
            valueDict["AL"].Add(al);
            valueDict["NH"].Add(nh);
            valueDict["NL"].Add(nl);
            if (!IsSimpleMode)
            {
                graphDict["AH"].AddValue(ah, Color.White);
                graphDict["AL"].AddValue(al, Color.Yellow);
                graphDict["NH"].AddValue(nh, Color.Green);
                graphDict["NL"].AddValue(nl, Color.Blue);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["AH"].RemoveAt(Count - 1);
            valueDict["AL"].RemoveAt(Count - 1);
            valueDict["NH"].RemoveAt(Count - 1);
            valueDict["NL"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["AH"].RemoveLast();
                graphDict["AL"].RemoveLast();
                graphDict["NH"].RemoveLast();
                graphDict["NL"].RemoveLast();
            }
            AH.RemoveLast();
            AL.RemoveLast();
            NH.RemoveLast();
            NL.RemoveLast();
            GenerateCDP(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateCDP(Count - 1);
            
            
        }

        public List<double> GetAHValues()
        {
            return valueDict["AH"];
        }

        public double GetAHValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["AH"][index];
            else
                return JPR.NaN;
        }

        public List<double> GetALValues()
        {
            return valueDict["AL"];
        }

        public double GetALValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["AL"][index];
            else
                return JPR.NaN;
        }
        public List<double> GetNHValues()
        {
            return valueDict["NH"];
        }

        public double GetNHValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["NH"][index];
            else
                return JPR.NaN;
        }

        public List<double> GetNLValues()
        {
            return valueDict["NL"];
        }

        public double GetNLValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["NL"][index];
            else
                return JPR.NaN;
        }

        public double GetLastAH()
        {
            if (Count != 0)
                return valueDict["AH"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastAL()
        {
            if (Count != 0)
                return valueDict["AL"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastNH()
        {
            if (Count != 0)
                return valueDict["NH"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastNL()
        {
            if (Count != 0)
                return valueDict["NL"][Count - 1];
            else
                return JPR.NaN;
        }

        //用于界面设置指标参数
        public int N { get { return _n; } }
    }


}
