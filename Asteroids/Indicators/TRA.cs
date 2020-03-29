using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EPI.Asteroids.Indicators
{
    public class TRA:BaseIndicator
    {
        int _minSpan;
        //int _mpe;
        int _minAngle;
        int _maxAngle;
        double maxHigh;
        double buyNumberA = 0, buyNumberB = 0;
        double sellNumberA = 0, sellNumberB = 0;

        List<BarData> signDataList;//信号数据
        List<PointF> buyLines = new List<PointF>();//买入线
        List<PointF> sellLines = new List<PointF>();//卖出线
        PointF pointA1 = new PointF();
        PointF pointA2 = new PointF();
        PointF pointB1 = new PointF();
        PointF pointB2 = new PointF();
        PointF pointCross = new PointF();
        private Color _lineColor;

        BarData preBarData;

        public TRA(List<BarData> barDatas, Color lineColor, int minSpan = 5, int minAngle = 10, int maxAngle = 50, bool isShowInMain = true, string tag = "1")
            : base(barDatas)
        {
            Tag = tag;
            _minSpan = minSpan;
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            string paramTag = string.Format("({0},{1},{2})", _minSpan, _minAngle, _maxAngle);
            Name = string.Format("TRA{0}", paramTag);
            Description = "三角形态";
            valueDict.Add("TRA_B", new List<double>());
            valueDict.Add("TRA_S", new List<double>());
            //graphDict.Add("TRA", new IndicatorGraph() { Name = "TRA", Tag = tag, LineStyle = EnumLineStyle.SolidLine });
            _lineColor = lineColor;
            //_mpe = mpe;
            IsShowInMain = true;
            signDataList = new List<BarData>();
            Caculate();
        }

        public void SetParameters(Color lineColor, int minSpan = 5, int minAngle = 10, int maxAngle = 50)
        {
            if (minSpan != _minSpan || minAngle != _minAngle || maxAngle != _maxAngle||_lineColor!=lineColor)
            {
                string paramTag = string.Format("({0},{1},{2})", minSpan, minAngle, maxAngle);
                Name = string.Format("TRA{0}", paramTag);
                _lineColor = lineColor;
                _minSpan = minSpan;
                _minAngle = minAngle;
                _maxAngle = maxAngle;
                Caculate();
            }
        }

        protected override void Caculate()
        {
            signDataList.Clear();
            buyLines.Clear();
            sellLines.Clear();
            UserLineList.Clear();
            ValueDict["TRA_B"].Clear();
            ValueDict["TRA_S"].Clear();
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
            if (barDatas != null && Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    maxHigh = Math.Max(maxHigh, barDatas[i].High);
                    GenerateTRA(i);
                }
            }
            base.Caculate();
        }

        void GenerateTRA(int i)
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
                ValueDict["TRA_B"].Add(buyValue);
                double sellValue = sellNumberA * i + sellNumberB;
                sellLines.Add(new PointF() { X = i, Y = (float)sellValue });
                ValueDict["TRA_S"].Add(sellValue);
            }
            else
            {
                buyLines.Add(new PointF());
                sellLines.Add(new PointF());
                ValueDict["TRA_B"].Add(0);
                ValueDict["TRA_S"].Add(0);
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
                            //检查低点
                            if (pointB1.Y > curData.Low)
                            {
                                pointB1.X = i;
                                pointB1.Y = (float)curData.Low;
                            }
                            else
                            {
                                //找下一高点
                                pointA2.X = i;
                                pointA2.Y = (float)curData.High;
                            }
                        }
                        else if (pointA2.X - pointA1.X < _minSpan)//检查A2 到 A1 之间距离
                        {
                            pointA2.X = i;
                            pointA2.Y = (float)curData.High;
                        }
                        else if (pointB2.Y == 0)
                        {
                            if (pointA2.Y > pointA1.Y || pointA2.Y < pointB1.Y || curData.High > pointA1.Y || curData.Low < pointB1.Y)
                            {
                                //将A2点作为A1点
                                if (pointA1.Y < pointA2.Y)
                                {
                                    pointA1.X = pointA2.X;
                                    pointA1.Y = pointA2.Y;
                                }
                                pointA2.X = 0;
                                pointA2.Y = 0;
                                //判断B1是否比当前数据低
                                if (pointA1.Y < curData.High)
                                {
                                    pointA1.X = i;
                                    pointA1.Y = (float)curData.High;
                                }
                                else if (pointB1.Y > curData.Low)
                                {
                                    pointB1.X = i;
                                    pointB1.Y = (float)curData.Low;
                                }
                            }
                            else if (pointA2.Y < curData.High)
                            {
                                pointA2.X = i;
                                pointA2.Y = (float)curData.High;
                            }
                            else
                            {
                                //找下一低点
                                pointB2.X = i;
                                pointB2.Y = (float)curData.Low;
                            }
                        }
                        else
                        {
                            if (pointB2.Y < pointB1.Y || pointB2.Y > pointA2.Y || curData.High > pointA2.Y || curData.Low < pointB1.Y)
                            {
                                //将B2点作为B1点,A2点作为A1点
                                //if (pointA1.Y < pointA2.Y)
                                //{
                                //    pointA1.X = pointA2.X;
                                //    pointA1.Y = pointA2.Y;
                                //}
                                //pointA2.X = 0;
                                //pointA2.Y = 0;

                                //if (pointB1.Y > pointB2.Y)
                                //{
                                //    pointB1.X = pointB2.X;
                                //    pointB1.Y = pointB2.Y;
                                //}
                                //pointB2.X = 0;
                                //pointB2.Y = 0;

                                ////判断B1是否比当前数据低
                                //if (pointA1.Y < curData.High)
                                //{
                                //    pointA1.X = i;
                                //    pointA1.Y = (float)curData.High;
                                //}
                                //else if (pointB1.Y > curData.Low)
                                //{
                                //    pointB1.X = i;
                                //    pointB1.Y = (float)curData.Low;
                                //}
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
                            else if (pointB2.Y > curData.Low)
                            {
                                pointB2.X = i;
                                pointB2.Y = (float)curData.Low;
                            }
                            else
                            {
                                //确定2条线,判断A3
                                CreateTriangle(curData, ref buyNumberA, ref buyNumberB, ref sellNumberA, ref sellNumberB, 
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
                                //找下一低点
                                pointB2.X = i;
                                pointB2.Y = (float)curData.Low;
                            }
                        }
                        else if (pointA2.Y == 0)
                        {
                            if (pointB2.Y > pointA1.Y || pointB2.Y < pointB1.Y || curData.High > pointA1.Y || curData.Low < pointB1.Y)
                            {
                                ////将B2点作为B1点
                                //if (pointB1.Y > pointB2.Y)
                                //{
                                //    pointB1.X = pointB2.X;
                                //    pointB1.Y = pointB2.Y;
                                //}
                                //pointB2.X = 0;
                                //pointB2.Y = 0;

                                ////判断a1是否比当前数据高
                                //if (pointA1.Y < curData.High)
                                //{
                                //    pointA1.X = i;
                                //    pointA1.Y = (float)curData.High;
                                //}
                                //else if (pointB1.Y > curData.Low)
                                //{
                                //    pointB1.X = i;
                                //    pointB1.Y = (float)curData.Low;
                                //}

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
                            else if (pointB2.Y > curData.Low)
                            {
                                pointB2.X = i;
                                pointB2.Y = (float)curData.Low;
                            }
                            else
                            {
                                //找下一高点
                                pointA2.X = i;
                                pointA2.Y = (float)curData.High;
                            }
                        }
                        else if (pointA2.X - pointA1.X < _minSpan)//检查A2 到 A1 之间距离
                        {
                            pointA2.X = i;
                            pointA2.Y = (float)curData.High;
                        }
                        else
                        {
                            if (pointA2.Y > pointA1.Y || pointA2.Y < pointB2.Y || curData.High > pointA1.Y || curData.Low < pointB2.Y)
                            {
                                //将B2点作为B1点,A2点作为A1点
                                if (pointA1.Y < pointA2.Y)
                                {
                                    pointA1.X = pointA2.X;
                                    pointA1.Y = pointA2.Y;
                                }
                                pointA2.X = 0;
                                pointA2.Y = 0;

                                if (pointB1.Y > pointB2.Y)
                                {
                                    pointB1.X = pointB2.X;
                                    pointB1.Y = pointB2.Y;
                                }
                                pointB2.X = 0;
                                pointB2.Y = 0;
                                //判断a1是否比当前数据高
                                if (pointA1.Y < curData.High)
                                {
                                    pointA1.X = i;
                                    pointA1.Y = (float)curData.High;
                                }
                                else if (pointB1.Y > curData.Low)
                                {
                                    pointB1.X = i;
                                    pointB1.Y = (float)curData.Low;
                                }
                            }
                            else if (pointA2.Y < curData.High)
                            {
                                pointA2.X = i;
                                pointA2.Y = (float)curData.High;
                            }
                            else
                            {
                                //确定2条线
                                CreateTriangle(curData, ref buyNumberA, ref buyNumberB, ref sellNumberA, ref sellNumberB, 
                                    ref pointA1, ref pointA2, ref pointB1, ref pointB2, ref pointCross);
                                
                            }
                        }
                    }
                }
            }
        }

        private void CreateTriangle(BarData curData, ref double buyNumberA, ref double buyNumberB, ref double sellNumberA, 
            ref double sellNumberB, ref PointF pointA1, ref PointF pointA2, ref PointF pointB1, ref PointF pointB2, ref PointF pointCross)
        {
            buyNumberA = (pointA2.Y - pointA1.Y) / (pointA2.X - pointA1.X);
            buyNumberB = pointA2.Y - buyNumberA * pointA2.X;

            sellNumberA = (pointB2.Y - pointB1.Y) / (pointB2.X - pointB1.X);
            sellNumberB = pointB2.Y - sellNumberA * pointB2.X;

            pointCross.X = (float)((sellNumberB - buyNumberB) / (buyNumberA - sellNumberA));
            pointCross.Y = (float)(sellNumberA * pointCross.X + sellNumberB);

            double tan = (buyNumberA + sellNumberA) / (1 - buyNumberA * sellNumberA);
            if (tan < 0)
            {
                PointF point1=Point2D(pointA1);
                PointF point2=Point2D(pointA2);
                PointF point3=Point2D(pointB1);
                PointF point4=Point2D(pointB2);
                double buyLine = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                double sellLine = Math.Atan2(point4.Y - point3.Y, point4.X - point3.X);
                if (buyLine < 0)
                    buyLine = Math.PI - Math.Abs(buyLine);

                double angle = 180 - (buyLine - sellLine) * 180 / Math.PI;
                if (angle >= _minAngle && angle <= _maxAngle)
                {
                    AddUserLine(pointA1, pointCross, _lineColor, DashStyle.Solid, 1, "TRA");
                    AddUserLine(pointB1, pointCross, _lineColor, DashStyle.Solid, 1, "TRA");
                    signDataList.Add(curData);
                }
            }
            //AddUserLine(pointA1, pointA2, "TRA");
            //AddUserLine(pointB1, pointB2, "TRA");
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

        public PointF Point2D(PointF pt)
        {
            PointF aPoint = new PointF();
            aPoint.X = pt.X * 1920 / Count;
            aPoint.Y = pt.Y * 1080 / (float)maxHigh;
            return aPoint;
        }

        //public double Angle(PointF point1, PointF point2, PointF point3)
        //{
        //    PointF cen, first, second;
        //    List<PointF> pointList = new List<PointF>();
        //    pointList.Add(new PointF((float)Math.Log(point1.X), (float)Math.Log(point1.Y)));
        //    pointList.Add(new PointF((float)Math.Log(point2.X), (float)Math.Log(point2.Y)));
        //    pointList.Add(new PointF((float)Math.Log(point3.X), (float)Math.Log(point3.Y)));
        //    pointList = pointList.OrderBy(p => p.X).ToList();
        //    first = pointList[0];
        //    second = pointList[2];
        //    cen = pointList[1];
        //    // const double M_PI = 3.1415926535897;

        //    double ma_x = first.X - cen.X;
        //    double ma_y = first.Y - cen.Y;
        //    double mb_x = second.X - cen.X;
        //    double mb_y = second.Y - cen.Y;
        //    double v1 = (ma_x * mb_x) + (ma_y * mb_y);
        //    double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
        //    double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
        //    double cosM = v1 / (ma_val * mb_val);
        //    double angleAMB = Math.Acos(cosM) * 180 / Math.PI;

        //    return angleAMB;
        //} 
        /// <summary>
        /// 获取信号数据
        /// </summary>
        /// <returns></returns>
        public List<BarData> GetSignDatas()
        {
            return signDataList;
        }


        public override void UpdateBarData(BarData bar)
        {
            maxHigh = Math.Max(maxHigh, bar.High);
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
            maxHigh = Math.Max(maxHigh, bar.High);
            //base.AddBarData(bar);
            if (preBarData == null)
                preBarData = bar;
            if (preBarData.RealDateTime != bar.RealDateTime)
            {
                base.AddBarData(preBarData);
                GenerateTRA(Count - 1);
                preBarData = bar;
            }
            //GenerateTRA(Count - 1);
            
            
        }

        public double GetTRABValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["TRA_B"][index];
            else
                return JPR.NaN;
        }

        public double GetTRASValue(int index)
        {
            if (index >= 0 && index < Count)
                return valueDict["TRA_S"][index];
            else
                return JPR.NaN;
        }

        public double GetLastTRA_B()
        {
            if (Count != 0)
                return valueDict["TRA_B"][Count - 1];
            else
                return JPR.NaN;
        }

        public double GetLastTRA_S()
        {
            if (Count != 0)
                return valueDict["TRA_S"][Count - 1];
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

        public int MinAngle
        {
            get { return _minAngle; }
        }

        public int MaxAngle
        {
            get { return _maxAngle; }
        }


        ///// <summary>
        ///// 最大偏差值
        ///// </summary>
        //public int MPE
        //{
        //    get { return _mpe; }
        //}




    }
}
