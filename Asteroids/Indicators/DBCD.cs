
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class DBCD : BaseIndicator
    {
        /*
        N,M,T 5 16 76
        BIAS:=(CLOSE-MA(CLOSE, N))/MA(CLOSE, N);
        DIF:=(BIAS-REF(BIAS, M));
        DBCD:SMA(DIF, T,1);
        MM:MA(DBCD,5);
        */
        private AverageS  ma;
        private AverageS2 DBCDSMA;
        private AverageS  MM;
        private FixSeries BIAS;
        private EnumBarStruct barStruct;
        private int _n;
        private int _m;
        private int _t;

        public DBCD(List<BarData> bars, EnumBarStruct objBarStruct = EnumBarStruct.Close, int n = 5, int m = 16,int t=76, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _n = 14;
            _m = 6;
            _t = 76;
            IsSimpleMode = isSimpleMode;

            barStruct = objBarStruct;
            string paramTag = string.Format("({0},{1},{2},{3})", barStruct.ToString(), n, m, t);
            Name = string.Format("DBCD{0}", paramTag);
            Description = "异同离差乖离率\r\n"
                         +"原理：\r\n"
                         +"先计算乖离率BIAS，然后计算不同日的乖离率之间的离差，最后对离差进行指数移动平滑处理。\r\n"
                         +"用法：\r\n"
                         +"与乖离率相同。(优点是能够保持指标紧密同步，而且线条光滑，信号明确，能够有效的过滤掉伪信号。)";


            valueDict.Add("DBCD", new List<double>());
            valueDict.Add("MM", new List<double>());
            paramTag = string.Format("({0})", t);
            paramTag = string.Format("({0})", 5);
            if (!IsSimpleMode)
            {
                graphDict.Add("DBCD", new IndicatorGraph() { Name = "DBCD", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("MM", new IndicatorGraph() { Name = "MM", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            ma = new AverageS();
            DBCDSMA = new AverageS2();
            MM = new AverageS();
            BIAS = new FixSeries(_m+1);

            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct, int n, int m, int t)
        {
            if (objBarStruct != barStruct || n != _n || m != _m || t != _t)
            {
                string paramTag = string.Format("({0},{1},{2},{3})", objBarStruct.ToString(), n, m,t);
                Name = string.Format("DBCD{0}", paramTag);
                paramTag = string.Format("({0})", t);
                paramTag = string.Format("({0})", 5);
                if (!IsSimpleMode)
                {
                    graphDict["DBCD"].Tag = paramTag;
                    graphDict["MM"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _n = n;
                _m = m;
                _t = t;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["DBCD"].Clear();
            valueDict["MM"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["DBCD"].Clear();
                graphDict["MM"].Clear();
            }
            ma.SetParameters(_n);
            DBCDSMA.SetParameters(_t,1);
            MM.SetParameters(5);
            BIAS.SetParameters(_m+1);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateDBCD(i);
                }
            }
            base.Caculate();
        }

        private void GenerateDBCD(int i)
        {
            /*
            BIAS:= (CLOSE - MA(CLOSE, N)) / MA(CLOSE, N);
            DIF:= (BIAS - REF(BIAS, M));
            DBCD: SMA(DIF, T, 1);
            MM: MA(DBCD, 5);
            */
            BarData curData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
            }
            else
            {
                curData = barDatas[i];
            }
            double bias = JPR.NaN,value = JPR.NaN,diff=JPR.NaN,dbcd=JPR.NaN,mm=JPR.NaN;
            value = ma.AddValue(GetData(barDatas[i]));
            if (i >= _n -1)
            {
                //此时，MA(close,N)为正常值，已经有N根bar
                bias = (curData.Close - value) / value;
                BIAS.AddValue(bias);
            }
            if (BIAS.Length>_m)
            {
                //此时BIAS已经可以访问REF(BIAS,M)
                diff = bias - BIAS.REF(_m);
            }

            dbcd = DBCDSMA.Caculate(diff);
            mm = MM.AddValue(dbcd);

            valueDict["DBCD"].Add(dbcd);
            valueDict["MM"].Add(mm);
            if (!IsSimpleMode)
            {
                graphDict["DBCD"].AddValue(dbcd, Color.White);
                graphDict["MM"].AddValue(mm, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["DBCD"].RemoveAt(Count - 1);
            valueDict["MM"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["DBCD"].RemoveLast();
                graphDict["MM"].RemoveLast();
            }
            ma.RemoveLast();
            DBCDSMA.ResetValue();
            MM.RemoveLast();
            BIAS.RemoveLast();

            GenerateDBCD(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateDBCD(Count - 1);
            
            
        }

        private double GetData(BarData bar)
        {
            double data;
            switch (barStruct)
            {
                case EnumBarStruct.Open:
                    data = bar.Open;
                    break;
                case EnumBarStruct.High:
                    data = bar.High;
                    break;
                case EnumBarStruct.Low:
                    data = bar.Low;
                    break;
                case EnumBarStruct.Close:
                    data = bar.Close;
                    break;
                case EnumBarStruct.Volume:
                    data = bar.Volume;
                    break;
                case EnumBarStruct.OpenInterest:
                    data = bar.OpenInterest;
                    break;
                case EnumBarStruct.Amount:
                    data = bar.Amount;
                    break;
                default:
                    data = 0;
                    break;
            }
            return data;
        }

        public List<double> GetDBCDValues()
        {
            return valueDict["DBCD"];
        }

        public List<double> GetMMValues()
        {
            return valueDict["MM"];
        }

        public double GetDBCDValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["DBCD"][index];
            else
                return JPR.NaN;
        }

        public double GetMMValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MM"][index];
            else
                return JPR.NaN;
        }

        public double GetLastDBCD()
        {
            if (Count != 0)
                return valueDict["DBCD"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastMM()
        {
            if (Count != 0)
                return valueDict["MM"][Count - 1];
            else
                return JPR.NaN;
        }

        public int N { get { return _n; } }
        public int M { get { return _m; } }
        public int T { get { return _t; } }
        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
    }


}
