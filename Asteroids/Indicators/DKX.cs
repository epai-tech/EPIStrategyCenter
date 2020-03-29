
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class DKX : BaseIndicator
    {
        //M 1-100-10 N2 1-100-6
        /*
        A:=(3*C+L+O+H)/6;
        B:(20*A+19*REF(A,1)+18*REF(A,2)+17*REF(A,3)+16*REF(A,4)+15*REF(A,5)+14*REF(A,6)+13*REF(A,7)+12*REF(A,8)+11*REF(A,9)+10*REF(A,10)+9*REF(A,11)+8*REF(A,12)+7*REF(A,13)+6*REF(A,14)+5*REF(A,15)+4*REF(A,16)+3*REF(A,17)+2*REF(A,18)+REF(A,20))/210;
        D:MA(B, M);
        CROSS(B, D),BPK;
        CROSS(D, B),SPK;
        AUTOFILTER;
        */
        private FixSeries a;
        private AverageS  d;
        private int _m;

        public DKX(List<BarData> bars, int m = 10, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _m = m;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", m);
            Name = string.Format("DKX{0}", paramTag);
            Description = "多空线\r\n"
                          + "原理：\r\n"
                          + "多空线（DKX）是一个统计性指标。它是将主动买、主动卖的成交按时间区间分别统计而形成的一个曲线。\r\n"
                          + "多空线有两条线，以交叉方式提示买入卖出。\r\n"
                          + "用法：\r\n"
                          + "（1) 当DKX指标在超卖区上穿其均线时，为买入信号；当DKX指标在超买区下穿其均线时，为卖出信号。\r\n"
                          + "（2）在DKX指标之上，为多头市场，采取做多策略；股价在DKX指标之下，为空头市场，采取做空策略";

            /*a 用于存储前20次计算的数据,A:=(3*C+L+O+H)/6;
              B 用于存储 短周期的加权平均线
              D 用于存储 短周期的加权平均线 再次 用 m个周期简单平均 平滑后的值
              d 用于零时计算移动平均
             */
            valueDict.Add("B", new List<double>());
            valueDict.Add("D", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("B", new IndicatorGraph() { Name = "B", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("D", new IndicatorGraph() { Name = "D", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }

            a = new FixSeries(20);
            d = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct, int m)
        {
            if ( m != _m )
            {
                string paramTag = string.Format("({0})", objBarStruct.ToString(), m);
                Name = string.Format("DKX{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["B"].Tag = paramTag;
                    graphDict["D"].Tag = paramTag;
                }
                _m = m;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["B"].Clear();
            valueDict["D"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["B"].Clear();
                graphDict["D"].Clear();
            }
            d.SetParameters(_m);
            a.SetParameters(20);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateDKX(i);
                }
            }
            base.Caculate();
        }

        private void GenerateDKX(int i)
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
            //计算 A:= (3 * C + L + O + H) / 6;
            double value = (3 * curData.Close + curData.Low + curData.Open + curData.High) / 6.0;
            a.AddValue(value);

            /*计算 B:(20*A+19*REF(A,1)+18*REF(A,2)+17*REF(A,3)+16*REF(A,4)+15*REF(A,5)+14*REF(A,6)+
                      13*REF(A,7)+12*REF(A,8)+11*REF(A,9)+10*REF(A,10)+9*REF(A,11)+8*REF(A,12)+7*REF(A,13)+
                      6*REF(A,14)+5*REF(A,15)+4*REF(A,16)+3*REF(A,17)+2*REF(A,18)+REF(A,20))/210;
              当前有20个数据的时候，B为有效值，否则B为NanN
              直到B为有效值的时候，才开始均线的计算.
            */
            double b,dvalue;
            if (i >= 20-1)
            {
                b = 0.0;
                for (int j = 0; j < 20; j++)
                {
                    b = b + (20 - j) * a.REF(j);
                }
                b = b / 210.0;
                dvalue = d.AddValue(b);
            } else
            {
                //数据不够，不将Nan放入移动平均线中进行计算,先必须加满20个数据
                b = JPR.NaN;
                dvalue = JPR.NaN;
            }

            valueDict["B"].Add(b);
            valueDict["D"].Add(dvalue);
            if (!IsSimpleMode)
            {
                graphDict["B"].AddValue(b, Color.Yellow);
                graphDict["D"].AddValue(dvalue, Color.Red);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["B"].RemoveAt(Count - 1);
            valueDict["D"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["B"].RemoveLast();
                graphDict["D"].RemoveLast();
            }
            d.RemoveLast();
            GenerateDKX(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateDKX(Count - 1);
            
            
        }

        //获得多空线中的快线的时间序列数据
        public List<double> GetBValues()
        {
            return valueDict["B"];
        }

        //获得多空线中的慢线的时间序列数据
        public List<double> GetDValues()
        {
            return valueDict["D"];
        }

        //获得多空线中的快线 index 索引位置的数据
        public double GetBValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["B"][index];
            else
                return JPR.NaN;
        }

        //获得多空线中的慢线 index 索引位置的数据
        public double GetDValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["D"][index];
            else
                return JPR.NaN;
        }

        //获得多空线中的快线 的最后数据
        public double GetLastB()
        {
            if (Count != 0)
                return valueDict["B"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得多空线中的慢线 的最后数据
        public double GetLastD()
        {
            if (Count != 0)
                return valueDict["D"][Count - 1];
            else
                return JPR.NaN;
        }

        //用于界面设置指标参数
        public int M { get { return _m; } }
    }
}
