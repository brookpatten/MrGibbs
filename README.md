# MrGibbs

[Project Website](http://mrgibbs.io/)

[Project Blog](http://blog.mrgibbs.io/)

[(bad) Circuit Diagrams](https://github.com/brookpatten/MrGibbs/tree/jessie/hw)

Configuration Options(forthcoming)

These instructions are intended to be used on Raspbian Jessie-Lite, although they should be comparable on any BlueZ5 equipped linux distro.

#1. Mono Installation
As of this writing, a nightly or CI build of mono is required as the necassary changes to mono.posix have not made it into a release yet.
* Follow [these intructions](http://www.mono-project.com/docs/getting-started/install/linux/ci-packages/) to install mono.
* You will also need to install [libgcrypt11](https://launchpad.net/ubuntu/+source/libgcrypt11) which does not exist in the very latest ubuntu, so download the deb and install it with dpkg.


#2. Install git
sudo apt-get install git

#3. clone Mr.Gibbs
Don't forget to init submodules

git clone --recursive git://github.com/brookpatten/MrGibbs.git

#4. Change to mono Snapshot 
. mono-snapshot mono

#5 Build
xbuild

#6 Run It
mono gibbs.exe

#7 (Optional but recommended if you're actually going to sail with it) [Set it to run at boot](https://www.raspberrypi.org/documentation/linux/usage/rc-local.md)
