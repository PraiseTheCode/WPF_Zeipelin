using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Zeipelin;
using System.Diagnostics;

namespace WPF_Zeipelin
{
    class Rotator3D
    {
        const double G = 6.672e-8;
        const double SolarRadius = 69570000000;
        const double SolarMass = 1.989e33;

        Point3D center;
        int n_phi, n_beta;
        Ring[] rings;
        Viewport3D viewport;

        double dT, Re;
        Color[] colors;
        double[] Tes;

        bool ifrings = false;

        public Rotator3D(Point3D center, int n_phi, int n_beta, Ring[] rings, Viewport3D viewport, bool ifrr)
        {
            this.center = center;
            this.n_phi = n_phi;
            this.n_beta = n_beta;
            this.rings = rings;
            this.viewport = viewport;
            this.ifrings = ifrr;

            dT = (rings[0].Get_T() - rings[rings.Length/2 - 1].Get_T()) / 64.0;
            SetColorsTes();
            Re = Calc_R(Math.PI / 2);
        }

        public void drawRotator()
        {
            if (n_phi < 2 || n_beta < 2)
            {
                return;
            }

            Model3DGroup rotator = new Model3DGroup();
            Point3D[,] points = new Point3D[n_beta + 1, n_phi + 1];

            double dphi = Math.PI / n_phi;
            double dbeta = Math.PI * 2 / n_beta;

            for (int i = 0; i < n_phi + 1; i++)
            {
                for (int j = 0; j < n_beta + 1; j++)
                {
                    points[j, i] = getPositionRotator(i * dphi, j * dbeta);
                    points[j, i] += (Vector3D)center;
                }
            }

            Color color;
            int nr;
            bool found;
            int nnnn = 0;
            int black = 0;
            Point3D[] p = new Point3D[4];
            for (int j = 0; j < n_phi; j++)
            {
                found = false;
                nr = 0;
                black = 0;
                while (!found)
                {
                    found = rings[nr].PhiInRing(j * dphi);
                    if (!found) nr++;
                }
                if (nr != nnnn) black = 1;
                nnnn = nr;
                double Te = rings[nr].Get_T();
                int nt = 0;
                for (int jj = 0; jj < Tes.Length-1; jj++)
                {
                    if (Te < Tes[jj])
                        nt++;
                }
                color = colors[nt];
                if (ifrings && black == 1) color = Color.FromArgb(255, 58, 17, 107);

                for (int i = 0; i < n_beta; i++)
                {
                    p[0] = points[i, j];
                    p[1] = points[i + 1, j];
                    p[2] = points[i + 1, j + 1];
                    p[3] = points[i, j + 1];
                    drawTriangle(p[0], p[1], p[2], color, viewport);
                    drawTriangle(p[2], p[3], p[0], color, viewport);
                }
            }
        }


        public Point3D getPositionRotator(double phii, double betai)
        {
            double r = Calc_R(phii) / Re;

            Point3D point = new Point3D();
            point.X = r * Math.Sin(phii) * Math.Cos(betai);
            point.Y = r * Math.Sin(phii) * Math.Sin(betai);
            point.Z = r * Math.Cos(phii);

            return point;
        }

        double Calc_R(double phii)
        {
            double R = 0, R0 = 2 * SolarRadius, eps = 1;

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

        public static void drawTriangle(
            Point3D p0, Point3D p1, Point3D p2, Color color, Viewport3D viewport)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = color;
            Material material = new DiffuseMaterial(brush);

            GeometryModel3D geometry = new GeometryModel3D(mesh, material);
            ModelUIElement3D model = new ModelUIElement3D();
            model.Model = geometry;
            viewport.Children.Add(model);
        }

        private void SetColorsTes()
        {
            colors = new Color[64];

            int r0, rn, dr, g0, gn, dg, b0, bn, db;
            r0 = 255; rn = 85; dr = (rn - r0) / 64;
            g0 = 255; gn = 115; dg = (gn - g0) / 64;
            b0 = 255; bn = 190; db = (bn - b0) / 64;
            byte r, g, b;
            for (int i = 0; i < 64; i++)
            {
                r = Convert.ToByte(r0 + i * dr);
                g = Convert.ToByte(g0 + i * dg);
                b = Convert.ToByte(b0 + i * db);
                colors[i] = Color.FromArgb(255, r, g, b);
            }

            Tes = new double[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                Tes[i] = rings[0].Get_T() - i * dT;
            }

        }
    }
}
