using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Calculators;
using MrGibbs.Models;
using MrGibbs.MagneticVariation;
using NLog;

namespace MrGibbs.TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            var nlog = LogManager.GetCurrentClassLogger();
            var logger = new MrGibbs.Infrastructure.NLogLogger(nlog);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            MrGibbs.Test.StateTest stateTest = new Test.StateTest();
            stateTest.Run();

            watch.Stop();
            Console.ReadLine();

        }

        public static void VmgTest()
        {
            double courseAngle = 270;
            double courseOverGround = 270 - 45;
            double speed = 10;

            double courseAngleRadians = AngleUtilities.DegreestoRadians(courseAngle);
            double courseOverGroundRadians = AngleUtilities.DegreestoRadians(courseOverGround);

            double difference = AngleUtilities.AngleDifference(courseAngleRadians, courseOverGroundRadians);


            var cos = Math.Cos(difference);

            var vmg = cos * speed;

            vmg = MarkCalculator.VelocityMadeGood(courseAngle, courseOverGround, speed);


            Console.WriteLine(string.Format("VMG:{0:0.00}\tVMG%:{1:0.00}", vmg, (vmg / speed * 100)));
        }

        public static void VariationTest()
        {
            //var mag = new TSAGeoMag(logger);
            //var variation = mag.getDeclination(39.2576946666667, -84.2860833333333, TSAGeoMag.decimalYear(DateTime.Now),
            //    219.0/1000.0);
            //Console.WriteLine("Variation is {0} calculated in {1:0.00}s",variation,watch.Elapsed.TotalSeconds);

            //146.51 (140.61 true) from 39.302384,-84.3143165, altitude 264.9, deviation -5.90659270237146
            //233.10 (227.19 true) from 39.2972996666667,-84.3152046666667, altitude 271.5, deviation -5.90498473426532

        }

        public static void IntersectTest()
        {
            double deviation = -5.90659270237146;

            var a = new CoordinatePoint(new Coordinate(39.302384), new Coordinate(-84.3143165), 0);
            var aBearing = 146.51 + deviation;
            var b = new CoordinatePoint(new Coordinate(39.2972996666667), new Coordinate(-84.3152046666667), 0);
            var bBearing = 233.10 + deviation;

            //var a = new CoordinatePoint(new Coordinate(39,15,26.84), new Coordinate(-84,-17,-15.93), 0);
            //var aBearing = 90;
            //var b = new CoordinatePoint(new Coordinate( 39,15,35.25), new Coordinate( -84,-17,-11.84), 0);
            //var bBearing = 180;



            var intersection = CoordinatePointUtilities.FindIntersection(a, aBearing, b, bBearing);

            Console.WriteLine(intersection.Latitude.Value + "," + intersection.Longitude.Value);

        }
    }
}
