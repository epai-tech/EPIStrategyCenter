
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class ARBR : BaseIndicator
    {
        /*   2 <26<300
        AR : SUM(HIGH-OPEN, N)/SUM(OPEN-LOW, N)*100;
        BR : SUM(MAX(0, HIGH-REF(CLOSE,1)),N)/SUM(MAX(0, REF(CLOSE,1)-LOW),N)*100;
        */
        private int _n;
        private Summation AR_up;
        private Summation AR_dn;
        private Summation BR_up;
        private Summation BR_dn;

        public ARBR(List<BarData> bars, int n = 26, bool isSimpleMode = true, bool isShowInMain = false, string tag = "")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0})", n);
            Name = string.Format("ARBR{0}", paramTag);
            Description = "人气意愿指标:\r\n"
                         + "原理：\r\n"
                         + "最近N个周期内最高价与开盘价的差的和除以开盘价与最低价的差的和，所得的比值放大100。\r\n"
                         + "用法：\r\n"
                         + "（1）介于60至120，盘整；过高，回落；过低，反弹。\r\n"
                         + "（2）BR意愿指标 \r\n"
                         + "原理："
                         + "最近N个周期内，若某日的最高价高于前一天的收盘价，将该日最高价与前收的差累加到强势和\r\n"
                         + "中，若某日的最低价低于前收，则将前收与该日最低价的差累加到弱势和中。最后用强势和除以弱\r\n"
                         + "势和，所得比值放大100。\r\n"
                         + "用法:\r\n"
                         + "介于70至150，盘整；高于400，回调；低于50，反弹。\r\n"
                         + "综合用法:\r\n"
                         + "（1）AR和BR同时急速上升，意味价格峰位已近，应注意及时获利了结。\r\n"
                         + "（2）BR比AR低，且指标处于低于100以下时，可考虑逢低买进。\r\n"
                         + "（3）BR从高峰回跌，跌幅达120时，若AR无警戒讯号出现，应逢低买进。\r\n"
                         + "（4）BR急速上升，AR盘整小回时，应逢高卖出，及时了结。\r\n";

            valueDict.Add("AR", new List<double>());
            valueDict.Add("BR", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("AR", new IndicatorGraph() { Name = "ARBR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("BR", new IndicatorGraph() { Name = "ARBR", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }

            AR_up = new Summation();
            AR_dn = new Summation();
            BR_up = new Summation();
            BR_dn = new Summation();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int n)
        {
            if (n != _n)
            {
                string paramTag = string.Format("({0})", n);
                Name = string.Format("ARBR{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["AR"].Tag = paramTag;
                    graphDict["BR"].Tag = paramTag;
                }
                _n = n;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["AR"].Clear();
            valueDict["BR"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["AR"].Clear();
                graphDict["BR"].Clear();
            }
            AR_up.SetParameters(_n);
            AR_dn.SetParameters(_n);
            BR_up.SetParameters(_n);
            BR_dn.SetParameters(_n);

            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateARBR(i);
                }
            }
            base.Caculate();
        }

        private void GenerateARBR(int i)
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
            /*   2 <26<300
            AR : SUM(HIGH-OPEN, N)/SUM(OPEN-LOW, N)*100;
            BR : SUM(MAX(0, HIGH-REF(CLOSE,1)),N)/SUM(MAX(0, REF(CLOSE,1)-LOW),N)*100;
            */
            double ar = JPR.NaN, br = JPR.NaN,ar_up = JPR.NaN,ar_dn=JPR.NaN, br_up = JPR.NaN, br_dn = JPR.NaN;
            if (i > 0)
            {
                ar_up = AR_up.AddValue(curData.High - curData.Open);
                ar_dn = AR_dn.AddValue(curData.Open - curData.Low);

                br_up = BR_up.AddValue(Math.Max(0, curData.High - preData.Close));
                br_dn = BR_dn.AddValue(Math.Max(0, preData.Close - curData.Low));
                if (i >= _n )
                {
                    if (ar_dn !=0)
                        ar = ar_up / ar_dn * 100;
                    if (br_dn != 0)
                        br = br_up / br_dn * 100;
                }
            }
            valueDict["AR"].Add(ar);
            valueDict["BR"].Add(br);
            if (!IsSimpleMode)
            {
                graphDict["AR"].AddValue(ar, Color.White);
                graphDict["BR"].AddValue(br, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["AR"].RemoveAt(Count - 1);
            valueDict["BR"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["AR"].RemoveLast();
                graphDict["BR"].RemoveLast();
            }

            AR_up.RemoveLast();
            AR_dn.RemoveLast();
            BR_up.RemoveLast();
            BR_dn.RemoveLast();

            GenerateARBR(Count - 1);
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateARBR(Count - 1);
            
        }

        public List<double> GetARValues()
        {
            return valueDict["AR"];
        }
        public List<double> GetBRValues()
        {
            return valueDict["BR"];
        }

        public double GetARValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["AR"][index];
            else
                return JPR.NaN;
        }
        public double GetBRValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["BR"][index];
            else
                return JPR.NaN;
        }

        public double GetLastAR()
        {
            if (Count != 0)
                return valueDict["AR"][Count - 1];
            else
                return JPR.NaN;
        }
        public double GetLastBR()
        {
            if (Count != 0)
                return valueDict["BR"][Count - 1];
            else
                return JPR.NaN;
        }

        //用于界面设置参数
        public int N { get { return _n; } }
    }
}
