using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Zeipelin
{
    class MuQuadInterp
    {
        private double[][] intens = null;
        private double[] lambdas = null;
        private double[] mus = null;
        private double step;

        public MuQuadInterp(double[] lambdas, double[] mus, double[][] intens, double step)
        {
            this.lambdas = lambdas;
            this.mus = mus;
            this.intens = intens;
            this.step = step;
        }

        public double Interp(int r, double lambda, double mu)
        {
            /*int i;
            for (i = 1; i < lambdas.Length - 2; i++)
            {
                if (lambda <= lambdas[i + 1])
                {
                    break;
                }
            }*/

            int i = r + Convert.ToInt32((lambda - lambdas[r])/step) - 1;

            double Imu0 = intens[0][i] + (intens[0][i + 1] - intens[0][i]) / (lambdas[i + 1] - lambdas[i]) * (lambda - lambdas[i]);
            double Imu1 = intens[1][i] + (intens[1][i + 1] - intens[1][i]) / (lambdas[i + 1] - lambdas[i]) * (lambda - lambdas[i]);
            double Imu2 = intens[2][i] + (intens[2][i + 1] - intens[2][i]) / (lambdas[i + 1] - lambdas[i]) * (lambda - lambdas[i]);

            
            /*double a0, b0, c0;
            a0 = (intens[0][i + 1] - intens[0][i - 1]) / ((lambdas[i + 1] - lambdas[i - 1]) * (lambdas[i + 1] - lambdas[i])) - (intens[0][i] - intens[0][i - 1]) /
                    ((lambdas[i] - lambdas[i - 1]) * (lambdas[i + 1] - lambdas[i]));
            b0 = (intens[0][i] - intens[0][i - 1]) / (lambdas[i] - lambdas[i - 1]) - a0 * (lambdas[i] + lambdas[i - 1]);
            c0 = intens[0][i - 1] - b0 * lambdas[i - 1] - a0 * lambdas[i - 1] * lambdas[i - 1];
            double a1, b1, c1;
            a1 = (intens[1][i + 1] - intens[1][i - 1]) / ((lambdas[i + 1] - lambdas[i - 1]) * (lambdas[i + 1] - lambdas[i])) - (intens[1][i] - intens[1][i - 1]) /
                ((lambdas[i] - lambdas[i - 1]) * (lambdas[i + 1] - lambdas[i]));
            b1 = (intens[1][i] - intens[1][i - 1]) / (lambdas[i] - lambdas[i - 1]) - a1 * (lambdas[i] + lambdas[i - 1]);
            c1 = intens[1][i - 1] - b1 * lambdas[i - 1] - a1 * lambdas[i - 1] * lambdas[i - 1];
            double a2, b2, c2;
            a2 = (intens[2][i + 1] - intens[2][i - 1]) / ((lambdas[i + 1] - lambdas[i - 1]) * (lambdas[i + 1] - lambdas[i])) - (intens[2][i] - intens[2][i - 1]) /
                ((lambdas[i] - lambdas[i - 1]) * (lambdas[i + 1] - lambdas[i]));
            b2 = (intens[2][i] - intens[2][i - 1]) / (lambdas[i] - lambdas[i - 1]) - a2 * (lambdas[i] + lambdas[i - 1]);
            c2 = intens[2][i - 1] - b2 * lambdas[i - 1] - a2 * lambdas[i - 1] * lambdas[i - 1];

            double Imu0 = a0 * lambda * lambda + b0 * lambda + c0;
            double Imu1 = a1 * lambda * lambda + b1 * lambda + c1;
            double Imu2 = a2 * lambda * lambda + b2 * lambda + c2;*/


            double interp;
            if (mu < mus[2])
                interp = Imu2;
            else if (mu < mus[0])
            {
                double a, b, c;
                a = (Imu2 - Imu0) / ((mus[2] - mus[0]) * (mus[2] - mus[1])) - (Imu1 - Imu0) / ((mus[1] - mus[0]) * (mus[2] - mus[1]));
                b = (Imu1 - Imu0) / (mus[1] - mus[0]) - a * (mus[1] + mus[0]);
                c = Imu0 - b * mus[0] - a * mus[0] * mus[0];

                interp = a * mu * mu + b * mu + c;
            }
            else
                interp = Imu0;

            //Debug.WriteLine("int 1 = {0}, 2 = {1}, 3 = {2}", Imu0, Imu1, Imu2);
            //Debug.WriteLine(interp);

            return interp;
        }
    }
}
