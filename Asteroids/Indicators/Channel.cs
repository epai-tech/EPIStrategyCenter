using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using EPI.Model;
using System.Drawing.Drawing2D;

namespace EPI.Asteroids.Indicators
{
    public class Channel:BaseIndicator
    {
        int _minSpan;
        int _mpe;
        double buyNumberA = 0, buyNumberB = 0;
        double sellNumberA = 0, sellNumberB = 0;
        bool isBuyUp = false;
        bool isSellUp = false;
        int checkCount = 0;

        List<PointF> buyLines = new List<PointF>();//买入线
        List<PointF> sellLines = new List<PointF>();//卖出线
        PointF pointA1 = new PointF();
        PointF pointA2 = new PointF();
        PointF pointA3 = new PointF();
        PointF pointB1 = new PointF();
        PointF pointB2 = new PointF();
        PointF pointB3 = new PointF();
        PointF pointCross = new PointF();

        BarData preBarData;
        private Color _lineColor;

        public Channel(List<BarData> barDatas, Color lineColor, int minSpan = 5, int mpe = 0, bool isSimpleMode = true, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _minSpan = minSpan;
            _mpe = mpe;
            _lineColor = lineColor;
            IsSimpleMode = isSimpleMode;
            string paramTag = string.Format("({0},{1})", _minSpan, _mpe);
            Name = string.Format("Channel{0}", paramTag);
            Description = "通道";
            valueDict.Add("Channel_U", new List<double>());
            valueDict.Add("Channel_D", new List<double>());
            //graphDict.Add("TRA", new IndicatorGraph() { Name = "TRA", Tag = tag, LineStyle = EnumLineStyle.SolidLine });
            IsShowInMain = true;
            Caculate();
        }

        public void SetParameters(Color lineColor, int minSpan = 5, int mpe = 0)
        {
            if (minSpan != _minSpan || mpe != _mpe || _lineColor != lineColor)
            {
                _lineColor = lineColor;
                buyLines.Clear();
                sellLines.Clear();
                UserLineList.Clear();
                ValueDict["Channel_U"].Clear();
                ValueDict["Channel_D"].Clear();
                buyNumberA = 0;
                buyNumberB = 0;
                sellNumberA = 0;
                sellNumberB = 0;
                pointA1.X = 0;
                pointA1.Y = 0;
                pointA2.X = 0;
                pointA2.Y = 0;
                pointA3.X = 0;
                pointA3.Y = 0;
                pointB1.X = 0;
                pointB1.Y = 0;
                pointB2.X = 0;
                pointB2.Y = 0;
                pointB3.X = 0;
                pointB3.Y = 0;
                pointCross.X = 0;
                pointCross.Y = 0;
                string paramTag = string.Format("({0},{1})", minSpan, mpe);
                Name = string.Format("Channel{0}", paramTag);
                _minSpan = minSpan;
                _mpe = mpe;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    GenerateUpChannel(i);
                }
            }
            base.Caculate();
        }

        void GenerateUpChannel(int i)
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
            if (buyNumberA != 0)
            {
                double buyValue = buyNumberA * i + buyNumberB;
                buyLines.Add(new PointF() { X = i, Y = (float)buyValue });
                ValueDict["Channel_U"].Add(buyValue);
                double sellValue = sellNumberA * i + sellNumberB;
                sellLines.Add(new PointF() { X = i, Y = (float)sellValue });
                ValueDict["Channel_D"].Add(sellValue);
            }
            else
            {
                buyLines.Add(new PointF());
                sellLines.Add(new PointF());
                ValueDict["Channel_U"].Add(0);
                ValueDict["Channel_D"].Add(0);
                if (pointA1.Y == 0 || pointB1.Y == 0)
                {
                    if (pointA1.Y == 0 && pointB1.Y == 0)
                    {
                        pointA1.X = i;
                        pointA1.Y = (float)curData.High;
                    }
                    else if (pointB1.Y == 0)
                    {
                        if (curData.High > pointA1.Y)
                        {
                            pointB1.X = pointA1.X;
                            pointB1.Y = (float)preData.Low;
                            pointA1.X = i;
                            pointA1.Y = (float)curData.High;
                        }
                        else
                        {
                            pointA1.X = i;
                            pointA1.Y = (float)curData.Low;
                        }
                    }
                }
                else
                {
                    if (pointA1.X < pointB1.X)//高点在前面
                    {
                        if (pointA2.Y == 0)
                        {
                            if (pointB1.Y > curData.Low)
                            {
                                pointB1.X = i;
                                pointB1.Y = (float)curData.Low;
                            }
                            else
                            {
                                if (i - pointA1.X > _minSpan)
                                {
                                    isBuyUp = curData.High > pointA1.Y;
                                    pointA2.X = i;
                                    pointA2.Y = (float)curData.High;
                                }
                            }
                        }
                        else if (pointB2.Y == 0)
                        {
                            if ((isBuyUp && curData.High > pointA2.Y) || (!isBuyUp && curData.High < pointA2.Y))
                            {
                                pointA2.X = i;
                                pointA2.Y = (float)curData.High;
                            }
                            else
                            {
                                if (i - pointB1.X > _minSpan)
                                {
                                    isSellUp = curData.Low > pointB1.Y;
                                    pointB2.X = i;
                                    pointB2.Y = (float)curData.Low;

                                    if (!((isBuyUp && isSellUp) || (!isBuyUp && !isSellUp)))
                                    {
                                        buyNumberA = 0;
                                        buyNumberB = 0;
                                        sellNumberA = 0;
                                        sellNumberB = 0;
                                        pointA1.X = 0;
                                        pointA1.Y = 0;
                                        pointA2.X = 0;
                                        pointA2.Y = 0;
                                        pointB1.X = 0;
                                        pointB1.Y = 0;
                                        pointB2.X = 0;
                                        pointB2.Y = 0;
                                        pointCross.X = 0;
                                        pointCross.Y = 0;
                                    }
                                }
                            }
                        }
                        else //if (pointA3.Y == 0)
                        {
                            if (checkCount == 0 && (isSellUp && curData.Low > pointB2.Y) || (!isSellUp && curData.Low < pointB2.Y))
                            {
                                pointB2.X = i;
                                pointB2.Y = (float)curData.Low;
                            }
                            else
                            {
                                //生成通道
                                CreateTriangle(ref buyNumberA, ref buyNumberB, ref sellNumberA, ref sellNumberB,
                                    ref pointA1, ref pointA2, ref pointB1, ref pointB2, ref pointCross);
                            }
                        }
                    }
                    else
                    {
                        if (pointB2.Y == 0)
                        {
                            if (pointA1.Y < curData.High)
                            {
                                pointA1.X = i;
                                pointA1.Y = (float)curData.High;
                            }
                            else
                            {
                                if (i - pointB1.X > _minSpan)
                                {
                                    isSellUp = curData.Low > pointB1.Y;
                                    pointB2.X = i;
                                    pointB2.Y = (float)curData.Low;
                                }
                            }

                        }
                        else if (pointA2.Y == 0)
                        {
                            if ((isSellUp && curData.Low > pointB2.Y) || (!isSellUp && curData.Low < pointB2.Y))
                            {
                                pointB2.X = i;
                                pointB2.Y = (float)curData.Low;
                            }
                            else
                            {
                                if (i - pointA1.X > _minSpan)
                                {
                                    isBuyUp = curData.High > pointA1.Y;
                                    pointA2.X = i;
                                    pointA2.Y = (float)curData.High;
                                    if (!((isBuyUp && isSellUp) || (!isBuyUp && !isSellUp)))
                                    {
                                        buyNumberA = 0;
                                        buyNumberB = 0;
                                        sellNumberA = 0;
                                        sellNumberB = 0;
                                        pointA1.X = 0;
                                        pointA1.Y = 0;
                                        pointA2.X = 0;
                                        pointA2.Y = 0;
                                        pointB1.X = 0;
                                        pointB1.Y = 0;
                                        pointB2.X = 0;
                                        pointB2.Y = 0;
                                        pointCross.X = 0;
                                        pointCross.Y = 0;
                                    }
                                }
                            }
                        }
                        else //if (pointA3.Y == 0)
                        {
                            if (checkCount == 0 && (isBuyUp && curData.High > pointA2.Y) || (!isBuyUp && curData.High < pointA2.Y))
                            {
                                pointA2.X = i;
                                pointA2.Y = (float)curData.High;
                            }
                            else
                            {
                                //生成通道
                                CreateTriangle(ref buyNumberA, ref buyNumberB, ref sellNumberA, ref sellNumberB,
                                    ref pointA1, ref pointA2, ref pointB1, ref pointB2, ref pointCross);
                            }
                        }
                    }
                }

            }
        }

        private void CreateTriangle(ref double buyNumberA, ref double buyNumberB, ref double sellNumberA,
            ref double sellNumberB, ref PointF pointA1, ref PointF pointA2, ref PointF pointB1, ref PointF pointB2, ref PointF pointCross)
        {
            if (checkCount == 0)
            {
                buyNumberA = (pointA2.Y - pointA1.Y) / (pointA2.X - pointA1.X);
                buyNumberB = pointA2.Y - buyNumberA * pointA2.X;

                sellNumberA = (pointB2.Y - pointB1.Y) / (pointB2.X - pointB1.X);
                sellNumberB = pointB2.Y - sellNumberA * pointB2.X;

                pointCross.X = (float)((sellNumberB - buyNumberB) / (buyNumberA - sellNumberA));
                pointCross.Y = (float)(sellNumberA * pointCross.X + sellNumberB);

                AddUserLine(pointA1, pointA2, _lineColor, DashStyle.Solid, 1, "Channel");
                AddUserLine(pointB1, pointB2, _lineColor, DashStyle.Solid, 1, "Channel");
            }

            checkCount = 5;

            if (checkCount == 5)
            {
                checkCount = 0;
                buyNumberA = 0;
                buyNumberB = 0;
                sellNumberA = 0;
                sellNumberB = 0;
                pointA1.X = 0;
                pointA1.Y = 0;
                pointA2.X = 0;
                pointA2.Y = 0;
                pointB1.X = 0;
                pointB1.Y = 0;
                pointB2.X = 0;
                pointB2.Y = 0;
                pointCross.X = 0;
                pointCross.Y = 0;
            }
        }

        public override void UpdateBarData(BarData bar)
        {
            //base.UpdateBarData(bar);
            //valueDict["TRA_B"].RemoveAt(Count - 1);
            //valueDict["TRA_S"].RemoveAt(Count - 1);
            //buyLines.RemoveAt(Count - 1);
            //sellLines.RemoveAt(Count - 1);
            if (preBarData != null && preBarData.RealDateTime == bar.RealDateTime)
                preBarData = bar;

            //GenerateTRA(Count - 1);
            

            
        }

        public override void AddBarData(BarData bar)
        {
            //base.AddBarData(bar);
            if (preBarData == null)
                preBarData = bar;
            if (preBarData.RealDateTime != bar.RealDateTime)
            {
                base.AddBarData(preBarData);
                GenerateUpChannel(Count - 1);
                preBarData = bar;
            }
            //GenerateTRA(Count - 1);
            
            
        }

        public double GetLastUp()
        {
            if (Count != 0)
                return valueDict["Channel_U"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastDown()
        {
            if (Count != 0)
                return valueDict["Channel_D"][Count - 1];
            else
                return JPR.NaN;
        }

        public List<double> GetUpValues()
        {
            return valueDict["Channel_U"];
        }

        public List<double> GetDownValues()
        {
            return valueDict["Channel_D"];
        }

        public double GetUpValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Channel_U"][index];
            else
                return JPR.NaN;
        }

        public double GetDownValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["Channel_D"][index];
            else
                return JPR.NaN;
        }

        public Color LineColor { get { return _lineColor; } }

        /// <summary>
        /// 最小间隔
        /// </summary>
        public int MinSpan
        {
            get { return _minSpan; }
        }

        /// <summary>
        /// 最大偏差值
        /// </summary>
        public int MPE
        {
            get { return _mpe; }
        }
    }
}
