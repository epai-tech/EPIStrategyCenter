
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class DDI : BaseIndicator
    {
        /*
        TR:=MAX(ABS(HIGH-REF(HIGH,1)),ABS(LOW-REF(LOW,1)));
        DMZ:=IFELSE((HIGH+LOW)<=(REF(HIGH,1)+REF(LOW,1)),0,MAX(ABS(HIGH-REF(HIGH,1)),ABS(LOW-REF(LOW,1))));
        DMF:=IFELSE((HIGH+LOW)>=(REF(HIGH,1)+REF(LOW,1)),0,MAX(ABS(HIGH-REF(HIGH,1)),ABS(LOW-REF(LOW,1))));
        DIZ:=SUM(DMZ, N)/(SUM(DMZ, N)+SUM(DMF, N));
        DIF:=SUM(DMF, N)/(SUM(DMF, N)+SUM(DMZ, N));
        DDI:=DIZ-DIF;
        DDI,COLORSTICK;
        ADDI:SMA(DDI, N1, M);
        AD:MA(ADDI, M1);
        */
        private Summation DMZ_sum;
        private Summation DMF_sum;
        private AverageS2 ADDI;
        private AverageS AD;
        private int _n; //13
        private int _n1;//30
        private int _m; //10
        private int _m1;//5

        public DDI(List<BarData> bars, int n = 13, int n1 = 30,int m=10,int m1=5, bool isSimpleMode = true, bool isShowInMain = false, string tag = "")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            _n1 = n1;
            _m = m;
            _m1 = m1;
            IsSimpleMode = isSimpleMode;

            string paramTag = string.Format("({0},{1},{2},{3})", n, n1,m,m1);
            Name = string.Format("DDI{0}", paramTag);
            Description = "方向标准离差指数\r\n"
                         + "原理：\r\n"
                         + "1、TR =（最高价 - 昨日最高价）的绝对值与（最低价 - 昨日最低价）的绝对值两者之间较大者。\r\n"
                         + "2、如果（最高价 + 最低价）<=（昨日最高价 + 昨日最低价），DMZ = 0，\r\n"
                         + "   如果（最高价 + 最低价）>（昨日最高价 + 昨日最低价），DMZ =（最高价 - 昨日最低价）的绝对值与\r\n"
                         + "（最低价 - 昨日最低价）的绝对值中较大值。\r\n"
                         + "3、如果（最高价 + 最低价）>=（昨日最高价 + 昨日最低价），DMF = 0，\r\n"
                         + "如果（最高价 + 最低价）<（昨日最高价 + 昨日最低价），DMZ =（最高价 - 昨日最低价）的绝对值与\r\n"
                         + "（最低价 - 昨日最低价）的绝对值中较大值。\r\n"
                         + "4、DIZ = N个周期DMZ的和 /（N个周期DMZ的和 + N个周期DMF的和）\r\n"
                         + "5、DIF = N个周期DMF的和 /（N个周期DMF的和 + N个周期DMZ的和）\r\n"
                         + "6、DDI = DIZ - DIF\r\n"
                         + "7、ADDI = DDI在一定周期内的加权平均\r\n"
                         + "8、AD = ADDI在一定周期内的简单移动平均\r\n"
                         + "用法：\r\n"
                         + "（1）分析DDI柱状线，由红变绿(正变负)，卖出信号；由绿变红，买入信号。\r\n"
                         + "（2）ADDI与AD的交叉情况以及背离情况。\r\n";

            valueDict.Add("DDI", new List<double>());
            valueDict.Add("ADDI", new List<double>());
            valueDict.Add("AD", new List<double>());

            //paramTag = string.Format("({0})", n);
            paramTag = string.Format("({0},{1})", n1,m);
            paramTag = string.Format("({0})", m1);
            if (!IsSimpleMode)
            {
                graphDict.Add("ADDI", new IndicatorGraph() { Name = "ADDI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("DDI", new IndicatorGraph() { Name = "DDI", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("AD", new IndicatorGraph() { Name = "AD", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            DMZ_sum = new Summation();
            DMF_sum = new Summation();
            ADDI = new AverageS2();
            AD = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int n,int n1, int m, int m1)
        {
            if (n != _n || n1 != _n1 || m!=_m || m1 != _m1)
            {
                string paramTag = string.Format("({0},{1},{2},{3})", n, n1, m, m1);
                Name = string.Format("DDI{0}", paramTag);
                paramTag = string.Format("({0})", n);
                paramTag = string.Format("({0},{1})", n1, m);
                paramTag = string.Format("({0})", m1);
                if (!IsSimpleMode)
                {
                    graphDict["ADDI"].Tag = paramTag;
                    graphDict["DDI"].Tag = paramTag;
                    graphDict["AD"].Tag = paramTag;
                }
                _n = n;
                _n1 = n1;
                _m = m;
                _m1 = m1;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["DDI"].Clear();
            valueDict["ADDI"].Clear();
            valueDict["AD"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["DDI"].Clear();
                graphDict["ADDI"].Clear();
                graphDict["AD"].Clear();
            }
            DMZ_sum.SetParameters(_n);
            DMF_sum.SetParameters(_n);
            ADDI.SetParameters(_n1,_m);
            AD.SetParameters(_m1);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateDDI(i);
                }
            }
            base.Caculate();
        }

        private void GenerateDDI(int i)
        {
            /*
            TR:=MAX(ABS(HIGH-REF(HIGH,1)),ABS(LOW-REF(LOW,1)));
            DMZ:=IFELSE((HIGH+LOW)<=(REF(HIGH,1)+REF(LOW,1)),0,MAX(ABS(HIGH-REF(HIGH,1)),ABS(LOW-REF(LOW,1))));
            DMF:=IFELSE((HIGH+LOW)>=(REF(HIGH,1)+REF(LOW,1)),0,MAX(ABS(HIGH-REF(HIGH,1)),ABS(LOW-REF(LOW,1))));
            DIZ:=SUM(DMZ, N)/(SUM(DMZ, N)+SUM(DMF, N));
            DIF:=SUM(DMF, N)/(SUM(DMF, N)+SUM(DMZ, N));
            DDI:=DIZ-DIF;
            DDI,COLORSTICK;
            ADDI:SMA(DDI, N1, M);
            AD:MA(ADDI, M1);
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
            double tr = JPR.NaN,dmz = JPR.NaN,dmf = JPR.NaN,diz=JPR.NaN,dif=JPR.NaN,ddi=JPR.NaN,addi=JPR.NaN,ad=JPR.NaN,sum1,sum2;
            if (i > 0)
            {
                //计算TR
                tr = Math.Max(Math.Abs(curData.High - preData.High), Math.Abs(curData.Low - curData.Low));
                //计算 DMZ
                if ((curData.High + curData.Low) <= (preData.High + preData.Low))
                {
                    dmz = 0;
                } else
                {
                    dmz = tr;
                }
                //计算 DMF
                if ((curData.High + curData.Low) >= (preData.High + preData.Low))
                {
                    dmf = 0;
                }
                else
                {
                    dmf = tr;
                }
                //计算DIZ DIF
                sum1 = DMZ_sum.AddValue(dmz);
                sum2 = DMF_sum.AddValue(dmf);
                if (i >= _n )
                {
                    if (sum1 + sum2 > 0)
                    {
                        diz = sum1 / (sum1 + sum2);
                        dif = sum2 / (sum1 + sum2);
                    }
                }
            }
            ddi  = diz - dif;
            addi = ADDI.Caculate(ddi);
            ad   = AD.AddValue(addi);

            valueDict["DDI"].Add(ddi);
            valueDict["ADDI"].Add(addi);
            valueDict["AD"].Add(ad);
            if (!IsSimpleMode)
            {
                graphDict["DDI"].AddValue(ddi, Color.White);
                graphDict["ADDI"].AddValue(addi, Color.Yellow);
                graphDict["AD"].AddValue(ad, Color.Red);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);

            valueDict["DDI"].RemoveAt(Count - 1);
            valueDict["ADDI"].RemoveAt(Count - 1);
            valueDict["AD"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["DDI"].RemoveLast();
                graphDict["ADDI"].RemoveLast();
                graphDict["AD"].RemoveLast();
            }
            DMZ_sum.RemoveLast();
            DMF_sum.RemoveLast();
            ADDI.ResetValue();
            AD.RemoveLast();
            GenerateDDI(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateDDI(Count - 1);
            
            
        }

        public List<double> GetDDIValues()
        {
            return valueDict["DDI"];
        }
        public List<double> GetADDIValues()
        {
            return valueDict["ADDI"];
        }
        public List<double> GetADValues()
        {
            return valueDict["AD"];
        }

        public double GetDDIValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["DDI"][index];
            else
                return JPR.NaN;
        }
        public double GetADDIValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ADDI"][index];
            else
                return JPR.NaN;
        }
        public double GetADValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["AD"][index];
            else
                return JPR.NaN;
        }

        public double GetLastDDI()
        {
            if (Count != 0)
                return valueDict["DDI"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastADDI()
        {
            if (Count != 0)
                return valueDict["ADDI"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastAD()
        {
            if (Count != 0)
                return valueDict["AD"][Count - 1];
            else
                return JPR.NaN;
        }
        public int N { get { return _n; } }
        public int M { get { return _m; } }
        public int N1 { get { return _n1; } }
        public int M1 { get { return _m1; } }
    }


}
