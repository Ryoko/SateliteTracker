namespace SateliteTracker.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ASCOM.Astrometry;

    public class MovmentStatistic
    {
        private const int BufferSize = 1000;
        private double sco = 1;
        private int n = 1;

        public double AddValue(double value, double modelValue)
        {
            var error = Math.Abs(value - modelValue);

            this.sco = (this.sco * (this.n - 1) + error) / this.n;
            this.n++;
            return 0;
        }
    }

    public class Utils
    {
        static public string Deg2HEX32(double val)
        {
            var v = (Int32)((val / 360) * 4294967296);
            return v.ToString("X");
        }
        static public string Deg2HEX16(double val)
        {
            var v = (Int16)((val / 360) * 65536);
            return v.ToString("X");

        }
        static public string RADeg2HEX32(double val)
        {
            var v = (Int32)((val / 24) * 4294967296);
            return v.ToString("X");
        }
        static public string RADeg2HEX16(double val)
        {
            var v = (Int16)((val / 24) * 65536);
            return v.ToString("X");

        }

        static public double JDtime(DateTime time)
        {
            var utils = new ASCOM.Astrometry.AstroUtils.AstroUtils();
            var MJDdate = utils.CalendarToMJD(time.Day, time.Month, time.Year);
            MJDdate += time.TimeOfDay.TotalDays + 2400000.5;
            return MJDdate;
        }

        static public Coordinates AltAzm2RaDec(AltAzm altAzm, LatLon location, DateTime time, double elevation)
        {
            var JDdate = JDtime(time);
            var tfm = new ASCOM.Astrometry.Transform.Transform();
            var jdtt = tfm.JulianDateTT;
            //tfm.JulianDateTT = JDdate;
            tfm.SiteElevation = elevation * 1000;
            tfm.SiteLatitude = location.Lat;
            tfm.SiteLongitude = location.Lon;
            tfm.SiteTemperature = 0;
            tfm.SetAzimuthElevation(altAzm.Azm, altAzm.Alt);
            tfm.Refresh();
//            var res = new Coordinates(tfm.RAJ2000, tfm.DecJ2000);
//            var res = new Coordinates(tfm.RAApparent, tfm.DECApparent);
            var res = new Coordinates(tfm.RATopocentric, tfm.DECTopocentric);
            return res;
        }

        static public AltAzm RaDec2AltAzm(Coordinates coord, LatLon location, DateTime time, double elevation)
        {
            var utils = new ASCOM.Astrometry.AstroUtils.AstroUtils();
            var MJDdate = utils.CalendarToMJD(time.Day, time.Month, time.Year);
            MJDdate += time.TimeOfDay.TotalDays;

            var tfm = new ASCOM.Astrometry.Transform.Transform();
            tfm.JulianDateTT = MJDdate;
            tfm.SiteElevation = elevation * 1000;
            tfm.SiteLatitude = location.Lat;
            tfm.SiteLongitude = location.Lon;
            tfm.SiteTemperature = 0;
            tfm.SetApparent(coord.Ra, coord.Dec);
            tfm.Refresh();

            var res = new AltAzm(tfm.ElevationTopocentric, tfm.AzimuthTopocentric);
            return res;
        }

        public static Coordinates AltAzm2RaDec2(AltAzm altAzm, LatLon location, DateTime UTCtime, double elevation)
        {
            return new Coordinates(0, 0);
        }

        public static AltAzm RaDec2AltAzm2(Coordinates coord, LatLon location, DateTime UTCtime, double elevation)
        {
            var utils = new ASCOM.Astrometry.NOVAS.NOVAS31();

            var ra = coord.Ra * 15;
            var dec = coord.Dec;
            var time = UTCtime.TimeOfDay.TotalHours;
            var lat = location.Lat;
            var lon = location.Lon;
            var j200Time = utils.JulianDate((short)UTCtime.Year, (short)UTCtime.Month, (short)UTCtime.Day, time);
            j200Time -= 2451545.0;
            var LST =  100.46 + 0.985647 * j200Time + lon + 15 * time;
            LST = LST.ToRange(0, 360);

            var HA = (LST - ra).ToRange(0, 360);

            var sinALT = Math.Sin(dec.ToRad()) * Math.Sin(lat.ToRad()) + Math.Cos(dec.ToRad()) * Math.Cos(lat.ToRad()) * Math.Cos(HA.ToRad());
            var ALT = Math.Asin(sinALT);
            var alt = ALT.ToDeg();

            var cosA = (Math.Sin(dec.ToRad()) - Math.Sin(ALT) * Math.Sin(lat.ToRad())) / (Math.Cos(ALT) * Math.Cos(lat.ToRad()));
            var A = Math.Acos(cosA).ToDeg();

            var Azm = Math.Sin(HA.ToRad()) < 0 ? A : 360 - A;

            return new AltAzm(alt, Azm);
        }

        static public double NowLST(LatLon location)
        {
            var nov = new ASCOM.Astrometry.NOVAS.NOVAS31();
            var ast = new ASCOM.Astrometry.AstroUtils.AstroUtils();
            var currJD = ast.JulianDateUT1(0);
            double lstNow = 0;
            var res = nov.SiderealTime(
                currJD, 0d, 0, GstType.GreenwichApparentSiderealTime, Method.EquinoxBased, Accuracy.Full, ref lstNow);
            if (res != 0) throw new Exception("Error getting Local Apparent Sidereal time");
            return lstNow;
        }

        //public static double ToSiderealTime(DateTime dt)
        //{
        //    var utilities = new Utilities.Util();
        //    double siderealTime = (18.697374558 + 24.065709824419081 * (utilities.DateLocalToJulian(dt) - 2451545.0))
        //                          % 24.0;
        //    return siderealTime;
        //}

        static public string Bytes2Dump(IEnumerable<byte> buf)
        {
            return buf.Aggregate("", (current, b) => current + string.Format(" {0:x}", b));
        }
    }

    public class Coordinates
    {
        public Coordinates(double ra, double dec)
        {
            this.Ra = ra;
            this.Dec = dec;
        }
        public double Ra { get; set; }
        public double Dec { get; set; }
    }
    public class AltAzm
    {
        public AltAzm(double alt, double azm)
        {
            this.Alt = alt;
            this.Azm = azm;
        }
        public double Alt { get; set; }
        public double Azm { get; set; }
    }
    public class LatLon
    {
        public LatLon(double lat, double lon)
        {
            this.Lat = lat;
            this.Lon = lon;
        }
        public LatLon(decimal lat, decimal lon)
        {
            this.Lat = (double)lat;
            this.Lon = (double)lon;
        }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public static class Myextensions
    {
        public static byte[] ToBytes(this string val)
        {
            return val.Select((c, i) => (byte)c).ToArray();
        }

        public static double ToRange(this double val, double min, double max)
        {
            var v = val;
            while (v < min) v += max;
            while (v > max) v -= max;
            return v;
        }

        public static double ToRad(this double val)
        {
            return val * Math.PI / 180;
        }
        
        public static double ToDeg(this double val)
        {
            return val * 180 / Math.PI;
        }
    }

}
