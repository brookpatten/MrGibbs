using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Calculators;
using PovertySail.Models;
using PovertySail.MagneticVariation;
using NLog;

namespace PovertySail.TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            var nlog = LogManager.GetCurrentClassLogger();
            var logger = new PovertySail.Infrastructure.NLogLogger(nlog);


            var mag = new TSAGeoMag(logger);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var variation = mag.getDeclination(39.2576946666667, -84.2860833333333, TSAGeoMag.decimalYear(DateTime.Now),
                219.0/1000.0);
            watch.Stop();


            Console.WriteLine("Variation is {0} calculated in {1:0.00}s",variation,watch.Elapsed.TotalSeconds);
            Console.ReadLine();

        }
    }
}
