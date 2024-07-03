using System.Windows;

namespace SshPoc
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class LatencyMonitor : Window
    {
        public LatencyMonitor()
        {
            InitializeComponent();

            //LatencyMonitorViewModel vm = new LatencyMonitorViewModel();
            //_myPolyline.Points = vm.Points;

            //double[] x = new double[200];
            //for (int i = 0; i < x.Length; i++)
            //    x[i] = 3.1415 * i / (x.Length - 1);

            //for (int i = 0; i < 25; i++)
            //{
            //    var lg = new LineGraph();
            //    lines.Children.Add(lg);
            //    lg.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, (byte)(i * 10), 0));
            //    lg.Description = String.Format("Data series {0}", i + 1);
            //    lg.StrokeThickness = 2;
            //    lg.Plot(x, x.Select(v => Math.Sin(v + i / 10.0)).ToArray());
            //}


        }
    }
}