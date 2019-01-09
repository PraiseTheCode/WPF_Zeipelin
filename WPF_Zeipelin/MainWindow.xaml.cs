using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zeipelin;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Data;
using OxyPlot;
using System.Data.SqlClient;
using Microsoft.Win32;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Threading;
using System.Windows.Threading;
using OxyPlot.Annotations;

namespace WPF_Zeipelin
{
    public class myRow
    {
        public string b1 { set; get; }
        public string b2 { set; get; }
        public string b3 { set; get; }
    }

    public partial class MainWindow : Window
    {
        //String separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0].ToString();
        static String separator = ".";

        const double SolarMass = 1.989e33;
        const double SolarRadius = 69570000000;
        const double c = 29979245800;
        const double G = 6.672e-8;

        double inc;
        int n_phi_ring, n_phi_sub, n_beta_sub;
        string ringInput;

        double Teff, lgg, Vrot, T_pole, g_pole, mass;
        double Ve = 0;

        double incView = 90*Math.PI/180;

        Rotator3D vega;

        int n_threads = Environment.ProcessorCount;
        double Resolution;

        const double mu1 = 0.8872983346;
        const double mu2 = 0.5;
        const double mu3 = 0.1127016654;
        readonly double[] mus = { mu1, mu2, mu3 };

        static public double[] lambdas_fin = null, fluxes_fin = null;
        double[] lambdas, fluxes_fin_cont, fluxes_fin_line;
        double[] lambdas_obs, fluxes_obs, lambdas_obs_moved;
        double[] OC;
        /*OxyColor[] colors_bik = { OxyColor.FromArgb(255,134,0,79), OxyColor.FromArgb(255,51,51,204),
            OxyColor.FromArgb(255,0,179,119),OxyColor.FromArgb(255,255,102,0),
            OxyColor.FromArgb(255,204,0,136),OxyColor.FromArgb(255,255,102,102),
            OxyColor.FromArgb(255,0,51,102),OxyColor.FromArgb(255,0,77,0),
            OxyColor.FromArgb(255,204,204,0),OxyColor.FromArgb(255,204,0,0)};*/

        OxyColor[] colors_bik = { OxyColor.FromArgb(255,204,0,51), OxyColor.FromArgb(255,0,102,204),
            OxyColor.FromArgb(255,102,153,51),OxyColor.FromArgb(255,153,51,153),
            OxyColor.FromArgb(255,51,204,153),OxyColor.FromArgb(255,255,153,0),
            OxyColor.FromArgb(255,153,102,204),OxyColor.FromArgb(255,0,153,153),
            OxyColor.FromArgb(255,204,204,0),OxyColor.FromArgb(255,204,0,0),

            OxyColor.FromArgb(255,204,0,51), OxyColor.FromArgb(255,0,102,204),
            OxyColor.FromArgb(255,102,153,51),OxyColor.FromArgb(255,153,51,153),
            OxyColor.FromArgb(255,51,204,153),OxyColor.FromArgb(255,255,153,0),
            OxyColor.FromArgb(255,153,102,204),OxyColor.FromArgb(255,0,153,153),
            OxyColor.FromArgb(255,204,204,0),OxyColor.FromArgb(255,204,0,0)};

        int color_counter = 0;

        Ring[] rings;

        double step;
        int n_lamb;
        double lambda1, lambda_step;

        double Vturb, FWHM, Met;

        PlotModel plotModel, plotModel1, plotModel2, plotModel3, plotModel4;
        LineSeries lineSerie, lineSerie2, lineSerie3, lineSerie4;
        ScatterSeries sct1;

        BackgroundWorker worker;
        
        delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

        public MainWindow()
        {
            InitializeComponent();

            txtNThreads.Text = n_threads.ToString();

            cbMet.SelectedIndex = 6;

            Chemicals elems = new Chemicals();
            
            try
            {

                string[] allLines = System.IO.File.ReadAllLines("dabund.dat");
                List<myRow> myDataItems = new List<myRow>();
                for (int r = 0; r < allLines.Length; r++)
                {
                    string[] mmm = allLines[r].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string r1 = elems.get_name(Convert.ToInt32(mmm[0]));
                    string r2 = mmm[0];
                    string r3 = mmm[1];

                    myDataItems.Add(new myRow() { b1 = r1, b2 = r2, b3 = r3 });
                }
                dataGridView1.ItemsSource = myDataItems;
            }
            catch
            {
                string[] allLines = System.IO.File.ReadAllLines("dabund.dat");
                List<myRow> myDataItems = new List<myRow>();
                for (int r = 0; r < allLines.Length - 1; r++)
                {
                    string[] mmm = allLines[r].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string r1 = elems.get_name(Convert.ToInt32(mmm[0]));
                    string r2 = mmm[0];
                    string r3 = mmm[1];

                    myDataItems.Add(new myRow() { b1 = r1, b2 = r2, b3 = r3 });
                }
                dataGridView1.ItemsSource = myDataItems;
            }

            plotModel = new PlotModel
            {
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            LinearAxis xAxis = new LinearAxis();
            xAxis.Position = AxisPosition.Bottom;
            LinearAxis yAxis = new LinearAxis();
            yAxis.Position = AxisPosition.Left;
            LinearAxis xAxis1 = new LinearAxis();
            xAxis1.Position = AxisPosition.Bottom;
            LinearAxis yAxis1 = new LinearAxis();
            yAxis1.Position = AxisPosition.Left;
            LinearAxis xAxis2 = new LinearAxis();
            xAxis2.Position = AxisPosition.Bottom;
            LinearAxis yAxis2 = new LinearAxis();
            yAxis2.Position = AxisPosition.Left;
            LinearAxis xAxis3 = new LinearAxis();
            xAxis3.Position = AxisPosition.Bottom;
            LinearAxis yAxis3 = new LinearAxis();
            yAxis3.Position = AxisPosition.Left;
            LinearAxis xAxis4 = new LinearAxis();
            xAxis4.Position = AxisPosition.Bottom;
            LinearAxis yAxis4 = new LinearAxis();
            yAxis4.Position = AxisPosition.Left;
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);
            plotModel1 = new PlotModel
            {
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            plotModel1.Axes.Add(xAxis1);
            plotModel1.Axes.Add(yAxis1);
            plotModel2 = new PlotModel
            {
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            plotModel2.Axes.Add(xAxis2);
            plotModel2.Axes.Add(yAxis2);
            plotModel3 = new PlotModel
            {
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            plotModel3.Axes.Add(xAxis3);
            plotModel3.Axes.Add(yAxis3);
            plotModel4 = new PlotModel
            {
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            plotModel4.Axes.Add(xAxis4);
            plotModel4.Axes.Add(yAxis4);

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            startEvals();
        }

        private void btn_GraphNewWin_Click(object sender, RoutedEventArgs e)
        {
            WindowModel win2 = new WindowModel();
            win2.Show();

            if (chb_Black.IsChecked == false)
            {
                if (n_phi_sub * n_phi_ring * n_beta_sub <= 1000)
                    vega = new Rotator3D(new Point3D(0, 0, 0), n_phi_sub * n_phi_ring, n_beta_sub, rings, win2.mainViewport2, false);
                else vega = new Rotator3D(new Point3D(0, 0, 0), 60, 60, rings, win2.mainViewport2, false);
            }
            else
            {
                vega = new Rotator3D(new Point3D(0, 0, 0), 180, 60, rings, win2.mainViewport2, true);
            }
            vega.drawRotator();
            double incl3d2 = Math.Sin(inc) * (-2.5);
            double incl3d3 = Math.Cos(inc) * (-2.5);
            OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, 1), 2.5);
            win2.mainViewport2.Camera = myOCamera;
        }

        private void rbVe_Checked(object sender, RoutedEventArgs e)
        {
            if (txtVe != null)
            {
                txtVe.IsEnabled = true;
                txtW.IsEnabled = false;
            }
        }

        private void rbW_Checked(object sender, RoutedEventArgs e)
        {
            txtW.IsEnabled = true;
            txtVe.IsEnabled = false;
        }

        private void btnCalcModel_Click(object sender, RoutedEventArgs e)
        {
            lbRings.Items.Clear();
            mainViewport.Children.Clear();
            mainViewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });
            double incl3d2 = Math.Sin(incView) * (-2.5);
            double incl3d3 = Math.Cos(incView) * (-2.5);
            OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, 1), 2.5);
            mainViewport.Camera = myOCamera;

            double beta_grav = 0.25;

            Debug.WriteLine(separator);
            
            try
            {
                T_pole = Convert.ToDouble(txtTPole.Text.Replace(".", separator).Replace(",", separator));
                g_pole = Math.Pow(10, Convert.ToDouble(txtLoggPole.Text.Replace(".", separator).Replace(",", separator)));
                mass = Convert.ToDouble(txtMass.Text.Replace(".", separator).Replace(",", separator)) * SolarMass;
                n_phi_ring = Convert.ToInt32(txtNRings.Text.Replace(".", separator).Replace(",", separator));
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат ввода в блоке star model. Попробуйте снова.");
                return;
            }
            if ((n_phi_ring % (2 * n_threads)) != 0)
            {
                MessageBox.Show("Число колец должно быть кратным удвоенному числу потоков. Попробуйте снова.");
                return;
            }
            try
            {
                inc = Convert.ToDouble(txtInc.Text.Replace(".", separator).Replace(",", separator)) * Math.PI / 180;
                //n_phi_sub = Convert.ToInt32(180 / n_phi_ring / (Convert.ToDouble(txtDPhi.Text.Replace(".", separator).Replace(",", separator))));
                //n_beta_sub = Convert.ToInt32(360 / (Convert.ToDouble(txtDBeta.Text.Replace(".", separator).Replace(",", separator))));
                n_phi_sub = Convert.ToInt32(txtDPhi.Text.Replace(",", separator).Replace(".", separator));
                n_beta_sub = Convert.ToInt32(txtDBeta.Text.Replace(",", separator).Replace(".", separator));
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат ввода в блоке star model. Попробуйте снова.");
                return;
            }
            
            double omega = 0;
            if ((bool)rbVe.IsChecked)
            {
                Ve = Convert.ToDouble(txtVe.Text.Replace(".", separator).Replace(",", separator)) * 1e5;
                double Re0 = G * mass / (Math.Sqrt(G * mass * g_pole) - (Ve * Ve / 2.0));
                omega = Ve / Re0;
                txtW.Text = Convert.ToString(omega);
            }
            else
            {
                omega = Convert.ToDouble(txtW.Text.Replace(".", separator).Replace(",", separator));
            }

            Star test1 = new Star(beta_grav, T_pole, g_pole, omega, mass, n_phi_ring, n_phi_sub, n_beta_sub, inc);

            string str;
            string title = "phi" + "\t" + "R" + "\t" + "lg(g)" + "\t" + "T";
            lbRings.Items.Add(title);

            rings = test1.GetPartition();
            Ring ring;

            lambdas = new double[n_lamb];
            for (int l = 0; l < n_lamb; l++)
                lambdas[l] = lambda1 + l * step;

            if (File.Exists("param.dat")) File.Delete("param.dat");
            string pars1 = string.Format("  {0}     {1} \n", n_phi_ring / 2, n_threads);
            StreamWriter sw = new StreamWriter("param.dat", true, System.Text.Encoding.Default);
            sw.Write(pars1.Replace(",", "."));

            for (int i = 0; i < n_phi_ring / 2; i++)
            {
                ring = rings[i];

                str = Convert.ToString(Math.Round(ring.Get_phi_cent() * 180 / Math.PI, 1)) + "\t" +
                    Convert.ToString(Math.Round(ring.Get_R() / SolarRadius, 3)) + "\t" +
                    Convert.ToString(Math.Round(Math.Log10(ring.Get_g()), 4)) + "\t" +
                    Convert.ToString(Math.Round(ring.Get_T(), 1));
                lbRings.Items.Add(str);

                Teff = ring.Get_T();
                lgg = Math.Log10(ring.Get_g());

                if (Teff < 10000)
                    ringInput = string.Format(" {0:0} {1:0.000}  {2:0.000}", Teff, lgg, Vturb);
                else
                    ringInput = string.Format("{0:0} {1:0.000}  {2:0.000}", Teff, lgg, Vturb);

                sw.WriteLine(ringInput.Replace(",", "."));
                Debug.WriteLine(ringInput);
            }
            sw.Close();
            if (chb_Black.IsChecked == false)
            {
                if (n_phi_sub * n_phi_ring * n_beta_sub <= 1000)
                    vega = new Rotator3D(new Point3D(0, 0, 0), n_phi_sub * n_phi_ring, n_beta_sub, rings, mainViewport,false);
                else vega = new Rotator3D(new Point3D(0, 0, 0), 60, 60, rings, mainViewport, false);
            }
            else
            {
                vega = new Rotator3D(new Point3D(0, 0, 0), 180, 60, rings, mainViewport, true);
            }
            vega.drawRotator();
            btnUp.IsEnabled = true;
            btnDown.IsEnabled = true;
            btnInc.IsEnabled = true;
            btnSpec.IsEnabled = true;
            btn_GraphNewWin.IsEnabled = true;
        }
        
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            incView -= 10 * Math.PI / 180;

            double incl3d2 = Math.Sin(incView) * (-2.5);
            double incl3d3 = Math.Cos(incView) * (-2.5);

            if (incView <= Math.PI && incView > 0 || incView > 2 * Math.PI && incView <= 3 * Math.PI)
            {
                OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, 1), 2.5);
                mainViewport.Camera = myOCamera;
            }
            else
            {
                OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, -1), 2.5);
                mainViewport.Camera = myOCamera;
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            incView += 10 * Math.PI / 180;

            double incl3d2 = Math.Sin(incView) * (-2.5);
            double incl3d3 = Math.Cos(incView) * (-2.5);

            if (incView <= Math.PI && incView > 0 || incView > 2*Math.PI && incView <= 3*Math.PI)
            {
                OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, 1), 2.5);
                mainViewport.Camera = myOCamera;
            }
            else
            {
                OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, -1), 2.5);
                mainViewport.Camera = myOCamera;
            }
        }

        private void btnInc_Click(object sender, RoutedEventArgs e)
        {
            double incl3d2 = Math.Sin(inc) * (-2.5);
            double incl3d3 = Math.Cos(inc) * (-2.5);

            OrthographicCamera myOCamera = new OrthographicCamera(new Point3D(0, incl3d2, incl3d3), new Vector3D(0, -incl3d2, -incl3d3), new Vector3D(0, 0, 1), 2.5);
            mainViewport.Camera = myOCamera;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            inc = Convert.ToDouble(txtInc.Text.Replace(".", separator).Replace(",", separator)) * Math.PI / 180;
            Resolution = Convert.ToDouble(txtResPow.Text.Replace(",", separator).Replace(".", separator));

            bool VALD = false;
            if (slider_linelist.Value == 1) VALD = true;

            if (VALD)
            {
                File.Copy("C:/kurucmod/gfsint_VALD/gfsint.dat", "C:/kurucmod/gfsint.dat", true);
                Debug.WriteLine("VALD linelist activated");
            }
            else
            {
                File.Copy("C:/kurucmod/gfsint_old_shim/gfsint.dat", "C:/kurucmod/gfsint.dat", true);
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            dataGridView1.SelectAllCells();
            ApplicationCommands.Copy.Execute(null, dataGridView1);
            dataGridView1.UnselectAllCells();
            File.WriteAllText("dabund.dat", Clipboard.GetText(TextDataFormat.CommaSeparatedValue));
            
            string[] allLines = System.IO.File.ReadAllLines("dabund.dat");
            string newalllines = "";

            for (int r = 0; r < allLines.Length - 1; r++)
            {
                string[] mmm = allLines[r].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (r < 3)
                    newalllines += ("    " + mmm[1] + "  " + mmm[2] + "\r\n");
                else newalllines += ("   " + mmm[1] + "  " + mmm[2] + "\r\n");
            }
            File.WriteAllText("dabund.dat", newalllines);

            int flag = 0;
            try
            {
                //Met = Convert.ToDouble(txtMe.Text.Replace(".", separator).Replace(",", separator));
                Vturb = Convert.ToDouble(txtVturb.Text.Replace(".", separator).Replace(",", separator));
                //FWHM = Convert.ToDouble(txtFWHM.Text.Replace(".", separator).Replace(",", separator));
                n_lamb = Convert.ToInt32(txtNLambda.Text.Replace(".", separator).Replace(",", separator)) + 100;
                lambda_step = Convert.ToDouble(txtDLambda.Text.Replace(".", separator).Replace(",", separator));
                step = lambda_step;
                lambda1 = Convert.ToDouble(txtLambda0.Text.Replace(".", separator).Replace(",", separator)) - 50 * step;
            }
            catch (FormatException)
            {
                MessageBox.Show("Неверный формат ввода в блоке spectrum. Попробуйте снова.");
                return;
            }

            Met = Math.Pow(10, Convert.ToDouble(Convert.ToString(cbMet.SelectedItem).Replace(".", separator).Replace(",", separator)));

            if (File.Exists("start.dat")) File.Delete("start.dat");
            string starts = string.Format(" -3.010    {0:0.00000}" + "\n" + "{1}  {2:0.000} {3:0.0000}", Met, n_lamb, lambda1, lambda_step);
            using (StreamWriter sw2 = new StreamWriter("start.dat", false, System.Text.Encoding.Default))
            {
                sw2.Write(starts.Replace(",", "."));
            }

            if (File.Exists("fluxlall.dat")) File.Delete("fluxlall.dat");
            if (File.Exists("fluxcall.dat")) File.Delete("fluxcall.dat");
            Process proc = new Process();
            proc.StartInfo.FileName = "_vega.exe";
            proc.Start();
            proc.WaitForExit();

            try
            {
                using (StreamReader sr = new StreamReader("fluxlall.dat"))
                {
                    string[] s = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] mmm;

                    for (int j = 0; j < 1; j++)
                    {
                        int i = 0;
                        mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);

                        double testtt = Convert.ToDouble(mmm[3 * i + 3].Replace(".", separator).Replace(",", separator));
                    }
                }
                using (StreamReader sr = new StreamReader("fluxcall.dat"))
                {
                    string[] s = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] mmm;

                    for (int j = 0; j < 1; j++)
                    {
                        int i = 0;
                        mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                        double testtt = Convert.ToDouble(mmm[3 * i + 3].Replace(".", separator).Replace(",", separator));

                        //if (cont_intens_mu1[j] < line_intens_mu1[j] || cont_intens_mu2[j] < line_intens_mu2[j] || cont_intens_mu3[j] < line_intens_mu3[j])
                        //{
                        //    Debug.WriteLine("!!!!!! ring: {0}, j: {1}", i, j);
                        //}
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Что-то не устроило модуль Шиманского. Проверьте параметры в разделе spectrum и попробуйте снова.");
                return;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Что-то не устроило модуль Шиманского! Проверьте параметры в разделе spectrum и попробуйте снова.");
                return;
            }

            lambdas = new double[n_lamb];
            for (int l = 0; l < n_lamb; l++)
                lambdas[l] = lambda1 + l * step;

            prBar.Maximum = n_phi_ring * n_phi_sub / 2 ;
            prBar.Value = 0;
            worker.RunWorkerAsync();
            while (worker.IsBusy)
            {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            }
            if ((bool)(ch_idents.IsChecked))
            {
                //using (StreamReader sr = new StreamReader("vald.dat"))
                using (StreamReader sr = new StreamReader("vald.dat"))
                {
                    string[] s = sr.ReadToEnd().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] mmm;
                    double mlambd, mdepth;
                    string mident;

                    for (int j = 3; j < s.Length; j++)
                    {
                        mmm = s[j].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (mmm.Length == 1) break;

                        mlambd = Convert.ToDouble(mmm[1]) * 10;

                        if ((mlambd > lambdas_fin[0]) && (mlambd < lambdas_fin[lambdas_fin.Length - 1]))
                        {
                            mdepth = Convert.ToDouble(mmm[9]);
                            mident = mmm[0].Substring(1, mmm[0].Length - 2);
                            int ionn = Convert.ToInt16(""+mident[mident.Length - 1]);
                            mident = mident.Substring(0, mident.Length - 2);
                            if (ionn == 1) mident += " I";
                            if (ionn == 2) mident += " II";
                            if (ionn == 3) mident += " III";
                            if (ionn == 4) mident += " IV";
                            if (ionn == 5) mident += " V";
                            if (ionn == 6) mident += " VI";

                            plotModel.Annotations.Add(new LineAnnotation { Type = LineAnnotationType.Vertical, X = mlambd, MaximumY = 1, Color = OxyColors.Gray, Text = mident, TextPadding = 0, TextOrientation = AnnotationTextOrientation.Vertical });
                            
                        }
                    }

                    plotView1.InvalidatePlot(true);
                }
            }

            byte n1, n2, n3;
            if (tab.SelectedIndex == 1 || tab.SelectedIndex == 0)
            {
                n1 = 65;
                n2 = 29;
                n3 = 96; 
            }
            else
            {
                n1 = 85;
                n2 = 115;
                n3 = 190;
            }
            lineSerie = new LineSeries
            {
                StrokeThickness = 2,
                CanTrackerInterpolatePoints = false,
                Smooth = false,
                Color = OxyColor.FromArgb(255, n1, n2, n3)
            };
            

            plotModel.Series.Add(lineSerie);

            if (tab.SelectedIndex == 0)
            {
                PlotView.Model = plotModel;
                try
                {
                    plotModel.Series.Clear();
                }
                catch { }
                for (int i = 0; i < lambdas_fin.Length; i++)
                {
                    lineSerie.Points.Add(new DataPoint(lambdas_fin[i], fluxes_fin[i]));
                }
                plotModel.Series.Add(lineSerie);
                PlotView.InvalidatePlot(true);

                btnSaveSimpleGraph.IsEnabled = true;
                btnSave1.IsEnabled = false;
            }
            else
            {
                plotView1.Model = plotModel;
                lineSerie = new LineSeries
                {
                    StrokeThickness = 1,
                    CanTrackerInterpolatePoints = false,
                    Smooth = false,
                    Color = colors_bik[color_counter]
                };
                for (int i = 0; i < lambdas_fin.Length; i++)
                {
                    lineSerie.Points.Add(new DataPoint(lambdas_fin[i], fluxes_fin[i]));
                }
                plotModel.Series.Add(lineSerie);
                plotView1.InvalidatePlot(true);
                color_counter += 1;

                try { lineSerie3.Points.Clear(); }
                catch { };
                lineSerie3 = new LineSeries
                {
                    StrokeThickness = 2,
                    CanTrackerInterpolatePoints = false,
                    Smooth = false,
                    Color = OxyColor.FromArgb(255, 65, 29, 96)
                };
                plotModel3.Series.Add(lineSerie3);
                plotView3.Model = plotModel3;
                for (int i = 0; i < lambdas_fin.Length; i++)
                {
                    lineSerie3.Points.Add(new DataPoint(lambdas_fin[i], fluxes_fin[i]));
                }
                plotView3.InvalidatePlot(true);

                btnSaveSimpleGraph.IsEnabled = false;
                btnSave1.IsEnabled = true;

                if (lambdas_obs != null)
                {
                    OC = new double[lambdas_obs.Length];
                    LinInterpolator li = new LinInterpolator(lambdas_fin, fluxes_fin);
                    double[] flux_th_obs = new double[lambdas_obs.Length];
                    for (int i = 0; i < lambdas_obs.Length; i++)
                        flux_th_obs[i] = li.Interp(lambdas_obs[i]);

                    for (int i = 0; i < lambdas_obs.Length; i++)
                        OC[i] = flux_th_obs[i] - fluxes_obs[i];

                    try { lineSerie4.Points.Clear(); }
                    catch { };
                    lineSerie4 = new LineSeries
                    {
                        StrokeThickness = 2,
                        CanTrackerInterpolatePoints = false,
                        Smooth = false,
                        Color = OxyColor.FromArgb(255, 65, 29, 96)
                    };
                    for (int i = 0; i < OC.Length; i++)
                        lineSerie4.Points.Add(new DataPoint(lambdas_obs[i], OC[i]));
                    plotModel4.Series.Add(lineSerie4);
                    plotView4.Model = plotModel4;
                    plotView4.InvalidatePlot();
                    txtChi2.Text = String.Format("{0:0.000e0}", Get_Chi2());

                    btnSave4.IsEnabled = true;
                    btnSavetxtOC.IsEnabled = true;
                }

                
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Debug.WriteLine("RunTime " + elapsedTime);

            btnSave.IsEnabled = true;

            if (VALD)
            {
                File.Copy("C:/kurucmod/gfsint_old_shim/gfsint.dat", "C:/kurucmod/gfsint.dat", true);
                Debug.WriteLine("VALD linelist deactivated");
            }
        }

        private void btnSaveSimpleGraph_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.ShowDialog();
            string fileName = sf.FileName;
            using (var stream = File.Create(fileName))
            {
                var pdfExporter = new PdfExporter { Width = 600, Height = 400 };
                pdfExporter.Export(plotModel, stream);
            }
        }


        private void startEvals()
        {
            UpdateProgressBarDelegate updProgress = new UpdateProgressBarDelegate(prBar.SetValue);

            double[] line_flux = new double[n_lamb - 100];
            double[] cont_flux = new double[n_lamb - 100];
            for (int q = 0; q < n_lamb - 100; q++)
            {
                line_flux[q] = 0;
                cont_flux[q] = 0;
            }

            Stopwatch stopWatchPF = new Stopwatch();
            stopWatchPF.Start();

            double value = 0;
            Parallel.For(0, n_phi_ring / 2, i =>
            {

                Debug.WriteLine("Calc {0} ring...", i);
                double[] line_intens_mu1, line_intens_mu2, line_intens_mu3, cont_intens_mu1, cont_intens_mu2, cont_intens_mu3;

                cont_intens_mu1 = new double[n_lamb];
                cont_intens_mu2 = new double[n_lamb];
                cont_intens_mu3 = new double[n_lamb];
                line_intens_mu1 = new double[n_lamb];
                line_intens_mu2 = new double[n_lamb];
                line_intens_mu3 = new double[n_lamb];

                try
                {
                    using (StreamReader sr = new StreamReader("fluxlall.dat"))
                    {
                        string[] s = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        string[] mmm;

                        for (int j = 0; j < s.Length; j++)
                        {
                            mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                            lambdas[j] = Convert.ToDouble(mmm[0].Replace(".", separator).Replace(",", separator));
                            line_intens_mu1[j] = Convert.ToDouble(mmm[3 * i + 1].Replace(".", separator).Replace(",", separator));
                            line_intens_mu2[j] = Convert.ToDouble(mmm[3 * i + 2].Replace(".", separator).Replace(",", separator));
                            line_intens_mu3[j] = Convert.ToDouble(mmm[3 * i + 3].Replace(".", separator).Replace(",", separator));
                        }
                    }
                    using (StreamReader sr = new StreamReader("fluxcall.dat"))
                    {
                        string[] s = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        string[] mmm;

                        for (int j = 0; j < s.Length; j++)
                        {
                            mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                            cont_intens_mu1[j] = Convert.ToDouble(mmm[3 * i + 1].Replace(".", separator).Replace(",", separator));
                            cont_intens_mu2[j] = Convert.ToDouble(mmm[3 * i + 2].Replace(".", separator).Replace(",", separator));
                            cont_intens_mu3[j] = Convert.ToDouble(mmm[3 * i + 3].Replace(".", separator).Replace(",", separator));

                            //if (cont_intens_mu1[j] < line_intens_mu1[j] || cont_intens_mu2[j] < line_intens_mu2[j] || cont_intens_mu3[j] < line_intens_mu3[j])
                            //{
                            //    Debug.WriteLine("!!!!!! ring: {0}, j: {1}", i, j);
                            //}
                        }
                    }
                }
                catch
                {
                    MessageBox.Show(String.Format("Что-то не устроило модуль Шиманского в {0} кольце.", i));
                    return;
                }

                double[] line_flux_ring = new double[n_lamb - 100];
                double[] cont_flux_ring = new double[n_lamb - 100];

                Ring ring = rings[i];
                Ring ring2 = rings[n_phi_ring - i - 1];

                Sector[][] parts = ring.Get_Sub();
                bool[][] viss = ring.Get_Visibs();

                for (int r = 0; r < n_lamb - 100; r++)
                {
                    line_flux_ring[r] = 0;
                    cont_flux_ring[r] = 0;

                    //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", lambdas[r], cont_intens_mu1[r], cont_intens_mu2[r], cont_intens_mu3[r], line_intens_mu1[r], line_intens_mu2[r], line_intens_mu3[r]);
                }

                Sector[][] sectors = ring.Get_Sub();

                double[][] intens_line = new double[3][];
                intens_line[0] = line_intens_mu1;
                intens_line[1] = line_intens_mu2;
                intens_line[2] = line_intens_mu3;
                double[][] intens_cont = new double[3][];
                intens_cont[0] = cont_intens_mu1;
                intens_cont[1] = cont_intens_mu2;
                intens_cont[2] = cont_intens_mu3;

                int counter = 0;
                double mu, area, Vrot_s;
                //MuLinInterp mulin_cont = new MuLinInterp(lambdas, mus, intens_cont, step);
                //MuLinInterp mulin_line = new MuLinInterp(lambdas, mus, intens_line, step);
                MuQuadInterp mulin_cont = new MuQuadInterp(lambdas, mus, intens_cont, step);
                MuQuadInterp mulin_line = new MuQuadInterp(lambdas, mus, intens_line, step);

                //StreamWriter test2 = new StreamWriter("grid.txt");
                //StreamWriter test3 = new StreamWriter("interps_test.txt");

                //StreamWriter test4 = new StreamWriter("interps_tests.txt");

                for (int j = 0; j < n_phi_sub; j++)
                {

                    Dispatcher.Invoke(updProgress, new object[] { ProgressBar.ValueProperty, ++value });

                    double lambda_d;
                    double line_intensjk, cont_intensjk;
                    double[] line_intenss_mu, cont_intenss_mu;
                    line_intenss_mu = new double[3];
                    cont_intenss_mu = new double[3];

                    double a;
                    string strtt = "\t";
                    for (int t = 0; t < Convert.ToInt32((0.975 - 0.025) / 0.05); t++)
                    {
                        a = 0.025 + 0.05 * t;
                        strtt += Convert.ToString(a) + "\t";
                    }

                    for (int k = 0; k < n_beta_sub; k++)
                    {
                        //Sector sector = sectors[j][k];
                        //Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", sector.Get_PhiCent() * 180 / Math.PI, sector.Get_BetaCent() * 180 / Math.PI, sector.Get_Mu(), Math.Acos(sector.Get_Mu()) * 180 / Math.PI, sector.Get_Vrot(), sector.Get_Area());
                        //Debug.WriteLine("{0} for sector {1}, {2},  ring 1", viss[j][k], sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI);
                        if (viss[j][k])
                        {
                            mu = sectors[j][k].Get_Mu();
                            area = sectors[j][k].Get_Area();
                            Vrot_s = sectors[j][k].Get_Vrot();
                            //Debug.WriteLine("mu {0} for sector {1} {2}", mu, sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI);

                            //Debug.WriteLine("area {0} for sector {1} {2}", area, sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI);
                            for (int r = 50; r < n_lamb - 50; r++)
                            {
                                lambda_d = lambdas[r] * (1 + Vrot_s / c);

                                line_intensjk = mulin_line.Interp(r, lambda_d, mu);
                                cont_intensjk = mulin_cont.Interp(r, lambda_d, mu);
                                line_flux_ring[r - 50] += area * mu * line_intensjk;
                                cont_flux_ring[r - 50] += area * mu * cont_intensjk;

                                if (i == 3)
                                {
                                    if (j == 5)
                                    {
                                        if (k == 5)
                                        {
                                            //test4.WriteLine(strtt);
                                            //Debug.WriteLine(strtt.Replace(",", "."));

                                            strtt = string.Format("{0:0.00}    ", lambdas[r]);
                                            double[] intsss = new double[Convert.ToInt32((0.975 - 0.025) / 0.05)];
                                            for (int t = 0; t < Convert.ToInt32((0.975 - 0.025) / 0.05); t++)
                                            {
                                                intsss[t] = mulin_line.Interp(r, lambdas[r], (0.025 + 0.05 * t));
                                                strtt += string.Format("{0:0.000e0}    ", intsss[t]);
                                            }

                                            //test4.WriteLine(strtt.Replace(",", "."));
                                            //Debug.WriteLine(strtt.Replace(",", "."));

                                            string strr = string.Format("{0:0.00}    {1:0.000e0}    {2:0.000e0}    {3:0.000e0}", lambdas[r], line_intens_mu1[r], line_intens_mu2[r], line_intens_mu3[r]);
                                            //test2.WriteLine(strr.Replace(",", "."));
                                            //Debug.WriteLine(strr.Replace(",", "."));
                                        }
                                    }
                                }

                                //if (cont_intensjk < line_intensjk)
                                //{
                                //    Debug.WriteLine("поток в линии больше для сектора {0} {1} с координатами центра {2} {3}, lambda {4}", j, k, sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI, r);
                                //}

                                //Console.WriteLine("area{0} \t sector {1} {2} \t mu {3} \t cont {4} \t line {5} \t lambda {6}", area, sectors[j][k].Get_PhiCent()*180/Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI, mu, cont_intensjk, line_intensjk, lambda_d);
                                //if (cont_intensjk < line_intensjk)
                                //{
                                //string str2 = String.Format("{0:0.0}\t{1:0.0}\t{2:0.00000}\t{3:0.00}\t{4:0.0000000}", sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI, sectors[j][k].Get_Mu(), lambda_d, line_intensjk/cont_intensjk);
                                //sw.WriteLine("{0:0.000}\t{1:0.000e0}\t{2:0.000e0}\t{3:0.000e0}", lambdas_fin[i], fluxes_fin_cont[i], fluxes_fin_line[i], fluxes_fin[i]);
                                //sw2.WriteLine(str2.Replace(",", "."));
                                // }
                            }
                        }

                        counter++;
                        //Debug.WriteLine(counter);
                    }
                }
                //test2.Close();
                //test3.Close();
                //test4.Close();

                sectors = ring2.Get_Sub();
                viss = ring2.Get_Visibs();
                counter = 0;

                for (int j = 0; j < n_phi_sub; j++)
                {

                    double lambda_d;
                    double line_intensjk, cont_intensjk;
                    double[] line_intenss_mu, cont_intenss_mu;
                    line_intenss_mu = new double[3];
                    cont_intenss_mu = new double[3];

                    for (int k = 0; k < n_beta_sub; k++)
                    {
                        //Debug.WriteLine("{0} for sector {1}, {2},  ring 2", viss[j][k], sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI);
                        if (viss[j][k])
                        {
                            mu = sectors[j][k].Get_Mu();
                            area = sectors[j][k].Get_Area();
                            Vrot_s = sectors[j][k].Get_Vrot();
                            for (int r = 50; r < n_lamb - 50; r++)
                            {
                                lambda_d = lambdas[r] * (1 + Vrot_s / c);

                                line_intensjk = mulin_line.Interp(r, lambda_d, mu);
                                cont_intensjk = mulin_cont.Interp(r, lambda_d, mu);
                                line_flux_ring[r - 50] += area * mu * line_intensjk;
                                cont_flux_ring[r - 50] += area * mu * cont_intensjk;

                                //Console.WriteLine("area{0} \t sector {1} {2} \t mu {3} \t cont {4} \t line {5}", area, sectors[j][k].Get_PhiCent() * 180 / Math.PI, sectors[j][k].Get_BetaCent() * 180 / Math.PI, mu, cont_intensjk, line_intensjk);
                            }
                        }

                        counter++;
                        //Debug.WriteLine(counter);
                    }
                }

                lock (line_flux)
                {
                    for (int q = 0; q < n_lamb - 100; q++)
                    {
                        line_flux[q] += line_flux_ring[q];
                    }
                }
                lock (cont_flux)
                {
                    for (int q = 0; q < n_lamb - 100; q++)
                    {
                        cont_flux[q] += cont_flux_ring[q];
                    }
                }

                Debug.WriteLine("Finish {0} ring.", i);
            });


            stopWatchPF.Stop();
            TimeSpan tsPF = stopWatchPF.Elapsed;

            string elapsedTimePF = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                tsPF.Hours, tsPF.Minutes, tsPF.Seconds,
                tsPF.Milliseconds / 10);
            Debug.WriteLine("RunTime PF " + elapsedTimePF);



            double[] lamb_graph = new double[n_lamb - 100];
            double[] flux_norm = new double[n_lamb - 100];
            fluxes_fin_cont = new double[n_lamb - 100];
            fluxes_fin_line = new double[n_lamb - 100];
            for (int q = 0; q < n_lamb - 100; q++)
            {
                lamb_graph[q] = lambdas[q + 50];
                fluxes_fin_cont[q] = cont_flux[q];
                fluxes_fin_line[q] = line_flux[q];
                flux_norm[q] = line_flux[q] / cont_flux[q];
            }

            //double[] fluxes_conv = Convolved(lamb_graph, flux_norm, FWHM, lambda_step);
            double[] fluxes_conv = varConvolved(lamb_graph, flux_norm, Resolution, lambda_step);

            lambdas_fin = lamb_graph;
            fluxes_fin = fluxes_conv;

        }


        private double[] Convolved(double[] lambdas, double[] fluxes, double FWHM, double step)
        {
            if (FWHM >= 0.001)
            {
                double sigma = FWHM / 2.0 / Math.Sqrt(2.0 * Math.Log(2.0));
                int halfgauss = Convert.ToInt32(5 * sigma / step);
                double[] gauss = new double[2 * halfgauss + 1];
                double[] gauss_xx = new double[gauss.Length];
                double[] fluxes_to_conv = new double[lambdas.Length + 2 * halfgauss];
                for (int i = 0; i < halfgauss; i++)
                {
                    gauss_xx[i] = -halfgauss * step + i * step;
                    gauss[i] = GaussFunc(gauss_xx[i], sigma);
                    fluxes_to_conv[i] = 1;
                }
                for (int i = 0; i < fluxes.Length; i++)
                {
                    fluxes_to_conv[i + halfgauss] = fluxes[i];
                }
                gauss_xx[halfgauss] = 0;
                gauss[halfgauss] = GaussFunc(0, sigma);
                for (int i = 0; i < halfgauss; i++)
                {
                    gauss_xx[i + halfgauss + 1] = step + i * step;
                    gauss[i + halfgauss + 1] = GaussFunc(gauss_xx[i + halfgauss + 1], sigma);
                    fluxes_to_conv[halfgauss + fluxes.Length + i] = 1;
                }

                //for (int i = 0; i < gauss.Length; i++)
                //   Debug.WriteLine("{0}\t{1}", gauss_xx[i], gauss[i]);

                double flux;
                double[] fluxes_conv = new double[fluxes.Length];
                for (int i = halfgauss; i < fluxes.Length + halfgauss; i++)
                {
                    flux = 0;
                    for (int j = 0; j < gauss.Length; j++)
                        flux += fluxes_to_conv[i + halfgauss - j] * gauss[j];
                    fluxes_conv[i - halfgauss] = flux;
                }

                double norm = fluxes_conv.Max();
                for (int i = 0; i < fluxes_conv.Length; i++)
                    fluxes_conv[i] /= norm;

                return fluxes_conv;
            }
            else
            {
                return fluxes;
            }
        }

        private double[] varConvolved(double[] lambdas, double[] fluxes, double ResPow, double step)
        {
            if (ResPow <= 1000000)
            {
                double sigma;
                int halfgauss = 500;
                double[] gauss_xx = new double[2 * halfgauss + 1];
                double[] fluxes_to_conv = new double[lambdas.Length + 2 * halfgauss];
                double[] lambdas_conv = new double[lambdas.Length + 2 * halfgauss];
                for (int i = 0; i < halfgauss; i++)
                {
                    gauss_xx[i] = -halfgauss * step + i * step;
                    //gauss[i] = GaussFunc(gauss_xx[i], sigma);
                    fluxes_to_conv[i] = 1;
                    lambdas_conv[i] = lambdas[0] - (1 + halfgauss - i) * step;
                }
                for (int i = 0; i < fluxes.Length; i++)
                {
                    fluxes_to_conv[i + halfgauss] = fluxes[i];
                    lambdas_conv[i + halfgauss] = lambdas[i];
                }
                for (int i = 0; i < halfgauss; i++)
                {
                    gauss_xx[i + halfgauss + 1] = step + i * step;
                    fluxes_to_conv[halfgauss + fluxes.Length + i] = 1;
                    lambdas_conv[halfgauss + fluxes.Length + i] = lambdas[lambdas.Length-1] + (1 + i) * step;
                }

                double flux, gauss, varFWHM;
                double[] fluxes_conv = new double[fluxes.Length];
                for (int i = halfgauss; i < fluxes.Length + halfgauss; i++)
                {
                    flux = 0;
                    for (int j = 0; j < gauss_xx.Length; j++)
                    {
                        varFWHM = lambdas_conv[i + halfgauss - j] / ResPow;
                        //Debug.WriteLine(varFWHM);
                        sigma = varFWHM / 2.0 / Math.Sqrt(2.0 * Math.Log(2.0));
                        gauss = GaussFunc(gauss_xx[j], sigma);
                        flux += fluxes_to_conv[i + halfgauss - j] * gauss;
                    }
                    fluxes_conv[i - halfgauss] = flux;
                }

                double norm = fluxes_conv.Max();
                for (int i = 0; i < fluxes_conv.Length; i++)
                    fluxes_conv[i] /= norm;

                return fluxes_conv;
            }
            else
            {
                return fluxes;
            }
        }

        private double GaussFunc(double x, double sigma)
        {
            double f = (x / sigma) * (x / sigma);
            f = Math.Exp(-0.5 * f);
            f /= (sigma * Math.Sqrt(2 * Math.PI));
            return f;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int[] elems;
                double[] abunds;
                string[] s = File.ReadAllLines("dabund.dat").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                string[] mmm;

                int count = 0;
                for (int j = 0; j < s.Length; j++)
                {
                    mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                    //Debug.WriteLine(mmm[0]);
                    int n1 = Convert.ToInt32(mmm[0].Replace(".", separator).Replace(",", separator));
                    double ab1 = Convert.ToDouble(mmm[1].Replace(".", separator).Replace(",", separator));
                    if (Math.Abs(ab1) > 1e-10)
                    {
                        count++;
                    }
                }
                elems = new int[count];
                abunds = new double[count];

                count = 0;
                for (int j = 0; j < s.Length; j++)
                {
                    mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                    int n1 = Convert.ToInt32(mmm[0].Replace(".", separator).Replace(",", separator));
                    double ab1 = Convert.ToDouble(mmm[1].Replace(".", separator).Replace(",", separator));
                    if (Math.Abs(ab1) > 1e-10)
                    {
                        elems[count] = n1;
                        abunds[count] = ab1;
                        count++;
                    }
                }

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.ShowDialog();
                string path = saveFileDialog1.FileName;
                StreamWriter sw = new StreamWriter(path);
                string str;
                str = string.Format("Inc {0:0.00},   V {1:0},   T_pole {2:0},   logg_pole {3:0.0000},   mass {4:0.0},   rings {5},   Me {6:0.0},   V_turb {7:0.000},   FWHM {8:0.000},   " +
                    "n_lamb {9},   lamb1 {10:0.000},   dlamb {11:0.0000}", inc * 180 / Math.PI, Ve / 100000, T_pole, Math.Log10(g_pole), mass / SolarMass, n_phi_ring, Math.Log10(Met), Vturb, FWHM, n_lamb - 100, lambda1 + lambda_step * 50, lambda_step);
                str.Replace(",", ".");
                sw.Write(str);
                string sabunds = "";
                for (int i = 0; i < elems.Length; i++)
                {
                    sabunds += "\t";
                    sabunds += Convert.ToString(elems[i]);
                    sabunds += "\t";
                    sabunds += String.Format("{0:0.000}", abunds[i]);
                }
                sabunds += "\r\n";
                sabunds.Replace(",", ".");
                sw.Write(sabunds);

                for (int i = 0; i < lambdas_fin.Length; i++)
                {
                    str = String.Format("{0:0.000}\t{1:0.00000e0}\t{2:0.00000e0}\t{3:0.00000e0}", lambdas_fin[i], fluxes_fin[i], fluxes_fin_cont[i], fluxes_fin_line[i]);
                    //sw.WriteLine("{0:0.000}\t{1:0.000e0}\t{2:0.000e0}\t{3:0.000e0}", lambdas_fin[i], fluxes_fin_cont[i], fluxes_fin_line[i], fluxes_fin[i]);
                    sw.WriteLine(str.Replace(",", "."));
                }
                sw.Close();
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch { }
        }

        private void btnLoadObs_Click(object sender, RoutedEventArgs e)
        {
            string path = "0";
            color_counter = 0;
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.ShowDialog();
                path = openFileDialog1.FileName;
            }
            catch { return; }

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string[] s = sr.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] mmm;

                    string[] cr = s[0].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                    double crr = 1;
                    Double.TryParse(cr[0], out crr);
                    if (crr != 0)
                    {
                        lambdas_obs = new double[s.Length];
                        fluxes_obs = new double[s.Length];

                        for (int j = 0; j < s.Length; j++)
                        {
                            mmm = s[j].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                            lambdas_obs[j] = Convert.ToDouble(mmm[0].Replace(".", separator).Replace(",", separator));
                            fluxes_obs[j] = Convert.ToDouble(mmm[1].Replace(".", separator).Replace(",", separator));
                        }
                    }
                    else
                    {
                        lambdas_obs = new double[s.Length - 1];
                        fluxes_obs = new double[s.Length - 1];

                        for (int j = 0; j < s.Length - 1; j++)
                        {
                            mmm = s[j + 1].Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                            lambdas_obs[j] = Convert.ToDouble(mmm[0].Replace(".", separator).Replace(",", separator));
                            fluxes_obs[j] = Convert.ToDouble(mmm[1].Replace(".", separator).Replace(",", separator));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Файл не найден");
                return;
            }
            catch (FormatException)
            {
                MessageBox.Show("Что-то не то с содержимым файла");
                return;
            }
            catch { }

            lambdas_obs_moved = new double[lambdas_obs.Length];
            for (int i = 0; i < lambdas_obs.Length; i++)
            {
                lambdas_obs_moved[i] = lambdas_obs[i];
            }

            try
            {
                sct1.Points.Clear();
            }
            catch { }
            try
            {
                //lineSerie.Points.Clear();
                //lineSerie2.Points.Clear();
                //lineSerie3.Points.Clear();
                //lineSerie4.Points.Clear();
                plotModel.Series.Clear();
                plotModel1.Series.Clear();
                plotModel2.Series.Clear();
                plotModel3.Series.Clear();
                plotModel4.Series.Clear();
            }
            catch{ }

            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();
            plotView3.InvalidatePlot();
            plotView4.InvalidatePlot();
            lineSerie2 = new LineSeries
            {
                StrokeThickness = 2,
                CanTrackerInterpolatePoints = false,
                Smooth = false,
                Color = OxyColor.FromArgb(255, 85, 115, 190)
            };
            sct1 = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerFill = OxyColor.FromArgb(255, 0, 0, 0) };
            for (int i = 0; i < lambdas_obs.Length; i++)
            {
                sct1.Points.Add(new ScatterPoint(lambdas_obs[i], fluxes_obs[i], 1, 100));
                lineSerie2.Points.Add(new DataPoint(lambdas_obs[i], fluxes_obs[i]));
            }
            plotModel.Series.Add(sct1);
            plotModel2.Series.Add(lineSerie2);
            PlotView.Model = null;
            plotView1.Model = plotModel;
            plotView2.Model = plotModel2;
            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();

            txtChi2.Text = "";

            btnLeft.IsEnabled = true;
            btnRight.IsEnabled = true;
            btnZero.IsEnabled = true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //lineSerie.Points.Clear();
                //sct1.Points.Clear();
                //lineSerie2.Points.Clear();
                //lineSerie3.Points.Clear();
                //lineSerie4.Points.Clear();
                plotModel.Series.Clear();
                plotModel1.Series.Clear();
                plotModel2.Series.Clear();
                plotModel3.Series.Clear();
                plotModel4.Series.Clear();
            }
            catch { };
            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();
            plotView3.InvalidatePlot();
            plotView4.InvalidatePlot();
                

            btnShowObs.IsEnabled = true;

            color_counter = 0;
        }
        
        private void btnShowObs_Click(object sender, RoutedEventArgs e)
        {
            lineSerie2 = new LineSeries
            {
                StrokeThickness = 2,
                CanTrackerInterpolatePoints = false,
                Smooth = false,
                Color = OxyColor.FromArgb(255, 85, 115, 190)
            };
            sct1 = new ScatterSeries { MarkerType = MarkerType.Circle, MarkerFill = OxyColor.FromArgb(255, 0, 0, 0) };
            for (int i = 0; i < lambdas_obs.Length; i++)
            {
                sct1.Points.Add(new ScatterPoint(lambdas_obs[i], fluxes_obs[i], 1, 100));
                lineSerie2.Points.Add(new DataPoint(lambdas_obs[i], fluxes_obs[i]));
            }
            plotModel.Series.Add(sct1);
            plotModel2.Series.Add(lineSerie2);
            plotView1.Model = plotModel;
            plotView2.Model = plotModel2;
            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();
        }

        private void btnSave1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.ShowDialog();
                string fileName = sf.FileName;
                using (var stream = File.Create(fileName))
                {
                    var pdfExporter = new PdfExporter { Width = Convert.ToInt32(txtWidth.Text), Height = Convert.ToInt32(txtHeight.Text) };
                    pdfExporter.Export(plotModel, stream);
                }
            }
            catch {  }
        }

        private void btnSave4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.ShowDialog();
                string fileName = sf.FileName;
                using (var stream = File.Create(fileName))
                {
                    var pdfExporter = new PdfExporter { Width = 400, Height = 400 };
                    pdfExporter.Export(plotModel4, stream);
                }
            }
            catch { }

        }

        private double Get_Chi2()
        {
            LinInterpolator lipV1 = new LinInterpolator(lambdas_fin, fluxes_fin);
            double[] flux_th_V_obs = new double[lambdas_obs.Length];
            for (int i = 0; i < lambdas_obs.Length; i++)
                flux_th_V_obs[i] = lipV1.Interp(lambdas_obs_moved[i]);

            double ff = 0;
            for (int i = 0; i < lambdas_obs.Length; i++)
                ff += Math.Pow((fluxes_obs[i] - flux_th_V_obs[i]), 2);

            return ff;
        }
        
        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            double step2 = Convert.ToDouble(txt_MoveStep.Text);
            for (int i = 0; i < lambdas_obs_moved.Length; i++)
            {
                lambdas_obs_moved[i] -= step2;
            }

            try
            {
                sct1.Points.Clear();
                lineSerie2.Points.Clear();
                lineSerie4.Points.Clear();
            }
            catch { }

            OC = new double[lambdas_obs.Length];
            LinInterpolator li = new LinInterpolator(lambdas_fin, fluxes_fin);
            double[] flux_th_obs = new double[lambdas_obs.Length];
            for (int i = 0; i < lambdas_obs.Length; i++)
                flux_th_obs[i] = li.Interp(lambdas_obs_moved[i]);

            for (int i = 0; i < lambdas_obs.Length; i++)
                OC[i] = flux_th_obs[i] - fluxes_obs[i];

            for (int i = 0; i < lambdas_obs_moved.Length; i++)
            {
                sct1.Points.Add(new ScatterPoint(lambdas_obs_moved[i], fluxes_obs[i], 1, 100));
                lineSerie2.Points.Add(new DataPoint(lambdas_obs_moved[i], fluxes_obs[i]));
                lineSerie4.Points.Add(new DataPoint(lambdas_obs_moved[i], OC[i]));
            }

            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();
            plotView4.InvalidatePlot();

            txtChi2.Text = String.Format("{0:0.000e0}", Get_Chi2());
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            double step2 = Convert.ToDouble(txt_MoveStep.Text);
            for (int i = 0; i < lambdas_obs_moved.Length; i++)
            {
                lambdas_obs_moved[i] += step2;
            }

            try
            {
                sct1.Points.Clear();
                lineSerie2.Points.Clear();
                lineSerie4.Points.Clear();
            }
            catch { }

            OC = new double[lambdas_obs.Length];
            LinInterpolator li = new LinInterpolator(lambdas_fin, fluxes_fin);
            double[] flux_th_obs = new double[lambdas_obs.Length];
            for (int i = 0; i < lambdas_obs.Length; i++)
                flux_th_obs[i] = li.Interp(lambdas_obs_moved[i]);

            for (int i = 0; i < lambdas_obs.Length; i++)
                OC[i] = flux_th_obs[i] - fluxes_obs[i];

            for (int i = 0; i < lambdas_obs_moved.Length; i++)
            {
                sct1.Points.Add(new ScatterPoint(lambdas_obs_moved[i], fluxes_obs[i], 1, 100));
                lineSerie2.Points.Add(new DataPoint(lambdas_obs_moved[i], fluxes_obs[i]));
                lineSerie4.Points.Add(new DataPoint(lambdas_obs_moved[i], OC[i]));
            }

            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();
            plotView4.InvalidatePlot();

            txtChi2.Text = String.Format("{0:0.000e0}", Get_Chi2());
        }
        
        private void btnZero_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < lambdas_obs_moved.Length; i++)
            {
                lambdas_obs_moved[i] = lambdas_obs[i];
            }

            try
            {
                sct1.Points.Clear();
                lineSerie2.Points.Clear();
                lineSerie4.Points.Clear();
            }
            catch { }

            OC = new double[lambdas_obs.Length];
            LinInterpolator li = new LinInterpolator(lambdas_fin, fluxes_fin);
            double[] flux_th_obs = new double[lambdas_obs.Length];
            for (int i = 0; i < lambdas_obs.Length; i++)
                flux_th_obs[i] = li.Interp(lambdas_obs_moved[i]);

            for (int i = 0; i < lambdas_obs.Length; i++)
                OC[i] = flux_th_obs[i] - fluxes_obs[i];

            for (int i = 0; i < lambdas_obs_moved.Length; i++)
            {
                sct1.Points.Add(new ScatterPoint(lambdas_obs_moved[i], fluxes_obs[i], 1, 100));
                lineSerie2.Points.Add(new DataPoint(lambdas_obs_moved[i], fluxes_obs[i]));
                lineSerie4.Points.Add(new DataPoint(lambdas_obs_moved[i], OC[i]));
            }

            plotView1.InvalidatePlot();
            plotView2.InvalidatePlot();
            plotView4.InvalidatePlot();

            txtChi2.Text = String.Format("{0:0.000e0}", Get_Chi2());
        }

        private void btnSavetxtOC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.ShowDialog();
                string path = saveFileDialog1.FileName;
                StreamWriter sw = new StreamWriter(path);
                string str;
                str = string.Format("Inc {0:0.00},   V {1:0},   T_pole {2:0},   logg_pole {3:0.0000},   mass {4:0.0},   rings {5},   Me {6:0.0},   V_turb {7:0.000},   FWHM {8:0.000},   " +
                    "n_lamb {9},   lamb1 {10:0.000},   dlamb {11:0.0000}", inc * 180 / Math.PI, Ve / 100000, T_pole, Math.Log10(g_pole), mass / SolarMass, n_phi_ring, Math.Log10(Met), Vturb, FWHM, n_lamb - 100, lambda1 + lambda_step * 50, lambda_step);
                str.Replace(",", ".");
                sw.Write(str);
                sw.Write("\r\n");

                for (int i = 0; i < OC.Length; i++)
                {
                    str = String.Format("{0:0.000}\t{1:0.00000e0}", lambdas_obs_moved[i], OC[i]);
                    //sw.WriteLine("{0:0.000}\t{1:0.000e0}\t{2:0.000e0}\t{3:0.000e0}", lambdas_fin[i], fluxes_fin_cont[i], fluxes_fin_line[i], fluxes_fin[i]);
                    sw.WriteLine(str.Replace(",", "."));
                }
                sw.Close();
            }
            catch {}
        }
    }


}

