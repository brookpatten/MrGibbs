# MrGibbs

[![Build Status](https://travis-ci.org/brookpatten/MrGibbs.svg?branch=master)](https://travis-ci.org/brookpatten/MrGibbs)

[Project Website](http://mrgibbs.io/)

[Project Blog](http://blog.mrgibbs.io/)

[(bad) Circuit Diagrams](https://github.com/brookpatten/MrGibbs/tree/master/hw)

Configuration Options(forthcoming)

These instructions are intended to be used on Raspbian Jessie-Lite, although they should be comparable on any BlueZ5 equipped linux distro.

#1 Raspbian Setup
* Expand root partition
* Enable I2C (sudo raspi-config)
* Boot to command prompt
* Add Stretch sources to /etc/apt/sources.list
```
deb http://mirrordirector.raspbian.org/raspbian/ jessie main contrib non-free r$
# Uncomment line below then 'apt-get update' to enable 'apt-get source'
deb-src http://archive.raspbian.org/raspbian/ jessie main contrib non-free rpi

deb http://mirrordirector.raspbian.org/raspbian/ stretch main contrib non-free $
# Uncomment line below then 'apt-get update' to enable 'apt-get source'
deb-src http://archive.raspbian.org/raspbian/ stretch main contrib non-free rpi
```
* set Jessie as the default distro by editing /etc/apt/apt.conf.d/40defaultrelease
```
APT::Default-Release "jessie";
```
* Install bluetooth from stretch
`apt-get install bluez -t stretch`
* edit /etc/dbus-1/system.d/bluetooth.conf, add the following

```
<policy user="pi">
    <allow own="org.bluez"/>
    <allow send_destination="org.bluez"/>
    <allow send_interface="org.bluez.Agent1"/>
    <allow send_interface="org.bluez.MediaEndpoint1"/>
    <allow send_interface="org.bluez.MediaPlayer1"/>
    <allow send_interface="org.bluez.Profile1"/>
    <allow send_interface="org.freedesktop.DBus.ObjectManager"/>
</policy>
```
* revert kernel
```
sudo apt-get install rpi-update
sudo rpi-update 46d179597370c5145c7452796acbee0f1ff93392
```
* reboot

#2 Install git
* sudo apt-get install git

#3A (Raspberry Pi 2) Mono Installation
As of this writing, a weekly build of mono is required as the necassary changes to mono.posix have not made it into a release yet.  CI builds do not include ArmHF packages so if you're intalling on a Pi, Weekly is the path of least resistance (Compiling mono from git on the pi is very time consuming).
* sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
* echo "deb http://download.mono-project.com/repo/debian nightly main" | sudo tee /etc/apt/sources.list.d/mono-nightly.list
* sudo apt-get update
* sudo apt-get install mono-snapshot-latest
* . mono-snapshot mono
 
#3B (Raspberry Pi A/B/+/Zero) Mono Compilation 
* git clone https://github.com/mono/mono.git
* sudo apt-get install autoconf libtool automake build-essential gettext
* cd mono
* ./autogen.sh --prefix=/usr/local
* make get-monolite-latest
* make (This will take about 4 hours)
* sudo make install
* cd ..

#4 clone Mr.Gibbs
* git clone --recursive git://github.com/brookpatten/MrGibbs.git

#5 Build
* cd MrGibbs/src
* xbuild

#6 Run It
* ./start.sh

#7 [Set it to run at boot](https://www.raspberrypi.org/documentation/linux/usage/rc-local.md)
