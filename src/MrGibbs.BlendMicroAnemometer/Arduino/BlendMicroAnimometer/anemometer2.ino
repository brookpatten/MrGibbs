//#include <SPI.h>
//#include <EEPROM.h>
//#include <boards.h>
#include <RBL_nRF8001.h>
#include<Wire.h>

//ble vars
const bool enableBle=true;
unsigned char buf[50] = {0};
unsigned char len = 0;

//mpu 6050 vars
const int MPU_addr=0x68;  // I2C address of the MPU-6050
int16_t AcX,AcY,AcZ,Tmp,GyX,GyY,GyZ;
const int bleLedPin=10;

//wind sensor vars
volatile unsigned long previousAnemometer;
volatile unsigned long latestAnemometer;
volatile unsigned long anemometerDifference;
volatile unsigned long latestAnemometerBounce;

volatile unsigned long latestVane;
volatile unsigned long vaneDifference;
volatile unsigned long latestVaneBounce;

volatile unsigned long heartbeat;
unsigned long lastPrintHeartbeat;
byte buffer[20];
const unsigned long debounce=50;

//uno pins/interrupts
//const int anemometerPin=3;
//const int anemometerInterrupt=1;
//const int anemometerLedPin=13;
//const int vanePin=2;
//const int vaneInterrupt=0;
//const int vaneLedPin=12;

//blend micro pins/interrupts
const int anemometerPin=1;
const int anemometerInterrupt=3;
const int anemometerLedPin=12;
bool anemometerLedStatus=false;
const int vanePin=0;
const int vaneInterrupt=2;
const int vaneLedPin=13;
bool vaneLedStatus=false;

void setup() {
  //logging setup
  Serial.begin(9600);
  
  //bt setup
  if(enableBle)
  {
    ble_set_name("MrGibbs");
    ble_begin();
    Serial.println(F("BLE initialized"));
  }
  pinMode(bleLedPin,OUTPUT);

  //mpu 6050 setup
  Wire.begin();
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B);  // PWR_MGMT_1 register
  Wire.write(0);     // set to zero (wakes up the MPU-6050)
  Wire.endTransmission(true);
  Serial.println(F("MPU initialized"));

  //wind setup
  heartbeat=0;
  lastPrintHeartbeat=0;
  pinMode(vaneLedPin, OUTPUT);
  pinMode(anemometerLedPin, OUTPUT);
  pinMode(anemometerPin, INPUT_PULLUP);
  pinMode(vanePin,INPUT_PULLUP);
  attachInterrupt(anemometerInterrupt,anemometer,CHANGE);
  attachInterrupt(vaneInterrupt,vane,CHANGE);
  Serial.println(F("Anemometer initialized"));

  Serial.println(F("Startup Complete"));
}

void loop()
{
  //if(heartbeat!=lastPrintHeartbeat)
  {
    if(!enableBle || ble_connected())
    {
      processMpuData();
      if(enableBle && ble_connected)
      {
        digitalWrite(bleLedPin,HIGH);
        writeBleData();
      }
      else
      {
        digitalWrite(bleLedPin,LOW);
      }
      //if(Serial.available())
      {
        writeSerialData();
      }
      lastPrintHeartbeat=heartbeat;
    }
    else
    {
      digitalWrite(bleLedPin,LOW);
    }
  }
  if(enableBle)
  {
    ble_do_events();
  }
  
  //digitalWrite(vaneLedPin, LOW); 
  //digitalWrite(anemometerLedPin, LOW); 
  delay(1000);
}

void writeSerialData()
{
  Serial.print("hb= ");
  Serial.print(heartbeat);
  Serial.print(", xyz=");
  Serial.print(AcX);
  Serial.print(",");
  Serial.print(AcY);
  Serial.print(",");
  Serial.print(AcZ);
  //Serial.print(",pa= ");
  //Serial.print(previousAnemometer);
  //Serial.print(",la= ");
  //Serial.print(latestAnemometer);
  //Serial.print(",ad= ");
  //Serial.print(anemometerDifference);
  //Serial.print(",lv=");
  //Serial.print(latestVane);
  //Serial.print(",vd=");
  //Serial.print(vaneDifference);
  Serial.print(", a=");
  Serial.print(angleDegrees(vaneDifference,anemometerDifference));
  Serial.print(", s=");
  Serial.println(speed(anemometerDifference));
}

void writeBleData()
{
      //make a byte array that we can send via uart, terminate with 1s
      buffer[0] = (byte) anemometerDifference;
      buffer[1] = (byte) anemometerDifference >> 8;
      buffer[2] = (byte) anemometerDifference >> 16;
      buffer[3] = (byte) anemometerDifference >> 24;
      buffer[4] = (byte) vaneDifference;
      buffer[5] = (byte) vaneDifference >> 8;
      buffer[6] = (byte) vaneDifference >> 16;
      buffer[7] = (byte) vaneDifference >> 24;
      buffer[8] = (byte) lowByte(AcX);
      buffer[9] = (byte) highByte(AcX);
      buffer[10] = (byte) lowByte(AcY);
      buffer[11] = (byte) highByte(AcY);
      buffer[12] = (byte) lowByte(AcZ);
      buffer[13] = (byte) highByte(AcZ);
      buffer[14] = (byte)0;
      buffer[15] = (byte)0;
      buffer[16] = (byte)0;
      buffer[17] = (byte)0;
      buffer[18] = (byte)0;
      buffer[19] = (byte)0;

    //buffer[0] = (byte)0;
    //buffer[1] = (byte)1;
    //buffer[2] = (byte)2;
    //buffer[3] = (byte)3;
    //buffer[4] = (byte)4;
    //buffer[5] = (byte)5;
    //buffer[6] = (byte)6;
    //buffer[7] = (byte)7;
    //buffer[8] = (byte)8;
    //buffer[9] = (byte)9;
    
    ble_write_bytes(buffer,15);

      //String stringOne =  String("speed="+String((int)speed(anemometerDifference))+", angle="+String((int)angleDegrees(vaneDifference,anemometerDifference))+", xyz="+String(AcX)+","+String(AcY)+","+String(AcZ)+"\n"); 
      //stringOne.getBytes(buffer,stringOne.length());
      
      //ble_write_bytes(buffer,stringOne.length());
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

void anemometer()
{
  unsigned long now = millis();
  if(now>latestAnemometerBounce+debounce && digitalRead(anemometerPin)==HIGH)
  {
    anemometerLedStatus = !anemometerLedStatus;
    digitalWrite(anemometerLedPin, anemometerLedStatus); 
    previousAnemometer = latestAnemometer;
    latestAnemometer = now;
    anemometerDifference = latestAnemometer - previousAnemometer;
    heartbeat++;
  }
  latestAnemometerBounce=now;
}

void vane()
{
  unsigned long now = millis();
  if(now > latestVaneBounce + debounce && digitalRead(vanePin)==HIGH)
  {
    vaneLedStatus = !vaneLedStatus;
    digitalWrite(vaneLedPin, vaneLedStatus);
    latestVane=now;
    if(latestVane>=latestAnemometer)
    {
      vaneDifference = latestVane - latestAnemometer;
    }
    else if(latestVane>=previousAnemometer)
    {
      vaneDifference = latestVane - previousAnemometer;
    }
    else
    {
      //this shouldn't happen
      vaneDifference = 0;
    }
  }
  latestVaneBounce=now;
}

//given ms between closures, returns speed in mph
//per peet bros ultimeter pro documentation
float speed(long closureRate)
{
  float rps = 1000.0/(float)closureRate;
  
  if(0.010 < rps && rps < 3.23)
  {
    return -0.1095*(rps*rps) + 2.9318*rps - 0.1412;
  }
  else if(3.23 <= rps && rps <54.362)
  {
    return 0.0052 * (rps * rps) + 2.1980 * rps + 1.1091;
  }
  else if(54.362 <= rps && rps < 66.332)
  {
    return 0.1104 * (rps * rps) - 9.5685 * rps + 329.87;
  }
  else
  {
    return 0.0;
  }
}

float angleDegrees(long vaneDifference,long anemometerDifference)
{
  return ((float)vaneDifference/(float)anemometerDifference)*360.0;
}

