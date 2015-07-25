using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Calculators
{
    public class TiltCompensatedHeadingCalculator : ICalculator
    {
        private ILogger _logger;
        private CalculatorPlugin _plugin;

        public TiltCompensatedHeadingCalculator(ILogger logger, CalculatorPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public void Calculate(State state)
        {
            if (state.Accel != null && state.Magneto != null)
            {
                double xh, yh, ayf, axf;
                ayf = state.Accel.Y/57.0; //Convert to rad
                axf = state.Accel.X/57.0; //Convert to rad
                xh = state.Magneto.X*Math.Cos(ayf) + state.Magneto.Y*Math.Sin(ayf)*Math.Sin(axf) -
                     state.Magneto.Z*Math.Cos(axf)*Math.Sin(ayf);
                yh = state.Magneto.Y*Math.Cos(axf) + state.Magneto.Z*Math.Sin(axf);

                var compass = Math.Atan2((double) yh, (double) xh)*(180.0/Math.PI) - 90.0; // angle in degrees
                if (compass > 0)
                {
                    compass = compass - 360;
                }
                compass = 360 + compass;
                state.MagneticHeading = compass;
            }
            else
            {
                state.MagneticHeading = null;
            }
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
        }
    }
}
