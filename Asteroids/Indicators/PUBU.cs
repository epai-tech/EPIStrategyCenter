
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class PUBU : BaseIndicator
    {
        /*
        M1 0-10-4
        M2 0-20-6
        M3 0-30-9
        M4 0-30-13
        M5 0-30-18
        M6 0-30-24

        PB1:(EMA(CLOSE, M1)+MA(CLOSE, M1*2)+MA(CLOSE, M1*4))/3;
        PB2:(EMA(CLOSE, M2)+MA(CLOSE, M2*2)+MA(CLOSE, M2*4))/3;
        PB3:(EMA(CLOSE, M3)+MA(CLOSE, M3*2)+MA(CLOSE, M3*4))/3;
        PB4:(EMA(CLOSE, M4)+MA(CLOSE, M4*2)+MA(CLOSE, M4*4))/3;
        PB5:(EMA(CLOSE, M5)+MA(CLOSE, M5*2)+MA(CLOSE, M5*4))/3;
        PB6:(EMA(CLOSE, M6)+MA(CLOSE, M6*2)+MA(CLOSE, M6*4))/3; //定义6条瀑布线
        */
        private EnumBarStruct barStruct;

        //存储各条线的参数大小
        private int[] m_array;

        //存储EMA(CLOSE, M1)
        private AverageE[] pb_ema_array;
        //存储 MA(CLOSE, M1*2)
        private AverageS[] pb_ma1_array;
        //存储 MA(CLOSE, M3*4)
        private AverageS[] pb_ma2_array;

        public PUBU(List<BarData> bars, EnumBarStruct objBarStruct = EnumBarStruct.Close, 
                      int m1 = 4, int m2 = 6, int m3 = 9,
                      int m4 = 13, int m5 = 18, int m6 = 24,
                      bool isSimpleMode = true,
                      bool isShowInMain = true, string tag = "")
            : base(bars)
        {
            Tag = tag;
            barStruct = objBarStruct;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1},{2},{3},{4},{5},{6})", barStruct.ToString(), m1, m2, m3, m4, m5, m6);
            Name = string.Format("PUBU{0}", paramTag);
            Description = "瀑布线:\r\n"
                         +"原理：\r\n"
                         +"瀑布线是由均线系统优化得来的趋势性指标，因其在运行的过程中，\r\n"
                         +"形态与瀑布极其相似，故得名为瀑布线。\r\n"
                         +"用法：\r\n"
                         +"1.瀑布线在低位粘合，短线瀑布线向上穿越长线瀑布线并向上发散，买入。\r\n"
                         +"2.瀑布线在高位粘合，短线瀑布线向下穿越长线瀑布线并向下发散，卖出。\r\n"
                         +"3.当长中短期瀑布线依次由下向上排列形成多头排列时，可持多单。\r\n"
                         +"4.当长中短期瀑布线依次由上向下排列形成空头排列时，可持空单。\r\n";
            pb_ema_array = new AverageE[6];
            pb_ma1_array = new AverageS[6];
            pb_ma2_array = new AverageS[6];
            m_array = new int[6] {m1,m2,m3,m4,m5,m6};

            for(int i = 0; i < 6; i++)
            {
                pb_ema_array[i] = new AverageE();
                pb_ma1_array[i] = new AverageS();
                pb_ma2_array[i] = new AverageS();

                //pbkey表示第几根线
                string pbkey = string.Format("PB{0}", i);
                //linetag 表示 这条线的参数
                string linetag = string.Format(",{0}", m_array[i]);  
                valueDict.Add(pbkey, new List<double>());
                if (!IsSimpleMode)
                {
                    graphDict.Add(pbkey, new IndicatorGraph() { Name = pbkey, Tag = linetag, LineStyle = EnumLineStyle.SolidLine });
                }
            }
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct, int m1, int m2, int m3, int m4, int m5, int m6)
        {
            if (objBarStruct != barStruct || m1 != m_array[0] || m2 != m_array[1] || m3 != m_array[2] || m4 != m_array[3] || m5 != m_array[4] || m6 != m_array[5])
            {
                int[] array = { m1, m2, m3, m4, m5, m6 };
                array.CopyTo(m_array, 0);
                string paramTag = string.Format("({0},{1},{2},{3},{4},{5},{6})", barStruct.ToString(), m1, m2, m3, m4, m5, m6);
                Name = string.Format("PUBU{0}", paramTag);
                for (int i = 0; i < 6; i++)
                {
                    string pbkey = string.Format("PB{0}", i);
                    string linetag = string.Format(",{0}", m_array[i]);
                    if (!IsSimpleMode)
                    {
                        graphDict[pbkey].Tag = linetag;
                    }
                }
                barStruct = objBarStruct;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            for (int i = 0; i < 6; i++)
            {
                pb_ema_array[i].SetParameters(m_array[i]);
                pb_ma1_array[i].SetParameters(2*m_array[i]);
                pb_ma2_array[i].SetParameters(4*m_array[i]);
                string pbkey = string.Format("PB{0}", i);
                valueDict[pbkey].Clear();
                if (!IsSimpleMode)
                {
                    graphDict[pbkey].Clear();
                }
            }

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GeneratePUBU(i);
                }
            }
            base.Caculate();
        }

        private void GeneratePUBU(int i)
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
            Color[] color_array = { Color.Yellow, Color.Blue, Color.Green, Color.Red, Color.Pink, Color.SaddleBrown };
            for (int j = 0; j < 6; j++)
            {
                double close = GetData(curData);
                //计算这个公式(EMA(CLOSE, M1)+MA(CLOSE, M1*2)+MA(CLOSE, M1*4))/3
                double lv = (pb_ema_array[j].Caculate(close) + pb_ma1_array[j].AddValue(close) + pb_ma2_array[j].AddValue(close)) / 3;
                string pbkey = string.Format("PB{0}", j);
                valueDict[pbkey].Add(lv);
                if (!IsSimpleMode)
                {
                    graphDict[pbkey].AddValue(lv, color_array[j]);
                }
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);

            for (int i = 0; i < 6; i++)
            {
                string pbkey = string.Format("PB{0}", i);
                valueDict[pbkey].RemoveAt(Count - 1);
                if (!IsSimpleMode)
                {
                    graphDict[pbkey].RemoveLast();
                }

                pb_ema_array[i].ResetValue();
                pb_ma1_array[i].RemoveLast();
                pb_ma2_array[i].RemoveLast();
            }

            GeneratePUBU(Count - 1);

           
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GeneratePUBU(Count - 1);
            
            
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

        //用于获得第i条线的所有时间序列值
        public List<double> GetValues(int lineidx)
        {
            string pbkey = string.Format("PB{0}", lineidx);
            return valueDict[pbkey];
        }

        //用于获得第i条线 索引为index的值
        public double GetValue(int lineidx,int index)
        {
            if (index >= 0 && index < Count)
            {
                string pbkey = string.Format("PB{0}", lineidx);
                return valueDict[pbkey][index];
            }
            else
                return JPR.NaN;
        }

        //用于获得第i条线的最后值
        public double GetLast(int i)
        {
            if (Count != 0  && i < 6) {
                string pbkey = string.Format("PB{0}", i);
                return valueDict[pbkey][Count - 1];
            }
            else
                return JPR.NaN;
        }

        //用于界面设置指标参数
        public int M1 { get { return m_array[0]; } }
        public int M2 { get { return m_array[1]; } }
        public int M3 { get { return m_array[2]; } }
        public int M4 { get { return m_array[3]; } }
        public int M5 { get { return m_array[4]; } }
        public int M6 { get { return m_array[5]; } }
        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
    }


}
