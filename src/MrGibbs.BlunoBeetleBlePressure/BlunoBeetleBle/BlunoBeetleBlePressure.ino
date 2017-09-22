#include <Adafruit_Sensor.h>
#include <Adafruit_BMP280.h>

Adafruit_BMP280 bmp; // I2C

byte buffer[12];
unsigned char tobytes[4];
float temp;
float pressure;
float altitude;

void setup() {
  Serial.begin(115200);//115200 is apprently the secret code to be able to write bt?
  
  if (!bmp.begin()) {  
    Serial.println(F("Could not find a valid BMP280 sensor, check wiring!"));
    while (1);
  }
}

void loop() {
  //if (Serial.available())  {
    //int piezoValue =0;
    //piezoValue = analogRead(piezoPin);

    temp = bmp.readTemperature();
    //Serial.print(F("T="));
    //Serial.print(temp);
    //Serial.println(" *C");

    pressure = bmp.readPressure();
    //Serial.print(F("P="));
    //Serial.print(pressure);
    //Serial.println(" Pa");

    altitude = bmp.readAltitude(1013.25);
    //Serial.print(F("A="));
    //Serial.print(); // this should be adjusted to your local forcase
    //Serial.println(" m");

    send(temp,pressure,altitude);
    
    //Serial.println();
    delay(1000);

    //Serial.read();
  //}
}

void send(float temp,float pressure,float altitude) {
  
  memcpy(tobytes,&temp,sizeof(float));
  buffer[0] = tobytes[0];
  buffer[1] = tobytes[1];
  buffer[2] = tobytes[2];
  buffer[3] = tobytes[3];

  memcpy(tobytes,&pressure,sizeof(float));
  buffer[4] = tobytes[0];
  buffer[5] = tobytes[1];
  buffer[6] = tobytes[2];
  buffer[7] = tobytes[3];

  memcpy(tobytes,&altitude,sizeof(float));
  buffer[8] = tobytes[0];
  buffer[9] = tobytes[1];
  buffer[10] = tobytes[2];
  buffer[11] = tobytes[3];

  Serial.write(buffer,12);
  Serial.flush();
}
