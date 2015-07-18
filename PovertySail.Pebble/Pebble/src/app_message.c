#include <pebble.h>

Window *window;	
static TextLayer *c1_text_layer;
static TextLayer *v1_text_layer;
static TextLayer *c2_text_layer;
static TextLayer *v2_text_layer;
static TextLayer *c3_text_layer;
static TextLayer *v3_text_layer;
static TextLayer *m_text_layer;

Layer *window_layer;


// Key values for AppMessage Dictionary
enum {
	COMMAND_KEY = 0,	
	CAPTION1_KEY=0,
  VALUE1_KEY=1,
  CAPTION2_KEY=2,
  VALUE2_KEY=3,
  CAPTION3_KEY=4,
  VALUE3_KEY=5,
  MESSAGE_KEY=6
};

// Write message to buffer & send
void send_message(char* message){
	DictionaryIterator *iter;
	
	app_message_outbox_begin(&iter);
	dict_write_cstring(iter,COMMAND_KEY,message);
	
	dict_write_end(iter);
  	app_message_outbox_send();
}

// Called when a message is received from PebbleKitJS
static void in_received_handler(DictionaryIterator *received, void *context) {
	Tuple *tuple;
	
	tuple = dict_find(received, CAPTION1_KEY);
	if(tuple) {
    text_layer_set_text(c1_text_layer, tuple->value->cstring);
  }
  else{
    text_layer_set_text(c1_text_layer,"");
  }
  tuple = dict_find(received, VALUE1_KEY);
	if(tuple) {
    text_layer_set_text(v1_text_layer, tuple->value->cstring);
  }
  else{
    text_layer_set_text(v1_text_layer,"");
  }
	tuple = dict_find(received, CAPTION2_KEY);
	if(tuple) {
    text_layer_set_text(c2_text_layer, tuple->value->cstring);
  }
  else{
    text_layer_set_text(c2_text_layer,"");
  }
  tuple = dict_find(received, VALUE2_KEY);
	if(tuple) {
    text_layer_set_text(v2_text_layer, tuple->value->cstring);
  }
  else{
    text_layer_set_text(v2_text_layer,"");
  }
  tuple = dict_find(received, CAPTION3_KEY);
	if(tuple) {
    text_layer_set_text(c3_text_layer, tuple->value->cstring);
  }
  else{
    text_layer_set_text(c3_text_layer,"");
  }
  tuple = dict_find(received, VALUE3_KEY);
	if(tuple) {
    text_layer_set_text(v3_text_layer, tuple->value->cstring);
  }
  else{
    text_layer_set_text(v3_text_layer,"");
  }
  tuple = dict_find(received, MESSAGE_KEY);
	if(tuple && strlen(tuple->value->cstring)>0) {
    text_layer_set_text(m_text_layer, tuple->value->cstring);
    text_layer_set_background_color(m_text_layer,GColorWhite);
    text_layer_set_text_color(m_text_layer,GColorBlack);
  }
  else{
    text_layer_set_text(m_text_layer,"");
    text_layer_set_background_color(m_text_layer,GColorBlack);
  }
}


// Called when an incoming message from PebbleKitJS is dropped
static void in_dropped_handler(AppMessageResult reason, void *context) {	
}

// Called when PebbleKitJS does not acknowledge receipt of a message
static void out_failed_handler(DictionaryIterator *failed, AppMessageResult reason, void *context) {
}


static void select_click_handler(ClickRecognizerRef recognizer, void *context) {
  send_message("select");
}

static void up_click_handler(ClickRecognizerRef recognizer, void *context) {
  send_message("up");
}

static void down_click_handler(ClickRecognizerRef recognizer, void *context) {
  send_message("down");
}

static void click_config_provider(void *context) {
  window_single_click_subscribe(BUTTON_ID_SELECT, select_click_handler);
  window_single_click_subscribe(BUTTON_ID_UP, up_click_handler);
  window_single_click_subscribe(BUTTON_ID_DOWN, down_click_handler);
}

void init(void) {
	window = window_create();
  window_set_fullscreen(window,true);
  window_set_background_color(window,GColorBlack);
  window_set_click_config_provider(window, click_config_provider);
	window_stack_push(window, true);
  window_layer = window_get_root_layer(window);
  
  GRect c1_bounds = layer_get_bounds(window_layer);
  c1_text_layer = text_layer_create(c1_bounds);
  text_layer_set_background_color(c1_text_layer,GColorBlack);
  text_layer_set_text_color(c1_text_layer,GColorWhite);
  text_layer_set_font(c1_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(c1_text_layer, "Starting up...");
  text_layer_set_text_alignment(c1_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(c1_text_layer));
  
  GSize v1_size = {.w=c1_bounds.size.w,.h=c1_bounds.size.h-42};
  GPoint v1_point = {.x=0,.y=8};
  GRect v1_bounds = {.size=v1_size,.origin=v1_point};
  v1_text_layer = text_layer_create(v1_bounds);
  text_layer_set_background_color(v1_text_layer,GColorClear);
  text_layer_set_text_color(v1_text_layer,GColorWhite);
  text_layer_set_font(v1_text_layer,fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));
  text_layer_set_text(v1_text_layer, "");
  text_layer_set_text_alignment(v1_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(v1_text_layer));
  
  GSize c2_size = {.w=v1_bounds.size.w,.h=v1_bounds.size.h-8};
  GPoint c2_point = {.x=0,.y=50};
  GRect c2_bounds = {.size=c2_size,.origin=c2_point};
  c2_text_layer = text_layer_create(c2_bounds);
  text_layer_set_background_color(c2_text_layer,GColorClear);
  text_layer_set_text_color(c2_text_layer,GColorWhite);
  text_layer_set_font(c2_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(c2_text_layer, "");
  text_layer_set_text_alignment(c2_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(c2_text_layer));
  
  GSize v2_size = {.w=c2_bounds.size.w,.h=c2_bounds.size.h-42};
  GPoint v2_point = {.x=0,.y=58};
  GRect v2_bounds = {.size=v2_size,.origin=v2_point};
  v2_text_layer = text_layer_create(v2_bounds);
  text_layer_set_background_color(v2_text_layer,GColorClear);
  text_layer_set_text_color(v2_text_layer,GColorWhite);
  text_layer_set_font(v2_text_layer,fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));
  text_layer_set_text(v2_text_layer, "");
  text_layer_set_text_alignment(v2_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(v2_text_layer));
  
  GSize c3_size = {.w=v2_bounds.size.w,.h=v2_bounds.size.h-8};
  GPoint c3_point = {.x=0,.y=100};
  GRect c3_bounds = {.size=c3_size,.origin=c3_point};
  c3_text_layer = text_layer_create(c3_bounds);
  text_layer_set_background_color(c3_text_layer,GColorClear);
  text_layer_set_text_color(c3_text_layer,GColorWhite);
  text_layer_set_font(c3_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(c3_text_layer, "");
  text_layer_set_text_alignment(c3_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(c3_text_layer));
  
  GSize v3_size = {.w=c3_bounds.size.w,.h=c3_bounds.size.h};
  GPoint v3_point = {.x=0,.y=108};
  GRect v3_bounds = {.size=v3_size,.origin=v3_point};
  v3_text_layer = text_layer_create(v3_bounds);
  text_layer_set_background_color(v3_text_layer,GColorClear);
  text_layer_set_text_color(v3_text_layer,GColorWhite);
  text_layer_set_font(v3_text_layer,fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));
  text_layer_set_text(v3_text_layer, "");
  text_layer_set_text_alignment(v3_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(v3_text_layer));
  
  GSize m_size = {.w=v3_bounds.size.w,.h=v3_bounds.size.h-8};
  GPoint m_point = {.x=0,.y=152};
  GRect m_bounds = {.size=m_size,.origin=m_point};
  m_text_layer = text_layer_create(m_bounds);
  text_layer_set_font(m_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(m_text_layer, "");
  text_layer_set_text_alignment(m_text_layer,GTextAlignmentCenter);
  layer_add_child(window_layer, text_layer_get_layer(m_text_layer));
	
	// Register AppMessage handlers
	app_message_register_inbox_received(in_received_handler); 
	app_message_register_inbox_dropped(in_dropped_handler); 
	app_message_register_outbox_failed(out_failed_handler);
		
	app_message_open(app_message_inbox_size_maximum(), app_message_outbox_size_maximum());
	
	send_message("init");
}

void deinit(void) {
	app_message_deregister_callbacks();
	window_destroy(window);
}

int main( void ) {
	init();
	app_event_loop();
	deinit();
}
