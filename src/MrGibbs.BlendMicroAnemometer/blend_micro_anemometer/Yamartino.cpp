/*==============================================================================
 Arduino Library to calculate average wind direction and standard deviation based
 on http://en.wikipedia.org/wiki/Yamartino_method.
 
 Copyright (c) 2010-2013 Christopher Baker <http://christopherbaker.net>
 
 Conversion to Arduino Library Copyright 2015 by Andrew Bythell <abythell@ieee.org> 
 http://angryelectron.com
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 
 ==============================================================================*/
 
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "Yamartino.h"

/**
 * Constructor.
 * parameter: number of samples in the data set.
 */
Yamartino::Yamartino(int sampleSize) {

  historyLength = sampleSize;
  historyCos = (float*) malloc(historyLength * sizeof(float));
  historySin = (float*) malloc(historyLength * sizeof(float));
     
  // fill the history buffer with zeros, or else you won't
  // know what is in there.  it could take the values from
  // the last time the program ran, which will restult in
  // potentially odd startup behavior.
  memset(historyCos, 0, sizeof(historyCos));
  memset(historySin, 0, sizeof(historySin));
  
}  

/**
 * Destructor - free memory.
 */
Yamartino::~Yamartino() {
  free(historyCos);
  free(historySin);
}

/**
 * Add a new value to the data set.
 * parameter: heading in degrees.
 */
void Yamartino::add(float valueDegrees) {
  float valueRadians = valueDegrees * M_PI / 180.0; // convert to radians to use w/ sin/cos

  // move everyone value to the right by iterating through
  // the array starting at the second item and placing  
  // the last (i-1) value in the current position
  // the new value takes the 0th position.
  // we must keep the SIN and COS in order to calculate the
  // angular averages + stdevs correctly (see YAMARTINO METHOD)

  for (int i = historyLength - 1; i >= 0; i--) {
    if(i == 0) {
      historyCos[0] = cos(valueRadians); // put new val in the 0 position
      historySin[0] = sin(valueRadians); // put new val in the 0 position
    } 
    else {
      historyCos[i] = historyCos[i-1];
      historySin[i] = historySin[i-1];
    }
  } 
}

/**
 * Private: Calculate average and standard deviation.
 */
void Yamartino::analyzeHistoryBuffer() {

  float sumX = 0;
  float sumY = 0;

  for (int i = 0; i < historyLength; i++) {
    sumX += historyCos[i];
    sumY += historySin[i];
  }

  // calculate the average value in the history
  float t = atan2(sumY, sumX);
 
  compassAvg = t > 0 ? t : 2 * M_PI + t;

  // calculate the standard deviation
  float avgX = sumX / historyLength;
  float avgY = sumY / historyLength;

  //  METHOD FOR STANDARD DEVIATION!!
  // http://en.wikipedia.org/wiki/Yamartino_method
  float eps = sqrt(1 - (avgX*avgX + avgY*avgY));
  eps = isnan(eps) ? 0 : eps; // correct for NANs

   compassStd = asin(eps)* (1 + (2 / sqrt(3) - 1) * (eps * eps * eps));
}

/**
 * Get average heading in current data set.
 * returns: heading, in degrees.
 */
float Yamartino::averageHeading() {
  analyzeHistoryBuffer();
  return compassAvg * 180.0 / M_PI;
}

/**
 * Get standard deviation in current data set.
 * returns: standard deviation.
 */
float Yamartino::standardDeviation() {
  analyzeHistoryBuffer();
  return compassStd * 180.0 / M_PI;
}



