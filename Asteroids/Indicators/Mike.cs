using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class Mike : BaseIndicator
    {
        /* n 1 - 200 - 12
        TYP:=(HIGH+LOW+CLOSE)/3;//当根K线的最高值最低值收盘价的简单均值定义为TYP。
        LL:=LLV(LOW, N);//N个周期的最低值
        HH:=HHV(HIGH, N);//N个周期的最高值
        WR:TYP+(TYP-LL);
        MR:TYP+(HH-LL);
        SR:2*HH-LL;
        WS:TYP-(HH-TYP);
        MS:TYP-(HH-LL);
        SS:2*LL-HH;*/
        private int _n;
        private MinValue ll;
        private MaxValue hh;
        public Mike(List<BarData> bars, int n = 12, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})",n); 
            Name = string.Format("MIKE{0}", paramTag);
            Description = "麦克支撑压力:\r\n"
                          + "原理：\r\n"
                          + "MIKE指标是一种路径指标，依据Typical Price为计算基准，求其Weak、Medium、\r\n"
                          + "Strong三条带状支撑与压力，并且叠加到主图上。\r\n"
                          + "用法：\r\n"
                          + "1.Weak - s、Medium - s、Strong - s三条线代表初级、中级及强力支撑。\r\n"
                          + "2.Weak - r、Medium - r、Strong - r三条线代表初级、中级及强力压力。\r\n";

            //WR MR SR 分别为 初级，中级，强力 压力线
            valueDict.Add("WR", new List<double>());
            valueDict.Add("MR", new List<double>());
            valueDict.Add("SR", new List<double>());
            //WS MS SS 分别为 初级，中级，强力 支撑线
            valueDict.Add("WS", new List<double>());
            valueDict.Add("MS", new List<double>());
            valueDict.Add("SS", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("WR", new IndicatorGraph() { Name = "WR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("MR", new IndicatorGraph() { Name = "MR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("SR", new IndicatorGraph() { Name = "SR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                /**********************************/
                graphDict.Add("WS", new IndicatorGraph() { Name = "WS", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("MS", new IndicatorGraph() { Name = "MS", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("SS", new IndicatorGraph() { Name = "SS", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            //ll, hh 分别用于计算 过去k线的最小值，和最大值
            ll = new MinValue();
            hh = new MaxValue();

            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int n)
        {
            if (_n != n)
            {
                string paramTag = string.Format("({0})", n);
                Name = string.Format("MIKE{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["WR"].Tag = paramTag;
                    graphDict["MR"].Tag = paramTag;
                    graphDict["SR"].Tag = paramTag;
                    graphDict["WS"].Tag = paramTag;
                    graphDict["MS"].Tag = paramTag;
                    graphDict["SS"].Tag = paramTag;
                }

                _n = n;
                Caculate();
            }
        }

        public void Caculate()
        {
            valueDict["WR"].Clear();
            valueDict["MR"].Clear();
            valueDict["SR"].Clear();
            valueDict["WS"].Clear();
            valueDict["MS"].Clear();
            valueDict["SS"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["WR"].Clear();
                graphDict["MR"].Clear();
                graphDict["MR"].Clear();
                graphDict["WS"].Clear();
                graphDict["MS"].Clear();
                graphDict["SS"].Clear();
            }
            ll.SetParameters(_n);
            hh.SetParameters(_n);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateMIKE(i);
                }
            }
            base.Caculate();
        }

        private void GenerateMIKE(int i)
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
            double lvalue = ll.AddValue(curData.Low);
            double hvalue = hh.AddValue(curData.High);

            /*下面是阻力线与支撑线计算方法
             typ为中轴
              ss <  ms  <  ws  <---- typ ----<  wr  <  mr < sr
            */
            double typ = (curData.Low + curData.High + curData.Close) / 3;
            double wr = typ + (typ - lvalue);
            double mr = typ + (hvalue - lvalue);
            double sr = 2 * hvalue - lvalue;

            double ws = typ - (hvalue - typ);
            double ms = typ - (hvalue - lvalue);
            double ss = 2 * lvalue - hvalue;

            valueDict["WR"].Add(wr);
            valueDict["MR"].Add(mr);
            valueDict["SR"].Add(sr);
            valueDict["WS"].Add(ws);
            valueDict["MS"].Add(ms);
            valueDict["SS"].Add(ss);
            if (!IsSimpleMode)
            {
                graphDict["WR"].AddValue(wr, Color.Yellow);
                graphDict["MR"].AddValue(mr, Color.Red);
                graphDict["SR"].AddValue(sr, Color.Green);

                graphDict["WS"].AddValue(ws, Color.White);
                graphDict["MS"].AddValue(ms, Color.Red);
                graphDict["SS"].AddValue(ss, Color.Blue);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["WR"].RemoveAt(Count - 1);
            valueDict["MR"].RemoveAt(Count - 1);
            valueDict["SR"].RemoveAt(Count - 1);
            valueDict["WS"].RemoveAt(Count - 1);
            valueDict["MS"].RemoveAt(Count - 1);
            valueDict["SS"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["WR"].RemoveLast();
                graphDict["MR"].RemoveLast();
                graphDict["SR"].RemoveLast();
                graphDict["WS"].RemoveLast();
                graphDict["MS"].RemoveLast();
                graphDict["SS"].RemoveLast();
            }
            ll.RemoveLast();
            hh.RemoveLast();
            GenerateMIKE(Count - 1);

            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateMIKE(Count - 1);
            
            
        }

        //获得初级阻力线价格序列
        public List<double> GetWRValues()
        {
            return valueDict["WR"];
        }

        //获得中级阻力线价格序列
        public List<double> GetMRValues()
        {
            return valueDict["MR"];
        }

        //获得最强阻力线价格序列
        public List<double> GetSRValues()
        {
            return valueDict["SR"];
        }

        //获得初级支撑线价格序列
        public List<double> GetWSValues()
        {
            return valueDict["WS"];
        }

        //获得中级支撑线价格序列
        public List<double> GetMSValues()
        {
            return valueDict["MS"];
        }

        //获得最强支撑线价格序列
        public List<double> GetSSValues()
        {
            return valueDict["SS"];
        }

        //获得初级阻力线价格序列的最后价格
        public double GetWRLast()
        {
            if (Count != 0)
                return valueDict["WR"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得中级阻力线价格序列的最后价格
        public double GetMRLast()
        {
            if (Count != 0)
                return valueDict["MR"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得最强阻力线价格序列的最后价格
        public double GetSRLast()
        {
            if (Count != 0)
                return valueDict["SR"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得初级支撑线价格序列的最后价格
        public double GetWSLast()
        {
            if (Count != 0)
                return valueDict["WS"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得中级支撑线价格序列的最后价格
        public double GetMSLast()
        {
            if (Count != 0)
                return valueDict["MS"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得最强支撑线价格序列的最后价格
        public double GetSSLast()
        {
            if (Count != 0)
                return valueDict["SS"][Count - 1];
            else
                return JPR.NaN;
        }

        //获得初级压力线索引位置的价格
        public double GetWRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["WR"][index];
            else
                return JPR.NaN;
        }

        //获得中级压力线索引位置的价格
        public double GetMRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["WR"][index];
            else
                return JPR.NaN;
        }

        //获得最大压力线索引位置的价格
        public double GetSRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["WR"][index];
            else
                return JPR.NaN;
        }

        //获得初级支撑线索引位置的价格
        public double GetWSValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["WS"][index];
            else
                return JPR.NaN;
        }

        //获得中级支撑线索引位置的价格
        public double GetMSValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["MS"][index];
            else
                return JPR.NaN;
        }

        //获得最大支撑线索引位置的价格
        public double GetSSValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["SS"][index];
            else
                return JPR.NaN;
        }

        //用于界面设置指标参数
        public int N { get { return _n; } }  
    }
}
