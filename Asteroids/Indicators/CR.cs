
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class CR : BaseIndicator
    {   /* n = 26
           M1= 5
           M2= 10 
           M3= 20
        MID := (HIGH+LOW+CLOSE)/3;
        CR:SUM(MAX(0, HIGH-REF(MID,1)),N)/SUM(MAX(0, REF(MID,1)-LOW),N)*100;
        CRMA1:REF(MA(CR, M1),M1/2.5+1);
        CRMA2:REF(MA(CR, M2),M2/2.5+1);
        CRMA3:REF(MA(CR, M3),M3/2.5+1);*/

        private Summation CR_up;
        private Summation CR_dn;
        private AverageS ma1;
        private AverageS ma2;
        private AverageS ma3;
        private FixSeries MID;
        private int _n;
        private int _m1,_m2,_m3;

        public CR(List<BarData> bars, int n = 26, int m1 = 5, int m2 = 10, int m3 = 20, bool isSimpleMode = true, bool isShowInMain = false, string tag = "")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
            IsSimpleMode = isSimpleMode;

            string paramTag = string.Format("({0},{1},{2},{3})", n,m1,m2,m3);
            Name = string.Format("CP{0}", paramTag);
            Description = "CR能量指标:\r\n"
                         + "原理：\r\n"
                         + "在N日内，若某日最高价高于前一日中价(最高、最低价的均值)，将二者的差累加到强势和中；\r\n"
                         + "若某日最低价低于前中价，将前中价与最低价的差累加到弱势和中。强势和除以弱势和，再乘100，\r\n"
                         + "即得CR。同时绘制CR的M1周期、M2周期、M3周期均线。\r\n"
                         + "用法：\r\n"
                         + "该指标用于判断买卖时机。能够测量人气的热度和价格动量的潜能；\r\n"
                         + "显示压力带和支撑带，以辅助BRAR的不足。实际用来进行价格预测时：\r\n"
                         + "（1）a、b两线所夹的区域称为 副地震带，当CR由下往上欲穿越副地震带时，价格相对将遭次\r\n"
                         + "级压力干扰；当CR欲由上往下贯穿副地震带时，价格相对将遭遇次级支撑干扰。\r\n"
                         + "（2）c、d两线所夹成的区域称为 主地震带，当CR由下往上欲穿越主地震带时，价格相对将遭\r\n"
                         + "遇强大压力干扰；当CR由上往下欲贯穿主地震带时，价格相对将遭遇强大支撑干扰。\r\n"
                         + "（3）CR相对价格也会产生背离现象。特别是在高价区。\r\n"
                         + "（4）CR跌至a、b、c、d四条线的下方，再度由低点向上爬升160 % 时，为短线卖出时机。\r\n"
                         + "例如从CR100升到160。"
                         + "（5）CR下跌至40以下时，价格形成底部的机会相当高。\r\n"
                         + "（6）CR高于300 - 400之间时，价格很容易向下反转。\r\n";

            valueDict.Add("CR", new List<double>());
            valueDict.Add("CRMA1", new List<double>());
            valueDict.Add("CRMA2", new List<double>());
            valueDict.Add("CRMA3", new List<double>());

            //paramTag = string.Format("({0})",n);
            
            paramTag = string.Format("({0})", m1);
            paramTag = string.Format("({0})", m2);
            paramTag = string.Format("({0})", m3);
            if (!IsSimpleMode)
            {
                graphDict.Add("CRMA2", new IndicatorGraph() { Name = "CRMA2", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("CRMA1", new IndicatorGraph() { Name = "CRMA1", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("CR", new IndicatorGraph() { Name = "CR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("CRMA3", new IndicatorGraph() { Name = "CRMA3", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            ma1 = new AverageS();
            ma2 = new AverageS();
            ma3 = new AverageS();
            MID = new FixSeries(2);
            CR_up = new Summation();
            CR_dn = new Summation();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int n, int m1, int m2, int m3)
        {
            if (n != _n || m1 != _m1 || m2 != _m2 || m3 != _m3)
            {
                string paramTag = string.Format("({0},{1},{2},{3})", n, m1, m2, m3);
                Name = string.Format("CR{0}", paramTag);
                paramTag = string.Format("({0})", n); 
                paramTag = string.Format("({0})", m1);
                paramTag = string.Format("({0})", m2);
                paramTag = string.Format("({0})", m3);
                if (!IsSimpleMode)
                {
                    graphDict["CRMA2"].Tag = paramTag;
                    graphDict["CRMA1"].Tag = paramTag;
                    graphDict["CR"].Tag = paramTag;
                    graphDict["CRMA3"].Tag = paramTag;
                }

                _n = n;
                _m1 = m1;
                _m2 = m2;
                _m3 = m3;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["CR"].Clear();
            valueDict["CRMA1"].Clear();
            valueDict["CRMA2"].Clear();
            valueDict["CRMA3"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["CR"].Clear();
                graphDict["CRMA1"].Clear();
                graphDict["CRMA2"].Clear();
                graphDict["CRMA3"].Clear();
            }

            ma1.SetParameters(_m1);
            ma2.SetParameters(_m2);
            ma3.SetParameters(_m3);
            //for update case-----------------------------------------------
            MID.SetParameters(2);
            CR_up.SetParameters(_n);
            CR_dn.SetParameters(_n);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateCR(i);
                }
            }
            base.Caculate();
        }

        private void GenerateCR(int i)
        {
            /* n:26
            M1:5
            M2 :10 
            M3:20
            MID: (HIGH+LOW+CLOSE)/3;
            CR:SUM(MAX(0, HIGH-REF(MID,1)),N)/SUM(MAX(0, REF(MID,1)-LOW),N)*100;
            CRMA1:REF(MA(CR, M1),M1/2.5+1);
            CRMA2:REF(MA(CR, M2),M2/2.5+1);
            CRMA3:REF(MA(CR, M3),M3/2.5+1);*/
            BarData curData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
            }
            else
            {
                curData = barDatas[i];
            }
            double cr = JPR.NaN, crma1 = JPR.NaN, crma2 = JPR.NaN, crma3 = JPR.NaN,cr_up =JPR.NaN,cr_dn = JPR.NaN,value;
            MID.AddValue((curData.High + curData.Low + curData.Close) / 3);
            if (i > 0)
            {
                cr_up = Math.Max(0, curData.High - MID.REF(1));
                cr_dn = Math.Max(0, MID.REF(1) - curData.Low);
                cr_up = CR_up.AddValue(cr_up);
                cr_dn = CR_dn.AddValue(cr_dn);
            }
            if (i >= _n && cr_dn != 0)
            {
                cr = cr_up / cr_dn * 100;
                crma1 = ma1.AddValue(cr);
                crma2 = ma2.AddValue(cr);
                crma3 = ma3.AddValue(cr);
            }

            valueDict["CR"].Add(cr);
            

            //文华整数算法不一样，需要向上
            value = ma1.REF((int)Math.Ceiling(_m1 / 2.5 + 1));
            valueDict["CRMA1"].Add(value);
            value = ma2.REF((int)Math.Ceiling(_m2 / 2.5 + 1));
            valueDict["CRMA2"].Add(value);
            value = ma3.REF((int)Math.Ceiling(_m3 / 2.5 + 1));
            valueDict["CRMA3"].Add(value);
            if (!IsSimpleMode)
            {
                graphDict["CRMA2"].AddValue(value, Color.Green);
                graphDict["CRMA1"].AddValue(value, Color.Yellow);
                graphDict["CR"].AddValue(cr, Color.White);
                graphDict["CRMA3"].AddValue(value, Color.Blue);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["CR"].RemoveAt(Count - 1);
            valueDict["CRMA1"].RemoveAt(Count - 1);
            valueDict["CRMA2"].RemoveAt(Count - 1);
            valueDict["CRMA3"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["CR"].RemoveLast();
                graphDict["CRMA1"].RemoveLast();
                graphDict["CRMA2"].RemoveLast();
                graphDict["CRMA3"].RemoveLast();
            }
            ma1.RemoveLast();
            ma2.RemoveLast();
            ma3.RemoveLast();
            MID.RemoveLast();
            CR_dn.RemoveLast();
            CR_up.RemoveLast();
            GenerateCR(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateCR(Count - 1);
            
            
        }

        public List<double> GetCRValues()
        {
            return valueDict["CR"];
        }

        public List<double> GetCRMA1Values()
        {
            return valueDict["CRMA1"];
        }

        public List<double> GetCRMA2Values()
        {
            return valueDict["CRMA2"];
        }

        public List<double> GetCRMA3Values()
        {
            return valueDict["CRMA3"];
        }

        public double GetCRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["CR"][index];
            else
                return JPR.NaN;
        }

        public double GetCRMA1Value(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["CRMA1"][index];
            else
                return JPR.NaN;
        }

        public double GetCRMA2Value(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["CRMA2"][index];
            else
                return JPR.NaN;
        }

        public double GetCRMA3Value(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["CRMA3"][index];
            else
                return JPR.NaN;
        }

        public double GetLastCR()
        {
            if (Count != 0)
                return valueDict["CR"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastCRMA1()
        {
            if (Count != 0)
                return valueDict["CRMA1"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastCRMA2()
        {
            if (Count != 0)
                return valueDict["CRMA2"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastCRMA3()
        {
            if (Count != 0)
                return valueDict["CRMA3"][Count - 1];
            else
                return JPR.NaN;
        }

        public int N { get { return _n; } }
        public int M1 { get { return _m1; } }
        public int M2 { get { return _m2; } }
        public int M3 { get { return _m3; } }
    }


}
