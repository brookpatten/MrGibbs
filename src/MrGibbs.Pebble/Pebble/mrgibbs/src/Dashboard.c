#include <pebble.h>
#include "Dashboard.h"
#include "Messages.h"
#include "MainMenu.h"

static void dashboard_select_click_handler(ClickRecognizerRef recognizer, void *context) {
  main_menu_init();
}

static void dashboard_up_click_handler(ClickRecognizerRef recognizer, void *context) {
  message_send_button(Up);
}

static void dashboard_down_click_handler(ClickRecognizerRef recognizer, void *context) {
  message_send_button(Down);
}

static void dashboard_click_config_provider(void *context) {
  window_single_click_subscribe(BUTTON_ID_SELECT, dashboard_select_click_handler);
  window_single_click_subscribe(BUTTON_ID_UP, dashboard_up_click_handler);
  window_single_click_subscribe(BUTTON_ID_DOWN, dashboard_down_click_handler);
}

void dashboard_clear(void *context){
  text_layer_set_text(c1_text_layer,"");
  text_layer_set_text(v1_text_layer,"");
  text_layer_set_text(c2_text_layer,"");
  text_layer_set_text(v2_text_layer,"");
  text_layer_set_text(c3_text_layer,"");
  text_layer_set_text(v3_text_layer,"");
  text_layer_set_text(m_text_layer,"");
  text_layer_set_background_color(m_text_layer,GColorBlack);
}

void dashboard_update(DictionaryIterator *received){
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
  
  if(!app_timer_reschedule(clear_timer,5000)){
    clear_timer = app_timer_register(5000,dashboard_clear,dashboard_window);
  }
  
}

void dashboard_init(void) {
	dashboard_window = window_create();
  #ifdef PBL_SDK_2
  window_set_fullscreen(dashboard_window,true);
  #endif
  window_set_background_color(dashboard_window,GColorBlack);
  window_set_click_config_provider(dashboard_window, dashboard_click_config_provider);
	window_stack_push(dashboard_window, true);
  dashboard_window_layer = window_get_root_layer(dashboard_window);
  
  GRect c1_bounds = layer_get_bounds(dashboard_window_layer);
  c1_text_layer = text_layer_create(c1_bounds);
  text_layer_set_background_color(c1_text_layer,GColorBlack);
  text_layer_set_text_color(c1_text_layer,GColorWhite);
  text_layer_set_font(c1_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(c1_text_layer, "Starting up...");
  text_layer_set_text_alignment(c1_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(c1_text_layer));
  
  GSize v1_size = {.w=c1_bounds.size.w,.h=c1_bounds.size.h-42};
  GPoint v1_point = {.x=0,.y=8};
  GRect v1_bounds = {.size=v1_size,.origin=v1_point};
  v1_text_layer = text_layer_create(v1_bounds);
  text_layer_set_background_color(v1_text_layer,GColorClear);
  text_layer_set_text_color(v1_text_layer,GColorWhite);
  text_layer_set_font(v1_text_layer,fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));
  text_layer_set_text(v1_text_layer, "");
  text_layer_set_text_alignment(v1_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(v1_text_layer));
  
  GSize c2_size = {.w=v1_bounds.size.w,.h=v1_bounds.size.h-8};
  GPoint c2_point = {.x=0,.y=50};
  GRect c2_bounds = {.size=c2_size,.origin=c2_point};
  c2_text_layer = text_layer_create(c2_bounds);
  text_layer_set_background_color(c2_text_layer,GColorClear);
  text_layer_set_text_color(c2_text_layer,GColorWhite);
  text_layer_set_font(c2_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(c2_text_layer, "");
  text_layer_set_text_alignment(c2_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(c2_text_layer));
  
  GSize v2_size = {.w=c2_bounds.size.w,.h=c2_bounds.size.h-42};
  GPoint v2_point = {.x=0,.y=58};
  GRect v2_bounds = {.size=v2_size,.origin=v2_point};
  v2_text_layer = text_layer_create(v2_bounds);
  text_layer_set_background_color(v2_text_layer,GColorClear);
  text_layer_set_text_color(v2_text_layer,GColorWhite);
  text_layer_set_font(v2_text_layer,fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));
  text_layer_set_text(v2_text_layer, "");
  text_layer_set_text_alignment(v2_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(v2_text_layer));
  
  GSize c3_size = {.w=v2_bounds.size.w,.h=v2_bounds.size.h-8};
  GPoint c3_point = {.x=0,.y=100};
  GRect c3_bounds = {.size=c3_size,.origin=c3_point};
  c3_text_layer = text_layer_create(c3_bounds);
  text_layer_set_background_color(c3_text_layer,GColorClear);
  text_layer_set_text_color(c3_text_layer,GColorWhite);
  text_layer_set_font(c3_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(c3_text_layer, "");
  text_layer_set_text_alignment(c3_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(c3_text_layer));
  
  GSize v3_size = {.w=c3_bounds.size.w,.h=c3_bounds.size.h};
  GPoint v3_point = {.x=0,.y=108};
  GRect v3_bounds = {.size=v3_size,.origin=v3_point};
  v3_text_layer = text_layer_create(v3_bounds);
  text_layer_set_background_color(v3_text_layer,GColorClear);
  text_layer_set_text_color(v3_text_layer,GColorWhite);
  text_layer_set_font(v3_text_layer,fonts_get_system_font(FONT_KEY_BITHAM_42_BOLD));
  text_layer_set_text(v3_text_layer, "");
  text_layer_set_text_alignment(v3_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(v3_text_layer));
  
  GSize m_size = {.w=v3_bounds.size.w,.h=v3_bounds.size.h-8};
  GPoint m_point = {.x=0,.y=152};
  GRect m_bounds = {.size=m_size,.origin=m_point};
  m_text_layer = text_layer_create(m_bounds);
  text_layer_set_font(m_text_layer,fonts_get_system_font(FONT_KEY_GOTHIC_14_BOLD));
  text_layer_set_text(m_text_layer, "");
  text_layer_set_text_alignment(m_text_layer,GTextAlignmentCenter);
  layer_add_child(dashboard_window_layer, text_layer_get_layer(m_text_layer));
  
  clear_timer = app_timer_register(5000,dashboard_clear,dashboard_window);
}

void dashboard_deinit(void) {
  app_timer_cancel(clear_timer);
  window_destroy(dashboard_window);
}

void return_to_dashboard(bool animated){
  while(window_stack_get_top_window()!=dashboard_window){
    window_stack_pop(animated);
  }
}