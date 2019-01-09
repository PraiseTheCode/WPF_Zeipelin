using System;
using System.Diagnostics;

namespace Zeipelin
{
    class Sector
    {
        private const double G = 6.672e-8;
        private const double SolarRadius = 69570000000;
        private double phi1, phi2, beta1, beta2;
        private double phi_cent, beta_cent;
        private double vrot, r;
        private double cos_etha; // angle between normal and radius vector
        private double mu; // cos_gamma - angle between normal and line of sight 
        private bool visibility = false; // become true if mu > 0

        //phi1...______.......
        //....../......\......
        //...../........\.....
        //phi2/__________\....
        //beta1..........beta2

        public Sector(double phi1, double phi2, double beta1, double beta2)
        {
            this.phi1 = phi1;
            this.phi2 = phi2;
            this.beta1 = beta1;
            this.beta2 = beta2;

            phi_cent = phi1 + (phi2 - phi1) / 2;
            beta_cent = beta1 + (beta2 - beta1) / 2;

            r = Calc_R(phi_cent);
            mu = Mu();
            //vrot = Vrot();
            if (visibility)
                vrot = Vrot();
        }
        
        public double Get_Mu() { return mu; }
        public double Get_CosEtha() { return cos_etha; }
        public double Get_Vrot() { return vrot; }
        public double Get_PhiCent() { return phi_cent; }
        public double Get_BetaCent() { return beta_cent; }
        public bool Get_Visibility() { return visibility; }

        private double Mu()
        {
            double grad_r = G * Star.mass / r / r - Math.Pow(Star.omega, 2) * Math.Pow(Math.Sin(phi_cent), 2) * r;
            double grad_phi = -Math.Pow(Star.omega, 2) * Math.Sin(phi_cent) * Math.Cos(phi_cent) * r;

            double decart_x = Math.Sin(phi_cent) * Math.Cos(beta_cent) * grad_r + Math.Cos(phi_cent) * Math.Cos(beta_cent) * grad_phi;
            double decart_y = Math.Sin(phi_cent) * Math.Sin(beta_cent) * grad_r + Math.Cos(phi_cent) * Math.Sin(beta_cent) * grad_phi;
            double decart_z = Math.Cos(phi_cent) * grad_r - Math.Sin(phi_cent) * grad_phi;

            double norm = Math.Sqrt(decart_x * decart_x + decart_y * decart_y + decart_z * decart_z);
            double normed_x = decart_x / norm;
            double normed_z = decart_z / norm;

            double rotated_x = Math.Cos(Math.PI / 2 - Star.inc) * normed_x + Math.Sin(Math.PI / 2 - Star.inc) * normed_z;

            if (rotated_x > 0)
            {
                visibility = true;

                double normed_y = decart_y / norm;

                double rotated_z = -Math.Sin(Math.PI / 2 - Star.inc) * normed_x + Math.Cos(Math.PI / 2 - Star.inc) * normed_z;

                double rrx = Math.Cos(Math.PI / 2 - Star.inc) * Math.Sin(phi_cent) * Math.Cos(beta_cent) + Math.Sin(Math.PI / 2 - Star.inc) * Math.Cos(phi_cent);
                double rry = Math.Sin(phi_cent) * Math.Sin(beta_cent);
                double rrz = -Math.Sin(Math.PI / 2 - Star.inc) * Math.Sin(phi_cent) * Math.Cos(beta_cent) + Math.Cos(Math.PI / 2 - Star.inc) * Math.Cos(phi_cent);

                cos_etha = rotated_x * rrx + normed_y * rry + rotated_z * rrz;
            }

            return rotated_x;
        }

        private double Vrot()
        {
            return Star.omega * r * Math.Sin(Star.inc) * Math.Sin(beta_cent) * Math.Sin(phi_cent);
        }

        private double Calc_R(double phii)
        {
            double R = 0, R0 = 2*SolarRadius, eps = 1;

            int count = 0;
            while (eps > 0.001 || R < 0)
            {
                count++;
                R = R0 - ((G * Star.mass / R0) + (Star.omega * Star.omega * Math.Pow(Math.Sin(phii), 2) * R0 * R0 / 2) - Star.potential) / (-G * Star.mass / R0 / R0
                    + Star.omega * Star.omega * Math.Pow(Math.Sin(phii), 2) * R0);
                eps = Math.Abs((R - R0) / R);
                R0 = R;
            }
            
            return R;
        }

        public double Get_Area()
        {
            //double[] vect = { r * Math.Sin(phi_cent) * Math.Cos(beta_cent), r * Math.Sin(phi_cent) * Math.Sin(beta_cent), r * Math.Cos(phi_cent) };
            //vect = Rotate_inc(vect);

            //double drdf = (Calc_R(phi_cent + 0.001) - r) / 0.001;
            //double ar = Math.Sin(phi_cent) * r * Math.Sqrt(r * r - drdf * drdf) * (phi2 - phi1) * (beta2 - beta1);
            //Debug.WriteLine(ar);

            double ar = r * r * Math.Sin(phi_cent) * (phi2 - phi1) * (beta2 - beta1) / cos_etha;
            //double ar = Math.Sin(phi_cent) / cos_etha * (phi2 - phi1) * (beta2 - beta1);

            //ar *= mu;

            return ar;
        }

        private double[] Rotate_inc(double[] vector)
        {
            double[] vect = new double[3];
            vect[0] = Math.Cos(Math.PI / 2 - Star.inc) * vector[0] + Math.Sin(Math.PI / 2 - Star.inc) * vector[2];
            vect[1] = vector[1];
            vect[2] = -Math.Sin(Math.PI / 2 - Star.inc) * vector[0] + Math.Cos(Math.PI / 2 - Star.inc) * vector[2];
            return vect;
        }
    }
}
