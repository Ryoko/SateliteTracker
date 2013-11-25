﻿using System;
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

    using SateliteTracker.Utils;

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
            if (tle != null)
            {
                var Location = new LatLon(53.9, 27.56667);
                var dt = DateTime.UtcNow;

                var JDdate = Utils.Utils.JDtime(dt);
                MJD.Text = JDdate.ToString("f5");
                orbit = new Orbit(tle);
                Site site = new Site(Location.Lat, Location.Lon, 0.290);
                EciTime eci = orbit.GetPosition(dt);
                Topo topoLook = site.GetLookAngle(eci);
                main.Altitude.Text = topoLook.ElevationDeg.ToString("f3");
                main.Azimuth.Text = topoLook.AzimuthDeg.ToString("f3");
                var altAzm = new AltAzm(topoLook.ElevationDeg, topoLook.AzimuthDeg);
                var RaDec = Utils.Utils.AltAzm2RaDec(altAzm, Location, dt, 0.29);

                var tt2 = Utils.Utils.RaDec2AltAzm2(
                    new Coordinates(16.695, 36.466667),
                    new LatLon(52.5, -1.9166667),
                    new DateTime(1998, 8, 10, 23, 10, 00),
                    0);
                var ttt = Utils.Utils.RaDec2AltAzm2(RaDec, Location, dt, 0.2);

                RA.Text = DMS.FromDeg(RaDec.Ra).ToString();
                Dec.Text = DMS.FromDeg(RaDec.Dec).ToString();
            }
        }

    }
}
