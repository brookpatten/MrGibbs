#include <pebble.h>
  
Window *dashboard_window;	
Layer *dashboard_window_layer;
static TextLayer *c1_text_layer;
static TextLayer *v1_text_layer;
static TextLayer *c2_text_layer;
static TextLayer *v2_text_layer;
static TextLayer *c3_text_layer;
static TextLayer *v3_text_layer;
static TextLayer *m_text_layer;

static AppTimer *clear_timer;

void dashboard_init(void);
void dashboard_deinit(void);
static void dashboard_select_click_handler(ClickRecognizerRef recognizer, void *context);
static void dashboard_up_click_handler(ClickRecognizerRef recognizer, void *context);
static void dashboard_down_click_handler(ClickRecognizerRef recognizer, void *context);
static void dashboard_click_config_provider(void *context);
void dashboard_update(DictionaryIterator *received);
void dashboard_clear(void *context);

void return_to_dashboard(bool animated);