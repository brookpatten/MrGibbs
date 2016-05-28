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
 
#ifndef YAMARTINO_H
#define YAMARTINO_H

class Yamartino {
  
 public:
   Yamartino(int length);
   ~Yamartino();
   void add(float heading);
   float averageHeading(void);
   float standardDeviation(void); 
   
 private:
   int historyLength;
   void analyzeHistoryBuffer(void);
   float *historyCos;
   float *historySin;
   float compassAvg;
   float compassStd;     
};

#endif /* YAMARTINO_H */
