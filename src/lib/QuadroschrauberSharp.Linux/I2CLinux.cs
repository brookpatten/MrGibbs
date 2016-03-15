using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if !WINDOWS
using Mono.Unix.Native;
#endif

namespace QuadroschrauberSharp.Linux
{
    public static class LunixNatives
    {
        public const int O_RDWR = 2;

        [DllImport("libc.so.6")]
        extern public static int open(string file, int mode);

        [DllImport("libc.so.6")]
        extern public static int close(int fd);

        [DllImport("libc.so.6")]
        extern public static int ioctl(int fd, int request, byte x);

        public const int I2C_SLAVE = 0x0703;

    }

#if WINDOWS
    static unsafe class Syscall
    {
        public static int open(string file, OpenFlags flags)
        {
            return 1;
        }

        public static ulong write(int fd, void* reg, ulong x)
        {
            return x;
        }

        public static ulong read(int fd, void* reg, ulong x)
        {
            return x;
        }

        public static int close(int fd)
        {
            return 0;
        }
    }

    enum OpenFlags
    {
        O_RDWR
    }
#endif

    public unsafe class I2CLinux
    {
        string device;
        int fd = -1;

        public I2CLinux(int index)
        {
            device = "/dev/i2c-" + index;
            Open();
            //Close();
        }

        public void Open()
        {
            fd = Syscall.open(device, OpenFlags.O_RDWR);
            if (fd < 0)
                throw new IOException(device);
        }

        void IoCtl(byte devAddr)
        {
            int ret = LunixNatives.ioctl(fd, LunixNatives.I2C_SLAVE, devAddr);
            if (ret < 0)
            {
				var exception = new System.ComponentModel.Win32Exception (Marshal.GetLastWin32Error ());
				throw exception;
			}
        }

        public byte readBytes(byte devAddr, byte regAddr, byte length, byte[] data, int offset, ushort timeout = 0)
        {
            if (length > 127)
                throw new IOException(device + ": length > 127");

            //Open();

            IoCtl(devAddr);

            //fixed(byte* p = &regAddr)
            {
                int ret = (int)Syscall.write(fd, &regAddr, 1);
                if (ret != 1)
                    throw new IOException(device + ": write");
            }

            int count;
            fixed (byte* p = &data[offset])
            {
                count = (int)Syscall.read(fd, p, (ulong)length);
                if (count < 0)
                    throw new IOException(device + ": read");
                else if (count != length)
                    throw new IOException(device + ": read short: length = " + length +" > " + count);
            }

            //Close();

            return (byte)count;
        }

        public byte readBytes(byte devAddr, byte regAddr, byte length, byte[] data, ushort timeout = 0)
        {
            return readBytes(devAddr, regAddr, length, data, 0, timeout);
        }

        /** Write multiple bytes to an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr First register address to write to
        * @param length Number of bytes to write
        * @param data Buffer to copy new data from
        * @return Status of operation (true = success)
        */
        public void writeBytes(byte devAddr, byte regAddr, byte length, byte[] data)
        {
            if (length > 127)
                throw new IOException(device + ": length > 127");

            //Open();
            IoCtl(devAddr);

            byte[] buffer = new byte[128];
            buffer[0] = regAddr;
            Array.Copy(data, 0, buffer, 1, length);

            int count;
            fixed (byte* p = buffer)
            {
                count = (int)Syscall.write(fd, p, (ulong)(length + 1));
            }

            if (count < 0)
            {
                throw new IOException(device + ": write = " + count);
            }
            else if (count != length + 1)
            {
                throw new IOException(device + ": write short = " + count);
            }

            //Close();
        }


        /** Write multiple words to a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr First register address to write to
        * @param length Number of words to write
        * @param data Buffer to copy new data from
        * @return Status of operation (true = success)
        */
        public void writeWords(byte devAddr, byte regAddr, byte length, ushort[] data)
        {
            int count = 0;
            byte[] buf = new byte[128];
            int i;

            // Should do potential byteswap and call writeBytes() really, but that
            // messes with the callers buffer

            if (length > 63)
            {
                throw new IOException(device + ": length > 63");
            }

            //Open();
            IoCtl(devAddr);

            buf[0] = regAddr;
            for (i = 0; i < (int)length; i++)
            {
                buf[i * 2 + 1] = (byte)(data[i] >> 8);
                buf[i * 2 + 2] = (byte)data[i];
            }
            fixed (byte* p = buf)
            {
                count = (int)Syscall.write(fd, p, (ulong)(length * 2 + 1));
            }
            if (count < 0)
            {
                throw new IOException(device + ": write");
            }
            else if (count != length * 2 + 1)
            {
                throw new IOException(device + ": write short");
            }
            //Close();
        }

        public void Close()
        {
            int ret = Syscall.close(fd);
            if (ret != 0)
                throw new IOException(device);
        }
    }
}
