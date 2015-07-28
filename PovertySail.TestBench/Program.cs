using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Calculators;
using PovertySail.Models;

namespace PovertySail.TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            MagVar magVar = new MagVar();

            double julianDate = JulianDate.JD(DateTime.UtcNow);
            long jd = (long) julianDate;

            double[] fields=new double[6];

            var variation = magVar.SGMagVar(39.2576946666667, -84.2860833333333, 219.0/1000.0, jd, 10, fields);

            Console.WriteLine(variation);
            Console.ReadLine();

        }
    }
}
