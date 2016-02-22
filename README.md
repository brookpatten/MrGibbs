# MrGibbs

[Project Website](http://mrgibbs.io/)

[Project Blog](http://blog.mrgibbs.io/)

[(bad) Circuit Diagrams](https://github.com/brookpatten/MrGibbs/tree/jessie/hw)

Configuration Options(forthcoming)

These instructions are intended to be used on Raspbian Jessie-Lite, although they should be comparable on any BlueZ5 equipped linux distro.

#1 Raspbian Setup
* Expand root partition
* Enable I2C (sudo raspi-config)
* Boot to command prompt
* Install Bluetooth (sudo apt-get install bluetooth)
* edit /etc/dbus-1/system.d/bluetooth.conf, add the following

`<policy user="pi">
    <allow own="org.bluez"/>
    <allow send_destination="org.bluez"/>
    <allow send_interface="org.bluez.Agent1"/>
    <allow send_interface="org.bluez.MediaEndpoint1"/>
    <allow send_interface="org.bluez.MediaPlayer1"/>
    <allow send_interface="org.bluez.Profile1"/>
    <allow send_interface="org.freedesktop.DBus.ObjectManager"/>
  </policy>`
* reboot

#2 Mono Installation
As of this writing, a weekly build of mono is required as the necassary changes to mono.posix have not made it into a release yet.  CI builds do not include ArmHF packages so if you're intalling on a Pi, Weekly is the path of least resistance (Compiling mono from git on the pi is very time consuming).
* sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
* echo "deb http://download.mono-project.com/repo/debian nightly main" | sudo tee /etc/apt/sources.list.d/mono-nightly.list
* sudo apt-get update
* sudo apt-get install mono-complete

#3 Install git
* sudo apt-get install git

#4 clone Mr.Gibbs
* git clone --recursive git://github.com/brookpatten/MrGibbs.git

#5 Build
* cd MrGibbs/src
* xbuild

#6 Run It
* ./start.sh

#7 [Set it to run at boot](https://www.raspberrypi.org/documentation/linux/usage/rc-local.md)
