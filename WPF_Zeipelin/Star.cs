using System;
using System.Diagnostics;

namespace Zeipelin
{
    class Star
    {
        private Ring[] partition;

        static public double beta_grav, T_pole, g_pole;
        static public double omega, mass, potential, inc;
        static public int n_phi_ring, n_phi_sub, n_beta_sub;

        static public double Re, Rp;

        const double G = 6.672e-8;

        public Star(double beta_grav, double T_pole, double g_pole, double omega, double mass, int n_phi_ring, int n_phi_sub,
            int n_beta_sub,  double inc)
        {
            Star.beta_grav = beta_grav;
            Star.T_pole = T_pole;
            Star.g_pole = g_pole;
            Star.omega = omega;
            Star.mass = mass;
            Star.n_phi_ring = n_phi_ring;
            Star.n_phi_sub = n_phi_sub;
            Star.n_beta_sub = n_beta_sub;
            Star.inc = inc;

            potential = Math.Sqrt(G * mass * g_pole);

            Re = GetR_eq();
            Rp = GetR_pole();

            RingPartition();
        }

        public Ring[] GetPartition() { return partition; }

        public double GetR_pole()
        {
            Ring pole = new Ring(0, 0);
            return pole.Get_R();
        }
        public double GetR_eq()
        {
            Ring eq = new Ring(90*Math.PI/180, 90 * Math.PI / 180);
            return eq.Get_R();
        }

        private void RingPartition()
        {
            double phi_cent, phi1, phi2;
            partition = new Ring[n_phi_ring];

            for (int i = 0; i < n_phi_ring; i++)
            {
                phi1 = i * Math.PI / n_phi_ring;
                phi2 = (i + 1) * Math.PI / n_phi_ring;
                phi_cent = phi1 + (phi2 - phi1) / 2;
                partition[i] = new Ring(phi1, phi2);
            }
        }
    }
}
