using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuadroschrauberSharp.Hardware;

using uint8_t = System.Byte;
using int8_t = System.SByte;
using int16_t = System.Int16;
using uint16_t = System.UInt16;
using uint32_t = System.UInt32;
using define = System.Byte;

namespace MrGibbs.HMC5883
{
    //derived from arduino library of the same name
    //https://github.com/jrowberg/i2cdevlib/tree/master/Arduino/HMC5883L

    public class Hmc5883:IDisposable
    {
        private byte HMC5883L_ADDRESS = 0x1E;
        private byte HMC5883L_DEFAULT_ADDRESS = 0x1E;

        private byte HMC5883L_RA_CONFIG_A = 0x00;
        private byte HMC5883L_RA_CONFIG_B = 0x01;
        private byte HMC5883L_RA_MODE = 0x02;
        private byte HMC5883L_RA_DATAX_H = 0x03;
        private byte HMC5883L_RA_DATAX_L = 0x04;
        private byte HMC5883L_RA_DATAZ_H = 0x05;
        private byte HMC5883L_RA_DATAZ_L = 0x06;
        private byte HMC5883L_RA_DATAY_H = 0x07;
        private byte HMC5883L_RA_DATAY_L = 0x08;
        private byte HMC5883L_RA_STATUS = 0x09;
        private byte HMC5883L_RA_ID_A = 0x0A;
        private byte HMC5883L_RA_ID_B = 0x0B;
        private byte HMC5883L_RA_ID_C = 0x0C;

        private byte HMC5883L_CRA_AVERAGE_BIT = 6;
        private byte HMC5883L_CRA_AVERAGE_LENGTH = 2;
        private byte HMC5883L_CRA_RATE_BIT = 4;
        private byte HMC5883L_CRA_RATE_LENGTH = 3;
        private byte HMC5883L_CRA_BIAS_BIT = 1;
        private byte HMC5883L_CRA_BIAS_LENGTH = 2;

        private byte HMC5883L_AVERAGING_1 = 0x00;
        private byte HMC5883L_AVERAGING_2 = 0x01;
        private byte HMC5883L_AVERAGING_4 = 0x02;
        private byte HMC5883L_AVERAGING_8 = 0x03;

        private byte HMC5883L_RATE_0P75 = 0x00;
        private byte HMC5883L_RATE_1P5 = 0x01;
        private byte HMC5883L_RATE_3 = 0x02;
        private byte HMC5883L_RATE_7P5 = 0x03;
        private byte HMC5883L_RATE_15 = 0x04;
        private byte HMC5883L_RATE_30 = 0x05;
        private byte HMC5883L_RATE_75 = 0x06;

        private byte HMC5883L_BIAS_NORMAL = 0x00;
        private byte HMC5883L_BIAS_POSITIVE = 0x01;
        private byte HMC5883L_BIAS_NEGATIVE = 0x02;

        private byte HMC5883L_CRB_GAIN_BIT = 7;
        private byte HMC5883L_CRB_GAIN_LENGTH = 3;

        private byte HMC5883L_GAIN_1370 = 0x00;
        private byte HMC5883L_GAIN_1090 = 0x01;
        private byte HMC5883L_GAIN_820 = 0x02;
        private byte HMC5883L_GAIN_660 = 0x03;
        private byte HMC5883L_GAIN_440 = 0x04;
        private byte HMC5883L_GAIN_390 = 0x05;
        private byte HMC5883L_GAIN_330 = 0x06;
        private byte HMC5883L_GAIN_220 = 0x07;

        private byte HMC5883L_MODEREG_BIT = 1;
        private byte HMC5883L_MODEREG_LENGTH = 2;

        private byte HMC5883L_MODE_CONTINUOUS = 0x00;
        private byte HMC5883L_MODE_SINGLE = 0x01;
        private byte HMC5883L_MODE_IDLE = 0x02;

        private byte HMC5883L_STATUS_LOCK_BIT = 1;
        private byte HMC5883L_STATUS_READY_BIT = 0;

        private byte _devAddr;
        private byte[] _buffer=new byte[6];
        private byte _mode;
        private I2C _i2c;
        private int _i2cAddress;

        public Hmc5883(int i2cAddress)
        {
            _i2cAddress = i2cAddress;
            _i2c = new I2C(_i2cAddress);
            _devAddr = HMC5883L_DEFAULT_ADDRESS;
        }

        public Hmc5883(int i2cAddress, byte deviceAddress)
        {
            _i2cAddress = i2cAddress;
            _i2c = new I2C(_i2cAddress);
            _devAddr = deviceAddress;
        }

        public void Initialize()
        {
			

            byte data = (byte)(((byte) (HMC5883L_AVERAGING_8 << (HMC5883L_CRA_AVERAGE_BIT - HMC5883L_CRA_AVERAGE_LENGTH + 1))) |
                        ((byte) (HMC5883L_RATE_15 << (HMC5883L_CRA_RATE_BIT - HMC5883L_CRA_RATE_LENGTH + 1))) |
                        ((byte) (HMC5883L_BIAS_NORMAL << (HMC5883L_CRA_BIAS_BIT - HMC5883L_CRA_BIAS_LENGTH + 1))));

            _i2c.writeByte(_devAddr, HMC5883L_RA_CONFIG_A,data);

            // write CONFIG_B register
            SetGain(HMC5883L_GAIN_1370);
    
            // write MODE register
            SetMode(HMC5883L_MODE_CONTINUOUS);
        }

        public bool TestConnection()
        {
            if (_i2c.readBytes(_devAddr, HMC5883L_RA_ID_A, 3, _buffer) == 3) 
            {
                return (_buffer[0] == 'H' && _buffer[1] == '4' && _buffer[2] == '3');
            }
            return false;
        }

        // CONFIG_A register
        public byte GetSampleAveraging()
        {
            var result = _i2c.readBits(_devAddr, HMC5883L_RA_CONFIG_A, HMC5883L_CRA_AVERAGE_BIT, HMC5883L_CRA_AVERAGE_LENGTH);
            return result;
        }

        public void SetSampleAveraging(byte averaging)
        {
            _i2c.writeBits(_devAddr, HMC5883L_RA_CONFIG_A, HMC5883L_CRA_AVERAGE_BIT, HMC5883L_CRA_AVERAGE_LENGTH, averaging);
        }

        public byte GetDataRate()
        {
            var result = _i2c.readBits(_devAddr, HMC5883L_RA_CONFIG_A, HMC5883L_CRA_RATE_BIT, HMC5883L_CRA_RATE_LENGTH);
            return result;
        }

        public void SetDataRate(byte rate)
        {
            _i2c.writeBits(_devAddr, HMC5883L_RA_CONFIG_A, HMC5883L_CRA_RATE_BIT, HMC5883L_CRA_RATE_LENGTH, rate);
        }

        public byte GetMeasurementBias()
        {
            var result = _i2c.readBits(_devAddr, HMC5883L_RA_CONFIG_A, HMC5883L_CRA_BIAS_BIT, HMC5883L_CRA_BIAS_LENGTH);
            return result;
        }

        public void SetMeasurementBias(byte bias)
        {
            _i2c.writeBits(_devAddr, HMC5883L_RA_CONFIG_A, HMC5883L_CRA_BIAS_BIT, HMC5883L_CRA_BIAS_LENGTH, bias);
        }

        // CONFIG_B register
        public byte GetGain()
        {
            var result = _i2c.readBits(_devAddr, HMC5883L_RA_CONFIG_B, HMC5883L_CRB_GAIN_BIT, HMC5883L_CRB_GAIN_LENGTH);
            return result;
        }

        public void SetGain(byte gain)
        {
            _i2c.writeByte(_devAddr, HMC5883L_RA_CONFIG_B, (byte)(gain << (HMC5883L_CRB_GAIN_BIT - HMC5883L_CRB_GAIN_LENGTH + 1)));
        }

        // MODE register
        public byte GetMode()
        {
            var result =_i2c.readBits(_devAddr, HMC5883L_RA_MODE, HMC5883L_MODEREG_BIT, HMC5883L_MODEREG_LENGTH);
            return result;
        }

        public void SetMode(byte newMode)
        {
            _i2c.writeByte(_devAddr, HMC5883L_RA_MODE, (byte)(newMode << (HMC5883L_MODEREG_BIT - HMC5883L_MODEREG_LENGTH + 1)));
            _mode = newMode; // track to tell if we have to clear bit 7 after a read
        }

        // DATA* registers
        public void GetHeading(ref short x, ref short y, ref short z)
        {
            _i2c.readBytes(_devAddr, HMC5883L_RA_DATAX_H, 6, _buffer);
            if (_mode == HMC5883L_MODE_SINGLE)
            {
                _i2c.writeByte(_devAddr, HMC5883L_RA_MODE, (byte)(HMC5883L_MODE_SINGLE << (HMC5883L_MODEREG_BIT - HMC5883L_MODEREG_LENGTH + 1)));
            }

            x= (short)((((int16_t)_buffer[0]) << 8) | _buffer[1]);
            z = (short)((((int16_t)_buffer[2]) << 8) | _buffer[3]);
            y = (short)((((int16_t)_buffer[4]) << 8) | _buffer[5]);


            //x = BitConverter.ToInt16(_buffer, 0);
            //y = BitConverter.ToInt16(_buffer, 4);
            //z = BitConverter.ToInt16(_buffer, 2);
        }

        public short GetHeadingX()
        {
            _i2c.readBytes(_devAddr, HMC5883L_RA_DATAX_H, 6, _buffer);
            if (_mode == HMC5883L_MODE_SINGLE)
            {
                _i2c.writeByte(_devAddr, HMC5883L_RA_MODE, (byte)(HMC5883L_MODE_SINGLE << (HMC5883L_MODEREG_BIT - HMC5883L_MODEREG_LENGTH + 1)));
            }
            return BitConverter.ToInt16(_buffer, 0);
        }

        public short GetHeadingY()
        {
            _i2c.readBytes(_devAddr, HMC5883L_RA_DATAX_H, 6, _buffer);
            if (_mode == HMC5883L_MODE_SINGLE)
            {
                _i2c.writeByte(_devAddr, HMC5883L_RA_MODE, (byte)(HMC5883L_MODE_SINGLE << (HMC5883L_MODEREG_BIT - HMC5883L_MODEREG_LENGTH + 1)));
            }
            return BitConverter.ToInt16(_buffer, 4);
        }

        public short GetHeadingZ()
        {
            _i2c.readBytes(_devAddr, HMC5883L_RA_DATAX_H, 6, _buffer);
            if (_mode == HMC5883L_MODE_SINGLE)
            {
                _i2c.writeByte(_devAddr, HMC5883L_RA_MODE,(byte)(HMC5883L_MODE_SINGLE << (HMC5883L_MODEREG_BIT - HMC5883L_MODEREG_LENGTH + 1)));
            }
            return BitConverter.ToInt16(_buffer, 2);
        }

        // STATUS register
        public bool GetLockStatus()
        {
            var result = _i2c.readBit(_devAddr, HMC5883L_RA_STATUS, HMC5883L_STATUS_LOCK_BIT);
            return ReadBit(result, 1);
        }

        public bool GetReadyStatus()
        {
            var result = _i2c.readBit(_devAddr, HMC5883L_RA_STATUS, HMC5883L_STATUS_READY_BIT);
            return ReadBit(result, 1);
        }

        // ID_* registers
        public byte GetIDA()
        {
            var result = _i2c.readByte(_devAddr, HMC5883L_RA_ID_A);
            return result;
        }

        public byte GetIDB()
        {
            var result = _i2c.readByte(_devAddr, HMC5883L_RA_ID_B);
            return result;
        }

        public byte GetIDC()
        {
            var result = _i2c.readByte(_devAddr, HMC5883L_RA_ID_C);
            return result;
        }

        public void Dispose()
        {
            if (_i2c != null)
            {
                _i2c.Close();
            }
        }

        private bool ReadBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber - 1)) != 0;
        }
    }
}
