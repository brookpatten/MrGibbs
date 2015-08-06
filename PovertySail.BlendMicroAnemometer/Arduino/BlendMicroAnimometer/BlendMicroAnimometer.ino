#include <SPI.h>
#include <EEPROM.h>
#include <boards.h>
#include <RBL_nRF8001.h>

//ble vars
unsigned char buf[16] = {0};
unsigned char len = 0;

//sensor vars
volatile unsigned long previousAnemometer;
volatile unsigned long latestAnemometer;
volatile unsigned long latestVane;
volatile unsigned long anemometerDifference;
volatile unsigned long vaneDifference;
byte buffer[20];
int anemometerPin=0;
int vanePin=1;

//blend micro supports interrupts on 0,1,2,3 only

void setup() {
  ble_set_name("Anemometer");
  ble_begin();

  pinMode(anemometerPin, INPUT);
  pinMode(vanePin,INPUT);
  
  attachInterrupt(anemometerPin,anemometer,HIGH);
  attachInterrupt(vanePin,vane,HIGH);
  
  Serial.begin(115200);
  Serial.println(F("setup complete"));
}

void loop()
{
  if ( ble_available() )
  {
    while ( ble_available() )
      Serial.write(ble_read());
      
    Serial.println();
  }
  
  //if ( Serial.available() )
  //{
    //delay(5);
    
    //while ( Serial.available() )
        //ble_write( Serial.read() );
  //}
  
  if (ble_connected())
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
    buffer[8] = (byte)255;
    buffer[9] = (byte)255;
    buffer[10] = (byte)255;
    buffer[11] = (byte)255;
    buffer[12] = (byte)66;//B
    buffer[13] = (byte)114;//r
    buffer[14] = (byte)111;//o
    buffer[15] = (byte)111;//o
    buffer[16] = (byte)107;//k
    buffer[17] = (byte)0;
    buffer[18] = (byte)0;
    buffer[19] = (byte)0;
    
    ble_write_bytes(buffer,20);
    
    Serial.write(buffer,20);
    Serial.println(F("Data Sent"));
  }
  ble_do_events();

  delay(1000);
}

void anemometer()
{
  previousAnemometer = latestAnemometer;
  latestAnemometer = millis();
  anemometerDifference = latestAnemometer - previousAnemometer;
}

void vane()
{
  latestVane=millis();
  vaneDifference = latestVane - latestAnemometer;
}

