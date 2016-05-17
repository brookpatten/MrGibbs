using System;

using MrGibbs.Models;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
	public class TrueWindCalculator:ICalculator
	{
		private ILogger _logger;
		private IPlugin _plugin;

		public TrueWindCalculator(ILogger logger, IPlugin plugin)
		{
			_plugin = plugin;
			_logger = logger;
		}

		public void Calculate(State state)
		{
			if (state.StateValues.ContainsKey (StateValue.ApparentWindAngle)
				&& state.StateValues.ContainsKey (StateValue.ApparentWindSpeedKnots)
				&& state.StateValues.ContainsKey (StateValue.SpeedInKnots)
				&& (state.StateValues.ContainsKey (StateValue.CourseOverGroundDirection)
					|| state.StateValues.ContainsKey (StateValue.MagneticHeading)
					|| state.StateValues.ContainsKey (StateValue.MagneticHeadingWithVariation))) {
				double boatHeading = 0;
				if (state.StateValues.ContainsKey (StateValue.CourseOverGroundDirection)) {
					boatHeading = state.StateValues [StateValue.CourseOverGroundDirection];
				} else if (state.StateValues.ContainsKey (StateValue.MagneticHeadingWithVariation)) {
					boatHeading = state.StateValues [StateValue.MagneticHeadingWithVariation];
				} else if (state.StateValues.ContainsKey (StateValue.MagneticHeading)) {
					boatHeading = state.StateValues [StateValue.MagneticHeading];
				}


				double [] trueWindSpeed = new double[1];
				double [] trueWindDirection = new double[1];

				TrueWind (1
				    ,new double[]{ boatHeading}
					,new double[]{ state.StateValues [StateValue.SpeedInKnots]}
		            ,new double[]{ state.StateValues [StateValue.ApparentWindAngle]}
		            ,0.0
		            //TODO: change this to mag heading once it's reliable
		          	,new double[]{ boatHeading}
		          	,new double[1]
			        ,new double[]{ state.StateValues [StateValue.ApparentWindSpeedKnots]}
				    ,new double[]{ double.MaxValue,double.MaxValue,double.MaxValue,double.MaxValue,double.MaxValue}
					,trueWindDirection
					,trueWindSpeed);

				if (trueWindDirection[0] != double.MaxValue) 
				{
					state.StateValues [StateValue.TrueWindAngle] = AngleUtilities.NormalizeAngleDegrees (trueWindDirection [0] - boatHeading);
					state.StateValues [StateValue.TrueWindDirection] = trueWindDirection [0];
				}
				if (trueWindSpeed[0] != double.MaxValue) 
				{
					state.StateValues [StateValue.TrueWindSpeedKnots] = trueWindSpeed [0];
				}
			}
		}

		/// <inheritdoc />
		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}

//		shamelessly borrowed from:
//		http://coaps.fsu.edu/woce/truewind/c-codes/truewind.c
//		http://coaps.fsu.edu/woce/truewind/true-C.html
//		http://coaps.fsu.edu/woce/truewind/paper/index.html
//
//		INPUT VALUES:
//
//		num	int		Number of observations in input (crse, cspd, wdir,
//                                         wspd, hd) and output (adir, tdir, tspd) data arrays.  
//          
//					ALL ARRAYS MUST BE OF EQUAL LENGTH.
//       crse	float array	Course TOWARD WHICH the vessel is moving over the 
//       ground. Referenced to true north and the fixed earth.
//
//       cspd	float array	Speed of vessel over the ground. Referenced
//       to the fixed earth.
//
//       hd	float array	Heading toward which bow of vessel is pointing. 
//       Referenced to true north.
//
//       zlr	float		Zero line reference -- angle between bow and
//       zero line on anemometer.  Direction is clockwise
//       from the bow.  (Use bow=0 degrees as default 
//		  when reference not known.)			
//
//       wdir	float array	Wind direction measured by anemometer,
//       referenced to the ship.
//
//       wspd	float array	Wind speed measured by anemometer,referenced to
//       the vessel's frame of reference.
//
//       wmis	float array	Five element array containing missing values for
//           crse, cspd, wdir, wspd, and hd. In the output, the missing
//           value for tdir is identical to the missing value
//               specified in wmis for wdir. Similarly, tspd uses
//                   the missing value assigned to wmis for wspd.
//
//                       *** WDIR MUST BE METEOROLOGICAL (DIRECTION FROM)! CRSE AND CSPD MUST BE
//                       RELATIVE TO A FIXED EARTH! ***
//
//                       OUTPUT VALUES:
//
//       tdir	float array	True wind direction - referenced to true north
//                       and the fixed earth with a direction from which 
//                       the wind is blowing (meteorological).
//       tspd	float array	True wind speed - referenced to the fixed earth.
//       adir	float array	Apparent wind direction (direction measured by
//				wind vane, relative to true north). IS 
//                       REFERENCED TO TRUE NORTH & IS DIRECTION FROM
//                       WHICH THE WIND IS BLOWING. Apparent wind 
//                       edirection is the sum of the ship relative wind 
//                       direction (measured by wind vane relative to the
//						bow), the ship's heading, and the zero-line
//                       reference angle.  NOTE:  The apparent wind speed
//                       has a magnitude equal to the wind speed measured
//						 by the anemometer (wspd).
		private void TrueWind(int num, double[] cog, double[] boatspeed, double[] apparantWindAngle, 
		                      double vaneOffset, double[] heading, double[] trueWindAngle, double[] apparantWindSpeed, 
		                      double[] wmis, double[] trueWindDirection, double[] trueWindSpeed 
		                      /*,int nw, int nwam, 
		                      int nwpm, int nwf*/)
		{

			/* Define variables. */

			int calm_flag;
			int i;
			double x, y, mcrse, mwdir, mtdir, dtor;

			/* Initialize values.*/

			x = 0;
			y = 0;
			int nw = 0;
			int nwam = 0;
			int nwpm = 0;
			int nwf = 0;
			dtor = Math.PI/180.0;  /* degrees to radians conversion  */

			/* Loop over 'num' values. */

			for(i=0; i<num; i++)
			{

				/*    Check course, ship speed, heading, wind direction, and wind speed
      for valid values (i.e. neither missing nor outside physically 
      acceptable ranges) . */

				if( ( ((cog[i] < 0) || (cog[i] > 360)) && (cog[i] != wmis[0]) ) ||
				( (boatspeed[i] < 0) && (boatspeed[i] != wmis[1]) ) ||
				( ((apparantWindAngle[i] < 0) || (apparantWindAngle[i] > 360)) && (apparantWindAngle[i] != wmis[2]) ) ||
				( (apparantWindSpeed[i] < 0) && (apparantWindSpeed[i] != wmis[3]) ) ||
				( ((heading[i] < 0) || (heading[i] > 360)) && (heading[i] != wmis[4]) ) )
				{         
					/*       When some or all of input data fails range check, true winds are set
         to missing. Step index for input value(s) being out of range   */
					nwf = nwf + 1;
					trueWindDirection[i] = wmis[2];
					trueWindSpeed[i] = wmis[3];   

					if( (cog[i] != wmis[0]) && (boatspeed[i] != wmis[1]) && 
					(apparantWindAngle[i] != wmis[2]) && (apparantWindSpeed[i] != wmis[3]) &&
					(heading[i] != wmis[4]) )
						/*         Step index for all input values being non-missing  */  
					{
						nw = nw + 1;
					}
					else 
					{ 
						if( (cog[i] != wmis[0]) || (boatspeed[i] != wmis[1]) || 
						(apparantWindAngle[i] != wmis[2]) || (apparantWindSpeed[i] != wmis[3]) ||
						(heading[i] != wmis[4]) )
							/*            Step index for part of input values being missing  */
						{
							nwpm = nwpm + 1;
						} 
						else
							/*            Step index for all input values being missing  */
						{
							nwam = nwam + 1;
						}                
					}
				} 

				/*    When course, ship speed, heading, wind direction, and wind speed 
      are all in range and non-missing, then compute true winds. */ 

				else if( (cog[i] != wmis[0]) && (boatspeed[i] != wmis[1]) && 
				(apparantWindAngle[i] != wmis[2]) && (apparantWindSpeed[i] != wmis[3]) && 
				(heading[i] != wmis[4]) )
				{
					nw = nw + 1;

					/*       convert from navigational coordinates to angles commonly used 
         in mathematics  */
					mcrse = 90.0 - cog[i];   

					/*       keep the value between 0 and 360 degrees  */
					if( mcrse <= 0.0 ) mcrse = mcrse + 360.0;

					/*	 check zlr for valid value.  If not valid, set equal to zero.*/
					if( (vaneOffset < 0.0) || (vaneOffset > 360.0) ) vaneOffset = 0.0;

					/*	 calculate apparent wind direction  */
					trueWindAngle[i] = heading[i] + apparantWindAngle[i] + vaneOffset;

					/*	 keep adir between 0 and 360 degrees  */
					while( trueWindAngle[i] >= 360.0 ) trueWindAngle[i] = trueWindAngle[i] - 360.0;	 

					/*       convert from meteorological coordinates to angles commonly used   
         in mathematics  */
					mwdir = 270.0 - trueWindAngle[i];

					/*       keep the value between 0 and 360 degrees  */
					if( mwdir <= 0.0 ) mwdir = mwdir + 360.0;
					else if( mwdir > 360.0 ) mwdir = mwdir - 360.0;

					/*       determined the East-West vector component and the North-South
         vector component of the true wind */ 
					x = (apparantWindSpeed[i] * Math.Cos(mwdir * dtor)) + (boatspeed[i] 
					                                          * Math.Cos(mcrse * dtor));
					y = (apparantWindSpeed[i] * Math.Sin(mwdir * dtor)) + (boatspeed[i] 
					                                          * Math.Sin(mcrse * dtor));

					/*       use the two vector components to calculate the true wind speed  */
					trueWindSpeed[i] = Math.Sqrt((x * x) + (y * y));
					calm_flag = 1;

					/*       determine the angle for the true wind  */
					if(Math.Abs(x) > 0.00001) mtdir = (Math.Atan2(y,x)) / dtor;
					else 
					{ 
						if(Math.Abs(y) > 0.00001) mtdir = 180.0 - (90.0 * y) / Math.Abs(y);
						else
							/*            the true wind speed is essentially zero: winds are calm
              and direction is not well defined   */ 
						{
							mtdir = 270.0;
							calm_flag = 0;
						}
					}

					/*       convert from the common mathematical angle coordinate to the 
         meteorological wind direction  */ 
					trueWindDirection[i] = 270.0 - mtdir;

					/*       make sure that the true wind angle is between 0 and 360 degrees */ 
					while(trueWindDirection[i] < 0.0) trueWindDirection[i] = (trueWindDirection[i] + 360.0) * calm_flag;
					while(trueWindDirection[i] > 360.0) trueWindDirection[i] = (trueWindDirection[i] - 360.0) * calm_flag;
					/* 	 Ensure WMO convention for tdir=360 for win from North and tspd > 0 */
					if (calm_flag == 1 && (trueWindDirection[i] < 0.0001))
						trueWindDirection[i] = 360.0;

					x = 0.0; 
					y = 0.0;
				}

				/*    When course, ship speed, apparent direction, and wind speed 
      are all in range but part of these input values are missing,
      then set true wind direction and speed to missing.  */  

				else
				{
					if( (cog[i] != wmis[0]) || (boatspeed[i] != wmis[1]) || 
					(apparantWindAngle[i] != wmis[2]) || (apparantWindSpeed[i] != wmis[3]) ||
					(heading[i] != wmis[4]) )
					{
						nwpm = nwpm + 1;
						trueWindDirection[i] = wmis[2];
						trueWindSpeed[i] = wmis[3];
					}

					/*       When course, ship speed, apparent direction, and wind speed 
         are all in range but all of these input values are missing,
         then set true wind direction and speed to missing.  */    

					else
					{
						nwam = nwam + 1;
						trueWindDirection[i] = wmis[2];
						trueWindSpeed[i] = wmis[3];
					}
				}    
			}        

		}              
	}
}

