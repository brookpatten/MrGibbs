using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uint8_t = System.Byte;
using uint16_t = System.UInt16;
using int8_t = System.Byte;
using QuadroschrauberSharp.Linux;

namespace QuadroschrauberSharp.Hardware
{
    public class I2C : I2CLinux
    {
        public I2C(int index) : base(index)
        {
        }

        public byte[] Read(byte devAddr, byte regAddr, byte length, ushort timeout = 0)
        {
            byte[] buffer = new byte[length];
            byte ret = readBytes(devAddr, regAddr, length, buffer, timeout);
            return buffer;
        }




        /** Read a single bit from an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to read from
        * @param bitNum Bit position to read (0-7)
        * @param data Container for single bit value
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Status of read operation (true = success)
        */
        public uint8_t readBit(uint8_t devAddr, uint8_t regAddr, uint8_t bitNum, uint16_t timeout = 0)
        {
            uint8_t b = readByte(devAddr, regAddr, timeout);
            return (byte)(b & (1 << bitNum));
        }
        public bool readBitB(uint8_t devAddr, uint8_t regAddr, uint8_t bitNum, uint16_t timeout = 0)
        {
            return readBit(devAddr, regAddr, bitNum, timeout) != 0;
        }

        /** Read a single bit from a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to read from
        * @param bitNum Bit position to read (0-15)
        * @param data Container for single bit value
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Status of read operation (true = success)
        */
        public uint16_t readBitW(uint8_t devAddr, uint8_t regAddr, uint8_t bitNum, uint16_t timeout)
        {
            uint16_t b;
            b = ReadWord(devAddr, regAddr, timeout);
            return (ushort)(b & (1 << bitNum));
        }

        /** Read multiple bits from an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to read from
        * @param bitStart First bit position to read (0-7)
        * @param length Number of bits to read (not more than 8)
        * @param data Container for right-aligned value (i.e. '101' read from any bitStart position will equal 0x05)
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Status of read operation (true = success)
        */
        public uint8_t readBits(uint8_t devAddr, uint8_t regAddr, uint8_t bitStart, uint8_t length, uint16_t timeout = 0)
        {
            // 01101001 read byte
            // 76543210 bit numbers
            // xxx args: bitStart=4, length=3
            // 010 masked
            // -> 010 shifted
            uint8_t count, b;
            b = readByte(devAddr, regAddr, timeout);
            uint8_t mask = (byte)(((1 << length) - 1) << (bitStart - length + 1));
            b &= mask;
            b >>= (bitStart - length + 1);
            return (byte)b;
        }

        public uint8_t readBits(uint8_t devAddr, int regAddr, uint8_t bitStart, uint8_t length, uint16_t timeout = 0)
        {
            return readBits(devAddr, (byte)regAddr, bitStart, length, timeout);
        }

        /** Read multiple bits from a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to read from
        * @param bitStart First bit position to read (0-15)
        * @param length Number of bits to read (not more than 16)
        * @param data Container for right-aligned value (i.e. '101' read from any bitStart position will equal 0x05)
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Status of read operation (1 = success, 0 = failure, -1 = timeout)
        */
        public uint16_t readBitsW(uint8_t devAddr, uint8_t regAddr, uint8_t bitStart, uint8_t length, uint16_t timeout)
        {
            // 1101011001101001 read byte
            // fedcba9876543210 bit numbers
            // xxx args: bitStart=12, length=3
            // 010 masked
            // -> 010 shifted
            uint8_t count;
            uint16_t w = ReadWord(devAddr, regAddr, timeout);
            uint16_t mask = (ushort)(((1 << length) - 1) << (bitStart - length + 1));
            w &= mask;
            w >>= (bitStart - length + 1);
            return w;
        }

        public byte readBytes(byte devAddr, int regAddr, byte length, byte[] data, ushort timeout = 0)
        {
            return base.readBytes(devAddr, (byte)regAddr, length, data, timeout);
        }

        /** Read single byte from an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to read from
        * @param data Container for byte value read from device
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Status of read operation (true = success)
        */
        public uint8_t readByte(uint8_t devAddr, uint8_t regAddr, uint16_t timeout = 0)
        {
            return Read(devAddr, regAddr, 1, timeout)[0];
        }
        public uint8_t readByte(uint8_t devAddr, int regAddr, uint16_t timeout = 0)
        {
            return Read(devAddr, (byte)regAddr, 1, timeout)[0];
        }

        /** Read single word from a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to read from
        * @param data Container for word value read from device
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Status of read operation (true = success)
        */
        public uint16_t ReadWord(uint8_t devAddr, uint8_t regAddr, uint16_t timeout = 0)
        {
            return readWords(devAddr, regAddr, 1, timeout)[0];
        }

        /** Read multiple words from a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr First register regAddr to read from
        * @param length Number of words to read
        * @param data Buffer to store read data in
        * @param timeout Optional read timeout in milliseconds (0 to disable, leave off to use default class value in I2Cdev::readTimeout)
        * @return Number of words read (0 indicates failure)
        */
        public uint16_t[] readWords(byte devAddr, byte regAddr, byte length, ushort timeout)
        {
            throw new Exception("NYI");
            //int8_t count = 0;

            //printf("ReadWords() not implemented\n");
            // Use readBytes() and potential byteswap
            //*data = 0; // keep the compiler quiet

            //return count;
        }

        /** write a single bit in an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to write to
        * @param bitNum Bit position to write (0-7)
        * @param value New bit value to write
        * @return Status of operation (true = success)
        */
        public void writeBit(byte devAddr, byte regAddr, byte bitNum, byte data)
        {
            byte b = readByte(devAddr, regAddr);
            b = (byte)((data != 0) ? (b | (1 << bitNum)) : (b & ~(1 << bitNum)));
            writeByte(devAddr, regAddr, b);
        }

        public void writeBit(byte devAddr, byte regAddr, byte bitNum, bool data2)
        {
            byte data = data2 ? (byte)1 : (byte)0;
            byte b = readByte(devAddr, regAddr);
            b = (byte)((data != 0) ? (b | (1 << bitNum)) : (b & ~(1 << bitNum)));
            writeByte(devAddr, regAddr, b);
        }

        /** write a single bit in a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to write to
        * @param bitNum Bit position to write (0-15)
        * @param value New bit value to write
        * @return Status of operation (true = success)
        */
        public void writeBitW(byte devAddr, byte regAddr, byte bitNum, ushort data)
        {
            ushort w = ReadWord(devAddr, regAddr);
            w = (ushort)((data != 0) ? (w | (1 << bitNum)) : (w & ~(1 << bitNum)));
            writeWord(devAddr, regAddr, w);
        }

        /** Write multiple bits in an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to write to
        * @param bitStart First bit position to write (0-7)
        * @param length Number of bits to write (not more than 8)
        * @param data Right-aligned value to write
        * @return Status of operation (true = success)
        */
        public void writeBits(byte devAddr, byte regAddr, byte bitStart, byte length, byte data)
        {
            // 010 value to write
            // 76543210 bit numbers
            // xxx args: bitStart=4, length=3
            // 00011100 mask byte
            // 10101111 original value (sample)
            // 10100011 original & ~mask
            // 10101011 masked | value
            byte b = readByte(devAddr, regAddr);
            byte mask = (byte)(((1 << length) - 1) << (bitStart - length + 1));
            data <<= (bitStart - length + 1); // shift data into correct position
            data &= mask; // zero all non-important bits in data
            b &= (byte)~(mask); // zero all important bits in existing byte
            b |= data; // combine data with existing byte
            writeByte(devAddr, regAddr, b);
        }

        public void writeBits(byte devAddr, byte regAddr, byte bitStart, byte length, sbyte data)
        {
            writeBits(devAddr, regAddr, bitStart, length, unchecked((byte)data));
        }

        public void writeBits(byte devAddr, int regAddr, byte bitStart, byte length, byte data)
        {
            writeBits(devAddr, (byte)regAddr, bitStart, length, data);
        }

        /** Write multiple bits in a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register regAddr to write to
        * @param bitStart First bit position to write (0-15)
        * @param length Number of bits to write (not more than 16)
        * @param data Right-aligned value to write
        * @return Status of operation (true = success)
        */
        void writeBitsW(byte devAddr, byte regAddr, byte bitStart, byte length, ushort data)
        {
            // 010 value to write
            // fedcba9876543210 bit numbers
            // xxx args: bitStart=12, length=3
            // 0001110000000000 mask byte
            // 1010111110010110 original value (sample)
            // 1010001110010110 original & ~mask
            // 1010101110010110 masked | value
            ushort w = ReadWord(devAddr, regAddr);
            byte mask = (byte)(((1 << length) - 1) << (bitStart - length + 1));
            data <<= (bitStart - length + 1); // shift data into correct position
            data &= mask; // zero all non-important bits in data
            w &= (ushort)~(mask); // zero all important bits in existing word
            w |= data; // combine data with existing word
            writeWord(devAddr, regAddr, w);
        }


        public void Write(byte devAddr, byte regAddr, byte[] data)
        {
            writeBytes(devAddr, regAddr, (byte)data.Length, data);
        }

        /** Write single byte to an 8-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register address to write to
        * @param data New byte value to write
        * @return Status of operation (true = success)
        */
        public void writeByte(byte devAddr, byte regAddr, byte data)
        {
            Write(devAddr, regAddr, new byte[] { data });
        }

        public void writeByte(byte devAddr, int regAddr, byte data)
        {
            writeByte(devAddr, (byte)regAddr, data);
        }
        public void writeByte(byte devAddr, byte regAddr, sbyte data)
        {
            writeByte(devAddr, regAddr, unchecked((sbyte)data));
        }
        /** Write single word to a 16-bit device register.
        * @param devAddr I2C slave device address
        * @param regAddr Register address to write to
        * @param data New word value to write
        * @return Status of operation (true = success)
        */
        public void writeWord(byte devAddr, byte regAddr, ushort data)
        {
            writeWords(devAddr, regAddr, 1, new ushort[] { data });
        }

        public void writeWord(byte devAddr, byte regAddr, short data)
        {
            writeWord(devAddr, regAddr, (ushort)data);
        }

    }
}
