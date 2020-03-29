using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EPI.Model;

namespace EPI.Asteroids.Functions
{
    public class FixSeries : BaseFunction
    {   
        /*固定大小的序列类，为了性能考虑。代码大部分拷贝自AverageS;
          因为增加了REF接口，所以把代码写到FixSeries，希望将来独立为一个时间序列类
        */
        private int _length;
        private List<double> tmpList;

        public FixSeries(int length)
        {
            Name = "FixSeries";
            Description = "计算指标的时候，有时候不需要存储所有数据，\n比如只参考最近的几根k线，所以用到这个FixSeries 固定大小的序列类";
            tmpList = new List<double>();
            _length = length;
        }

        public void SetParameters(int length)
        {
            _length = length;
            tmpList.Clear();
        }

        public void Clear()
        {
            tmpList.Clear();
        }

        public double AddValue(double data)
        {
            if (tmpList.Count > 0 && tmpList.Count == _length)
            {
                //删除不需要的过去 Length 之前的那个元素,为了性能
                tmpList.RemoveAt(0);
            } else if (tmpList.Count > _length) {
                throw new Exception("计算数量超出范围");
            }
            tmpList.Add(data);
            return data;
        }

        public void RemoveLast()
        {
            tmpList.RemoveAt(tmpList.Count - 1);
        }

        public double REF(int index)
        {
            /*index = 0;表示取最近的值
              index = 1;表示取上根Bar的值    
            */
            if (index < tmpList.Count && index >= 0)
            {
                return tmpList[_length - index - 1];
            }
            else
                return JPR.NaN;
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
