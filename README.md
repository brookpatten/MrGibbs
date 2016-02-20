# MrGibbs

[Project Website](http://mrgibbs.io/)

[Project Blog](http://blog.mrgibbs.io/)

[(bad) Circuit Diagrams](https://github.com/brookpatten/MrGibbs/tree/jessie/hw)

Configuration Options(forthcoming)

These instructions are intended to be used on Raspbian Jessie-Lite, although they should be comparable on any BlueZ5 equipped linux distro.

#1 Raspbian Setup
* Install Bluetooth (sudo apt-get install bluetooth)
* Enable I2C (sudo raspi-config)

#2 Mono Installation
As of this writing, a weekly build of mono is required as the necassary changes to mono.posix have not made it into a release yet.  CI builds do not include ArmHF packages so if you're intalling on a Pi, Weekly is the path of least resistance (Compiling mono from git on the pi is very time consuming).
* Follow [these intructions](http://www.mono-project.com/docs/getting-started/install/weekly-packages/) to install mono.

#3 Install git
* sudo apt-get install git

#4 clone Mr.Gibbs
* git clone --recursive git://github.com/brookpatten/MrGibbs.git

#5 Build
* cd MrGibbs/src
* xbuild

#6 Run It
* ./start.sh

#7 (Optional but recommended if you're actually going to sail with it) 
* [Set it to run at boot](https://www.raspberrypi.org/documentation/linux/usage/rc-local.md)
