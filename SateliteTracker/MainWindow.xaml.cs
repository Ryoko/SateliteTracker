using System;
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

namespace SateliteTracker
{
    using System.Threading;

    using Zeptomoby.OrbitTools;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            main = this;
            new Task(action).Start();
        }

        private static Tle tle;
        private static Orbit orbit;

        private static MainWindow main;

        private void ProcessTLE_Click(object sender, RoutedEventArgs e)
        {
            var lines = TLEedit.Text.Split(new char[] { '\n', '\r' }, 3);
            if (lines.Count() < 3) return;
            var nm = lines[0].TrimEnd(new char[] { ' ' });
            tle = new Tle(nm, lines[1], lines[2]);
            orbit = new Orbit(tle);

        }

        private static Action action = () =>
            {
                while (true)
                {
                    main.Dispatcher.Invoke(main.ShowData);
                    Thread.Sleep(500);
                }
            };

        private void ShowData()
        {
            if (tle != null && orbit != null)
            {
                Site site = new Site(53.9, 27.5, 0.290); // 0.00 N, 100.00 W, 0 km altitude
                EciTime eci = orbit.GetPosition(DateTime.UtcNow);
                Topo topoLook = site.GetLookAngle(eci);
                main.Altitude.Text = topoLook.ElevationDeg.ToString("f3");
                main.Azimuth.Text = topoLook.AzimuthDeg.ToString("f3");
                
            }
        }
    }
}
