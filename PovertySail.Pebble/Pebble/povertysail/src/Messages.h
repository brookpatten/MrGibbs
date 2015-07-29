#include <pebble.h>
  
enum AppMessageKey{
	COMMAND_KEY = 0,
  
  //button message
  BUTTON_KEY=1,
  
  //dash message
  LINE_KEY=1,
  MAP_KEY=2,
  
  //mark message
  MARK_KEY=1,
  BEARING_KEY=2,
	
  //data refresh message
  CAPTION1_KEY=0,
  VALUE1_KEY=1,
  CAPTION2_KEY=2,
  VALUE2_KEY=3,
  CAPTION3_KEY=4,
  VALUE3_KEY=5,
  MESSAGE_KEY=6
};

enum UICommand{
  Button=0,
  Dash=1,
  Course=2,
  Mark=3,
  NewRace=4,
  Calibrate=5,
  Restart=6,
  Reboot=7,
  Shutdown=8
};

enum Button{ 
  Up = 0, 
  Select = 1, 
  Down = 2 
};

void messages_init(void);
void messages_deinit(void);
void message_send_button(uint8_t button);
void message_send_dash(uint8_t line, uint8_t map);
void message_send_mark_bearing(uint8_t markindex, int32_t bearing);
void message_send_mark_location(uint8_t markindex);
void message_send_new_race();
void message_send_calibrate(void);
void message_send_restart(void);
void message_send_reboot(void);
void message_send_shutdown(void);
static void in_received_handler(DictionaryIterator *received, void *context);
static void in_dropped_handler(AppMessageResult reason, void *context);
static void out_failed_handler(DictionaryIterator *failed, AppMessageResult reason, void *context);