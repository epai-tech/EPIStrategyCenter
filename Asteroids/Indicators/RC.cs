
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;
namespace EPI.Asteroids.Indicators
{
    public class RC : BaseIndicator
    {
        //RC:=CLOSE/REF(CLOSE, N);
        //ARC:SMA(REF(RC,1),N,1);
        private AverageS2 ARC;
        private FixSeries RCValue;
        private EnumBarStruct barStruct;
        private int _n;//50

        public RC(List<BarData> bars, EnumBarStruct objBarStruct = EnumBarStruct.Close, int n = 50, bool isSimpleMode = true, bool isShowInMain = false, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _n = n;
            IsSimpleMode = isSimpleMode;
            barStruct = objBarStruct;
            string paramTag = string.Format("({0},{1})", barStruct.ToString(), n);
            Name = string.Format("RC{0}", paramTag);
            Description = "变化率指标\r\n"
                         + "原理：\r\n"
                         + "当天收盘价与N天前收盘价的比值再进行移动平均。\r\n"
                         + "用法：\r\n"
                         + "如果价格始终在上升，则变化率指数RC始终能保持在1线以上，如果变化率指数RC继续向上发展\r\n"
                         + "，则说明价格上升的速度在加快。反之，如果价格始终下降，则RC在1以下，如果RC继续向下发展，\r\n"
                         + "则说明价格下降的速度在加快。\r\n";

            valueDict.Add("ARC", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("ARC", new IndicatorGraph() { Name = "ARC", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            ARC = new AverageS2();
            RCValue = new FixSeries(2);
            IsShowInMain = isShowInMain;
            MaxCacheCount = n * 2 < 10 ? 10 : n * 2;
            Caculate();
        }

        public void SetParameters(EnumBarStruct objBarStruct, int n)
        {
            if (objBarStruct != barStruct || n != _n)
            {
                string paramTag = string.Format("({0},{1})", objBarStruct.ToString(), n);
                Name = string.Format("RC{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["ARC"].Tag = paramTag;
                }
                barStruct = objBarStruct;
                _n = n;
                MaxCacheCount = n * 2 < 10 ? 10 : n * 2;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            valueDict["ARC"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["ARC"].Clear();
            }
            ARC.SetParameters(_n,1);
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateARC(i);
                }
            }
            base.Caculate();
        }

        private void GenerateARC(int i)
        {
            BarData curData = null;
            BarData preData = null;
            if (i >= barDatas.Count)
            {
                curData = GetBarData(0);
                preData = GetBarData(_n);
            }
            else
            {
                curData = barDatas[i];
                if (i >= _n && i < barDatas.Count)
                {
                    preData = barDatas[i - _n];
                }
            }
            //RC:=CLOSE/REF(CLOSE, N);
            //ARC:SMA(REF(RC,1),N,1);
            double rc = JPR.NaN, arc = JPR.NaN;
            if (i >= _n)
            {
                //这个时候，RC为有效值
                rc = GetData(curData) / GetData(preData);
                RCValue.AddValue(rc);
                if (i > _n)
                {
                    //此时，ARC为有效值
                    arc = ARC.Caculate(RCValue.REF(1));
                }
            }
            valueDict["ARC"].Add(arc);
            if (!IsSimpleMode)
            {
                graphDict["ARC"].AddValue(arc, Color.Yellow);
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            base.UpdateBarData(bar);
            valueDict["ARC"].RemoveAt(Count - 1);
            if (!IsSimpleMode)
            {
                graphDict["ARC"].RemoveLast();
            }
            RCValue.RemoveLast();
            ARC.ResetValue();
            GenerateARC(Count - 1);
            
            
        }

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateARC(Count - 1);
            
            
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

        public List<double> GetARCValues()
        {
            return valueDict["ARC"];
        }

        public double GetARCValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["ARC"][index];
            else
                return JPR.NaN;
        }

        public double GetARCLast()
        {
            if (Count != 0)
                return valueDict["ARC"][Count - 1];
            else
                return JPR.NaN;
        }

        //用于界面设置参数
        public int DataType
        {
            get { return barStruct.GetHashCode(); }
        }
        public int N { get { return _n; } }

    }


}
