//NOTE I am using the Lock-out method, you need to uncomment this line in bounce2.h
//#define BOUNCE_LOCK_OUT
#include <Bounce2.h>
#include <RBL_nRF8001.h>
#include <Wire.h>
#include "Yamartino.h"

//mpu 6050 vars
const int MPU_addr=0x68;  // I2C address of the MPU-6050
int16_t AcX,AcY,AcZ,Tmp,GyX,GyY,GyZ;
const int bleLedPin=13;

//heartbeat
unsigned long now;
unsigned long heartbeat;
unsigned long lastSentAtHeartbeat;
unsigned long lastHeartbeatAt;
unsigned long heartbeatInterval=1000;

//buffers for serializing/sending data
byte buffer[20];
unsigned char tobytes[4];

//wind sensor state
unsigned long previousAnemometer;
unsigned long latestAnemometer;
unsigned long anemometerDifference;

unsigned long latestVane;
unsigned long vaneDifference;

//buffering/averaging
int bufferCount=0;
unsigned long anemometerBuffer;
Yamartino vaneBuffer(5);

//wind sensor hardware
const int anemometerPin=0;
const int vanePin=1;

const int anemometerLedPin=10;
bool anemometerLedStatus=false;

//wind sensor debounce
const unsigned long debounce=5;
Bounce anemometerBounce = Bounce(); 
Bounce vaneBounce = Bounce(); 

void setup() {
  //serial
  //Serial.begin(9600); 
  
  //bt setup
  ble_set_name("MrGibbs");
  ble_begin();

  //mpu 6050 setup
  Wire.begin();
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B);  // PWR_MGMT_1 register
  Wire.write(0);     // set to zero (wakes up the MPU-6050)
  Wire.endTransmission(true);

  //wind setup
  heartbeat=0;
  lastHeartbeatAt=0;

  //LED setup
  pinMode(anemometerLedPin, OUTPUT);
  pinMode(bleLedPin,OUTPUT);

  //anemometer setup
  pinMode(anemometerPin, INPUT_PULLUP);
  anemometerBounce.attach(anemometerPin);
  anemometerBounce.interval(debounce);

  //vane setup
  pinMode(vanePin,INPUT_PULLUP);
  vaneBounce.attach(vanePin);
  vaneBounce.interval(debounce);
  
}

void loop()
{
  //if(true)
  if(ble_connected())
  {
    digitalWrite(bleLedPin,HIGH);

    now=micros();
    
    anemometerBounce.update();
    vaneBounce.update();
    
    if(anemometerBounce.rose()==1)
    {
      anemometer(now);
    }
    
    if(vaneBounce.rose()==1)
    {
      //it is intentional that we don't re-get micros because when the switches overlap the closures can occur 
      //at the exact same time.  Via testing I decided that getting the micros once and re-using yeilds better results
      vane(now);
    }

    //handle the overflow of micros (every 70 min) by just resetting if "now" is more than 30 minutes before the last heartbeat
    if(now < lastHeartbeatAt - (30 * 60 * 1000 * 1000))
    {
      lastHeartbeatAt=0;
    }

    if(now > lastHeartbeatAt + (heartbeatInterval * 1000))
    {
      if(heartbeat != lastSentAtHeartbeat)
      {
        if(bufferCount>0)//if there's no buffer yet we keep waiting for data
        {
          anemometerLedStatus = !anemometerLedStatus;
          digitalWrite(anemometerLedPin, anemometerLedStatus); 
          
          processMpuData();
          writeBleData();
          lastSentAtHeartbeat=heartbeat;
  
          anemometerBuffer=0;
          bufferCount=-1;//set it to -1 to indicate we intend to "skip" the first measurement to avoid measuring the bt send time
        }
      }
  
      ble_do_events();
      
      lastHeartbeatAt=now;
    }
  }
  else
  {
    //aka sleep mode
    //TODO: figure out how to actually go low power
    digitalWrite(bleLedPin,LOW);
    digitalWrite(anemometerLedPin,LOW);
    delay(1000);
    ble_do_events();
  }
}

void writeBleData()
{
      //make a byte array that we can send via uart, terminate with 1s

      unsigned long anemometer = anemometerBuffer / bufferCount;
      
      memcpy(tobytes,&anemometer,sizeof(long int));
      buffer[0] = tobytes[0];
      buffer[1] = tobytes[1];
      buffer[2] = tobytes[2];
      buffer[3] = tobytes[3];

      float vane = vaneBuffer.averageHeading();
      //Serial.print("a=");
      //Serial.print(vane);
      //Serial.println();
      
      memcpy(tobytes,&vane,sizeof(float));
      buffer[4] = tobytes[0];
      buffer[5] = tobytes[1];
      buffer[6] = tobytes[2];
      buffer[7] = tobytes[3];

      memcpy(tobytes,&AcX,sizeof(int16_t));
      buffer[8]=tobytes[0];
      buffer[9]=tobytes[1];

      memcpy(tobytes,&AcY,sizeof(int16_t));
      buffer[10]=tobytes[0];
      buffer[11]=tobytes[1];

      memcpy(tobytes,&AcZ,sizeof(int16_t));
      buffer[12]=tobytes[0];
      buffer[13]=tobytes[1];
      
      buffer[14] = (byte)0;
      buffer[15] = (byte)0;

      ble_write_bytes(buffer,15);

}

void processMpuData()
{
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
  Wire.endTransmission(false);
  Wire.requestFrom(MPU_addr,14,true);  // request a total of 14 registers
  AcX=Wire.read()<<8|Wire.read();  // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)    
  AcY=Wire.read()<<8|Wire.read();  // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
  AcZ=Wire.read()<<8|Wire.read();  // 0x3F (ACCEL_ZOUT_H) & 0x40 (ACCEL_ZOUT_L)
  Tmp=Wire.read()<<8|Wire.read();  // 0x41 (TEMP_OUT_H) & 0x42 (TEMP_OUT_L)
  GyX=Wire.read()<<8|Wire.read();  // 0x43 (GYRO_XOUT_H) & 0x44 (GYRO_XOUT_L)
  GyY=Wire.read()<<8|Wire.read();  // 0x45 (GYRO_YOUT_H) & 0x46 (GYRO_YOUT_L)
  GyZ=Wire.read()<<8|Wire.read();  // 0x47 (GYRO_ZOUT_H) & 0x48 (GYRO_ZOUT_L)
}

void anemometer(unsigned long now)
{
  previousAnemometer = latestAnemometer;
  latestAnemometer = now;
  anemometerDifference = latestAnemometer - previousAnemometer;

  if(latestVane!=0)
  {
    vaneDifference = latestVane - previousAnemometer;
  }
  else
  {
    //it's possible that we "missed" a vane switch
    vaneDifference = anemometerDifference;
  }

  float vane = ((float)vaneDifference / (float)anemometerDifference)*(float)360.0;
  //Serial.print("r=");
  //Serial.print(vane);
  //Serial.println();
  
  bufferCount++;
  if(bufferCount>0){
    anemometerBuffer += anemometerDifference;
    vaneBuffer.add(vane);
  }

  latestVane=0;
  
  heartbeat++;
}

void vane(unsigned long now)
{
  latestVane=now;
}



