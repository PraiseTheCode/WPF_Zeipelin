using System;
using System.Diagnostics;

namespace Zeipelin
{
    class Ring
    {
        private Sector[][] subpartition;
        private double[][][] normals;
        private bool[][] visibs;

        private double T_ring, g_ring, R_ring, Vrot_ring;
        private double phi_cent, phi1r, phi2r;

        private const double G = 6.672e-8;
        private const double SolarRadius = 69570000000;

        public Ring(double phi1r, double phi2r)
        {
            this.phi1r = phi1r;
            this.phi2r = phi2r;

            phi_cent = phi1r + (phi2r - phi1r) / 2;

            /*if (phi_cent * 180 / Math.PI < 6)
            {
                R_ring = Math.Sqrt(G * Star.mass / Star.g_pole);
            }
            else
            {
                R_ring = Calc_R();
            }*/
            R_ring = Calc_R();


            g_ring = Calc_g();
            T_ring = Calc_T();
            //Vrot_ring = Star.omega * R_ring * Math.Sin(phi_cent)*Math.Sin(Star.inc);
            Vrot_ring = 0;

            Subpartition(phi1r, phi2r);
            //Debug.WriteLine("!!!");
            //Debug.WriteLine(subpartition.Length);
            /*for (int i = 0; i < subpartition.Length; i++)
            {
                for (int j = 0; j < subpartition[0].Length; j++)
                {
                    Debug.WriteLine("i: {0}, j: {1}, phi: {2}, beta: {3}", i, j, subpartition[i][j].Get_PhiCent() * 180 / Math.PI, 
                        subpartition[i][j].Get_BetaCent() * 180 / Math.PI);
                }
            }*/
        }

        public double Get_R() { return R_ring; }
        public double Get_g() { return g_ring; }
        public double Get_T() { return T_ring; }
        public double Get_Vrot_ring() { return Vrot_ring; }
        public Sector[][] Get_Sub() { return subpartition; }
        public double Get_phi_cent() { return phi_cent; }
        public double[][][] Get_Normals() { return normals; }
        public bool[][] Get_Visibs() { return visibs; }

        private double REquation(double[] R)
        {
            return Math.Abs((G * Star.mass / R[0]) + (Star.omega * Star.omega * Math.Pow(Math.Sin(phi_cent), 2) * R[0] * R[0] / 2) - Star.potential);
        }

        private double Calc_R()
        {
            double R = 0, R0 = SolarRadius, eps = 1;

            int count = 0;
            while (eps > 0.001 || R < 0)
            {
                count++;
                R = R0 - ((G * Star.mass / R0) + (Star.omega * Star.omega * Math.Pow(Math.Sin(phi_cent), 2) * R0 * R0 / 2) - Star.potential) / (-G * Star.mass / R0 / R0
                    + Star.omega * Star.omega * Math.Pow(Math.Sin(phi_cent), 2) * R0);
                eps = Math.Abs((R - R0) / R);
                //Debug.WriteLine(eps);
                R0 = R;
            }

            //Debug.WriteLine(R);
            //Debug.WriteLine(count);

            return R;
        }

        private double Calc_g()
        {
            double g = Math.Sqrt(Math.Pow((G * Star.mass / R_ring / R_ring), 2) + Star.omega * Star.omega * Math.Pow(Math.Sin(phi_cent), 2)
                * R_ring * (Star.omega * Star.omega * R_ring - 2 * G * Star.mass / R_ring / R_ring));
            return g;
        }

        private double Calc_T()
        {
            double T = Star.T_pole * Math.Pow((g_ring / Star.g_pole), Star.beta_grav);
            return T;
        }

        private void Subpartition(double phi1r, double phi2r)
        {
            int n_ph = Star.n_phi_sub;
            int n_be = Star.n_beta_sub;

            subpartition = new Sector[n_ph][];
            normals = new double[n_ph][][];
            visibs = new bool[n_ph][];

            double h_phi = (phi2r - phi1r) / n_ph;
            double h_beta = 2 * Math.PI / n_be;
            double phi1, phi2, beta1, beta2;
            for (int i = 0; i < n_ph; i++)
            {
                phi1 = phi1r + i * h_phi;
                phi2 = phi1 + h_phi;
                subpartition[i] = new Sector[n_be];
                normals[i] = new double[n_be][];
                visibs[i] = new bool[n_be];
                for (int j = 0; j < n_be; j++)
                {
                    beta1 = j * h_beta;
                    beta2 = beta1 + h_beta;
                    subpartition[i][j] = new Sector(phi1, phi2, beta1, beta2);
                    visibs[i][j] = subpartition[i][j].Get_Visibility();
                }
            }
        }

        public bool PhiInRing(double phiii)
        {
            if (phiii >= phi1r && phiii < phi2r)
                return true;
            else return false;
        }
    }
}
