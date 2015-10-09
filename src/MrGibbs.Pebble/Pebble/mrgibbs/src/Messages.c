#include <pebble.h>
#include "Dashboard.h"
#include "Messages.h"

void messages_init(void){
  app_message_register_inbox_received(in_received_handler); 
	app_message_register_inbox_dropped(in_dropped_handler); 
	app_message_register_outbox_failed(out_failed_handler);
		
	app_message_open(app_message_inbox_size_maximum(), app_message_outbox_size_maximum());
}

void messages_deinit(void) {
	app_message_deregister_callbacks();
}

void message_send_button(uint8_t button){
	DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Button);
  dict_write_uint8(iter,BUTTON_KEY,button);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_dash(uint8_t line, uint8_t map){
	DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Dash);
  dict_write_uint8(iter,LINE_KEY,line);
  dict_write_uint8(iter,MAP_KEY,map);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_mark_bearing(uint8_t markindex, int32_t bearing){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Mark);
  dict_write_uint8(iter,MARK_KEY,markindex);
  dict_write_int32(iter,BEARING_KEY,bearing);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_mark_location(uint8_t markindex){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Mark);
  dict_write_uint8(iter,MARK_KEY,markindex);
  
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_new_race(){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,NewRace);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_calibrate(void){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Calibrate);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_restart(void){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Restart);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_reboot(void){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Reboot);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

void message_send_shutdown(void){
  DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_uint8(iter,COMMAND_KEY,Shutdown);
	
	dict_write_end(iter);
  app_message_outbox_send();
}

// Called when a message is received from PebbleKitJS
void in_received_handler(DictionaryIterator *received, void *context) {
  //TODO: check for UI command that indicates a dashboard update and not something else
  dashboard_update(received);
}

// Called when an incoming message from PebbleKitJS is dropped
static void in_dropped_handler(AppMessageResult reason, void *context) {	
}

// Called when PebbleKitJS does not acknowledge receipt of a message
static void out_failed_handler(DictionaryIterator *failed, AppMessageResult reason, void *context) {
}