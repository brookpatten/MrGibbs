
This program is based on v4l API and some fswebcam source headers

With RapberryCam, you can take pictures and videos from a webcam pluged in your Raspberry pi.

For the moment, the video stream is not a MPEG stream, but just a succession of compressed frames.

In the futur version, a real MPEG format should be implemented.

Before trying it in Mono, please copy C sources files on your Rapsberry pi and type:

I. Install RapberryCam.so

#sudo su
#make
#make install

Or :

just copy Lib/RapberryCam.so in /Lib


II. Run ServerExample on your Rapsberry pi

mono RaspberryCam.ServerExample.exe

III. Run RaspberryCam.VideoViewer.exe on your PC
