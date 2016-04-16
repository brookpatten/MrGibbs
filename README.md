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
* Fast boot (don't wait for network)
* Enable SSH (if you want it)
* Boot to command prompt
* Update kernel
```
sudo apt-get install rpi-update
sudo rpi-update
sudo reboot
```

#2 Install & Configure BlueZ from source
```
sudo apt-get update
sudo apt-get install git build-essential autoconf cmake libtool libglib2.0 libdbus-1-dev libudev-dev libical-dev libreadline-dev
wget http://www.kernel.org/pub/linux/bluetooth/bluez-5.39.tar.xz
tar xvf bluez-5.39.tar.xz 
cd bluez-5.39/
./configure --prefix=/usr --mandir=/usr/share/man --sysconfdir=/etc --localstatedir=/var --enable-experimental --with-systemdsystemunitdir=/lib/systemd/system --with-systemduserunitdir=/usr/lib/systemd
make
sudo make install
```

Edit /etc/dbus-1/system.d/bluetooth.conf, add the following

```
<policy user="pi">
    <allow own="org.bluez"/>
    <allow send_destination="org.bluez"/>
    <allow send_interface="org.bluez.Agent1"/>
    <allow send_interface="org.bluez.MediaEndpoint1"/>
    <allow send_interface="org.bluez.MediaPlayer1"/>
    <allow send_interface="org.bluez.ThermometerWatcher1"/>
    <allow send_interface="org.bluez.AlertAgent1"/>
    <allow send_interface="org.bluez.Profile1"/>
    <allow send_interface="org.bluez.HeartRateWatcher1"/>
    <allow send_interface="org.bluez.CyclingSpeedWatcher1"/>
    <allow send_interface="org.bluez.GattCharacteristic1"/>
    <allow send_interface="org.bluez.GattDescriptor1"/>
    <allow send_interface="org.freedesktop.DBus.ObjectManager"/>
    <allow send_interface="org.freedesktop.DBus.Properties"/>
</policy>
```
Edit /etc/systemd/system/bluetooth.target.wants/bluetooth.service.
Change the line
```
ExecStart=/usr/libexec/bluetooth/bluetoothd
```
to
```
ExecStart=/usr/libexec/bluetooth/bluetoothd --experimental
```

Start up bluetooth

```
sudo systemctl daemon-reload
sudo hciconfig hci0 down
sudo service bluetooth start
sudo hciconfig hci0 up
```

#3A (Raspberry Pi 2/3) Mono Installation
As of this writing, a weekly build of mono is required as the necassary changes to mono.posix have not made it into a release yet.  CI builds do not include ArmHF packages so if you're intalling on a Pi, Weekly is the path of least resistance (Compiling mono from git on the pi is very time consuming).
```
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian alpha main" | sudo tee /etc/apt/sources.list.d/mono-alpha.list
sudo apt-get update
sudo apt-get install mono-devel
```

#3B (Raspberry Pi A/B/+/Zero) Mono Compilation 
```
sudo apt-get install autoconf libtool automake build-essential gettext libtool-bin
#if you want to speed things up you could limit the depth
git clone --recursive https://github.com/mono/mono.git
cd mono
./autogen.sh --prefix=/usr/local
make get-monolite-latest
#(This will take about 6 hours)
make 
sudo make install
cd ..
```

#4 Build Mr.Gibbs
```
git clone --recursive git://github.com/brookpatten/MrGibbs.git
cd MrGibbs/src
xbuild
```

#5 Run It
```
./run.sh
```
#6 [Set it to run at boot](https://www.raspberrypi.org/documentation/linux/usage/rc-local.md)

# Gibbs.exe.config Configuration Options
Most values have sensible defaults, but if you want to tweak things, this is where you do it.
```
<appSettings>
    <!--Enable Bluetooth discovery at startup to find any new devices-->
	<add key="BtEnableDiscovery" value="True"/>
	<!--How long to wait for new devices to appear before continuing startup-->
	<add key="BtDiscoveryWait" value="10"/>
	<!--which bt adapter to use-->
	<add key="BtAdapterName" value="hci0"/>
	<!--simulates sensor data, currently only works with gps and wind-->
	<add key="SimulateSensorData" value="False"/>
	<!--mac address of wind vane to use-->
	<add key="BlendMicroAnemometerAddress" value=""/>
	<!--device file for gps serial port-->
	<add key="GpsPort" value="/dev/ttyAMA0"/>
	<!--gps serial port baud rate-->
	<add key="GpsBaud" value="9600"/>
	<!--which i2c bus to use.  Newer pis all use 1, older ones use 0-->
	<add key="I2CAddress" value="1"/>
	<!--which file to use for magnetic deviation, defaults to newest cof file in exe dir-->
	<add key="cofFilePath" value="/home/brook/Desktop/MrGibbs/src/MrGibbs/bin/Debug/WMM.COF"/>
	<!--which pbw to send to pebbles, defaults ot newest in exe dir-->
	<add key="PbwPath" value="/home/brook/Desktop/MrGibbs/src/MrGibbs/bin/Debug/Mr._Gibbs.pbw"/>
	<!--where to write data files (sqlite), defaults to exe dir-->
	<add key="DataPath" value="/home/brook/Desktop/MrGibbs/src/MrGibbs/bin/Debug"/>
	<!--distance to mark at which mr gibbs will auto advance the "next" mark-->
	<add key="AutoRoundMarkDistanceMeters" value="30"/>
	<!--how long each iteration should take-->
	<add key="TargetCycleTime" value="1000"/>
</appSettings>
```
