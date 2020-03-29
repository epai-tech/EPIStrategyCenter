using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using EPI.Asteroids.Functions;

namespace EPI.Asteroids.Indicators
{
    public class ATRGrid1:BaseIndicator
    {
        private int _length;
        private double _num;
        private double trValue;
        private double atrValue;
        private TrueRange tr;
        private AverageS atr;
        private double preUpLineValue;
        private double preDownLineValue;
        private double upLineValue;
        private double downLineValue;
        private int _theme;
        Color upColor;
        Color downColor;

        public ATRGrid1(List<BarData> bars, int length = 20, double num = 1.5, int theme = 1, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(bars)
        {
            Tag = tag;
            _length = length;
            IsSimpleMode = isSimpleMode;
            _num = num <= 0 ? 1 : num;
            string paramTag = string.Format("({0},{1})", _length, _num);
            Name = string.Format("ATRGrid1{0}", paramTag);
            Description = "波动网格1";
            valueDict.Add("UpLine", new List<double>());
            valueDict.Add("DownLine", new List<double>());
            if (!IsSimpleMode)
            {
                graphDict.Add("UpLine", new IndicatorGraph() { Name = "ATRUpLine", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
                graphDict.Add("DownLine", new IndicatorGraph() { Name = "ATRDownLine", Tag = paramTag, LineStyle = EnumLineStyle.SolidLine });
            }
            ChangeTheme(theme);
            tr = new TrueRange();
            atr = new AverageS();
            IsShowInMain = isShowInMain;
            Caculate();
        }

        public void SetParameters(int length, double num, int theme = 1)
        {
            if (length != _length || num != _num || theme != _theme)
            {
                ChangeTheme(theme);
                string paramTag = string.Format("({0},{1})", length, num);
                Name = string.Format("ATRGrid1{0}", paramTag);
                if (!IsSimpleMode)
                {
                    graphDict["UpLine"].Tag = paramTag;
                    graphDict["DownLine"].Tag = paramTag;
                }
                _length = length;
                _num = num <= 0 ? 1 : num;
                Caculate();
            }
        }

        private void ChangeTheme(int theme)
        {
            _theme = theme;
            upColor = Color.Red;
            downColor = Color.Green;
            if (theme == 2)
            {
                upColor = Color.OrangeRed;
                downColor = Color.OrangeRed;
            }
            else if (theme == 3)
            {
                upColor = Color.Cyan;
                downColor = Color.Cyan;
            }
        }

        protected override void Caculate()
        {
            valueDict["UpLine"].Clear();
            valueDict["DownLine"].Clear();
            if (!IsSimpleMode)
            {
                graphDict["UpLine"].Clear();
                graphDict["DownLine"].Clear();
            }
            atr.SetParameters(_length);
            preUpLineValue = 0;
            preDownLineValue = 0;
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateATRGrid(i);
                }
            }
            base.Caculate();
        }

        private void GenerateATRGrid(int i)
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
            if (i == 0)
            { trValue = (curData.High - curData.Low); }
            else
            { trValue = tr.Caculate(preData.Close, curData.High, curData.Low); }
            atrValue = atr.AddValue(trValue);
            if (i<_length-1)
            {
                upLineValue = JPR.NaN;
                downLineValue = JPR.NaN;
            }
            else if (i == _length-1)
            {
                upLineValue = CheckCeilingPrice(curData.Close, _num * atrValue);
                downLineValue = CheckFloorPrice(curData.Close, _num * atrValue);
            }
            else
            {
                if (curData.Close > preUpLineValue)
                {
                    upLineValue = CheckCeilingPrice(curData.Close, _num * atrValue);
                    downLineValue = CheckFloorPrice(curData.Close - _num * atrValue, _num * atrValue);
                }
                else if (curData.Close < preDownLineValue)
                {
                    upLineValue = CheckCeilingPrice(curData.Close + _num * atrValue, _num * atrValue);
                    downLineValue = CheckFloorPrice(curData.Close, _num * atrValue);
                }
            }   
            preUpLineValue=upLineValue;
            preDownLineValue=downLineValue;
            valueDict["UpLine"].Add(upLineValue);
            valueDict["DownLine"].Add(downLineValue);
            if (!IsSimpleMode)
            {
                graphDict["UpLine"].AddValue(upLineValue, upColor);
                graphDict["DownLine"].AddValue(downLineValue, downColor);
            }
        }

        //public override void UpdateBarData(BarData bar)
        //{
        //    base.UpdateBarData(bar);
        //    valueDict["UpLine"].RemoveAt(Count - 1);
        //    valueDict["DownLine"].RemoveAt(Count - 1);
        //    graphDict["UpLine"].RemoveLast();
        //    graphDict["DownLine"].RemoveLast();
        //    atr.RemoveLast();
        //    GenerateATRGrid(Count - 1);
        //    Dictionary<string, double> resultDict = new Dictionary<string, double>();
        //    resultDict.Add("UpLine", GetLastUpLine());
        //    resultDict.Add("DownLine", GetLastDownLine());
        //    
        //}

        public override void AddBarData(BarData bar)
        {
            base.AddBarData(bar);
            GenerateATRGrid(Count - 1);
            
        }

        public List<double> GetUpValues()
        {
            return valueDict["UpLine"];
        }

        public List<double> GetDownValues()
        {
            return valueDict["DownLine"];
        }

        public double GetUpValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["UpLine"][index];
            else
                return JPR.NaN;
        }

        public double GetDownValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["DownLine"][index];
            else
                return JPR.NaN;
        }

        public double GetLastUp()
        {
            if (Count != 0)
                return valueDict["UpLine"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastDown()
        {
            if (Count != 0)
                return valueDict["DownLine"][Count - 1];
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }

        public double Num
        {
            get { return _num; }
        }
        /// <summary>
        /// 主题颜色
        /// </summary>
        public int Theme { get { return _theme; } }

        public double CheckCeilingPrice(double nowPrice, double priceTick)
        {
            double result;
            if (nowPrice != JPR.NaN)
            {
                string[] num = Convert.ToString(nowPrice / priceTick).Split('.');
                double intNum = Convert.ToDouble(num[0]);
                double decNum = 0;
                if (num.Length != 1)
                {
                    { decNum = Convert.ToDouble("0." + num[1]); }
                    if (intNum >= 0)
                    {
                        result = nowPrice - decNum * priceTick + priceTick;
                    }
                    else
                    {
                        result = nowPrice + decNum * priceTick;
                    }
                }
                else
                { result = nowPrice; }
                return result;
            }
            else
            { return nowPrice; }
        }
        public double CheckFloorPrice(double nowPrice, double priceTick)
        {
            double result;
            if (nowPrice != JPR.NaN)
            {
                string[] num = Convert.ToString(nowPrice / priceTick).Split('.');
                double intNum = Convert.ToDouble(num[0]);
                double decNum = 0;
                if (num.Length != 1)
                {
                    decNum = Convert.ToDouble("0." + num[1]);
                    if (intNum >= 0)
                    {
                        result = nowPrice - decNum * priceTick;
                    }
                    else
                    {
                        result = nowPrice + decNum * priceTick - priceTick;
                    }
                }
                else
                { result = nowPrice; }
                return result;
            }
            else
            { return nowPrice; }
        }
    }
}
