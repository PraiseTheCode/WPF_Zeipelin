using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zeipelin
{
    class LinInterpolator
    {
        private double[] xSet = null;
        private double[] ySet = null;

        public LinInterpolator(double[] xSet, double[] ySet)
        {
            this.xSet = xSet;
            this.ySet = ySet;
        }

        public double Interp(double x)
        {
            double k = 0, b = 0;
            if (x <= this.xSet[0])
            {
                k = (this.ySet[1] - this.ySet[0]) / (this.xSet[1] - this.xSet[0]);
                b = this.ySet[0] - k * this.xSet[0];
                return k * x + b;
            }
            if (x >= this.xSet[this.xSet.Length - 1])
            {
                k = (this.ySet[this.ySet.Length - 1] - this.ySet[this.ySet.Length - 2])
                    / (this.xSet[this.xSet.Length - 1] - this.xSet[this.xSet.Length - 2]);
                b = this.ySet[this.ySet.Length - 1] - k * this.xSet[this.ySet.Length - 1];
                return k * x + b;
            }
            
            for (int i = 0; i < this.xSet.Length; i++)
            {
                if (xSet[i] <= x && xSet[i+1]>x )
                {
                    k = (this.ySet[i + 1] - this.ySet[i]) / (this.xSet[i + 1] - this.xSet[i]);
                    b = this.ySet[i + 1] - k * this.xSet[i + 1];
                    break;
                }
            }

            return k * x + b;
        }
    }
}
