#include <pebble.h>

// Vector paths for the compass needles
static const GPathInfo NEEDLE_NORTH_POINTS = { 
  3,
  (GPoint[]) { { -8, 0 }, { 8, 0 }, { 0, -36 } }
};
static const GPathInfo NEEDLE_SOUTH_POINTS = { 
  3,
  (GPoint[]) { { 8, 0 }, { 0, 36 }, { -8, 0 } }
};

static Window *compass_window;
//static BitmapLayer *compass_bitmap_layer;
//static GBitmap *compass_background_bitmap;
static Layer *compass_path_layer;
static TextLayer *compass_heading_layer;
static TextLayer *compass_text_layer_calib_state;

//static GPath *compass_needle_north;
//static GPath *compass_needle_south;

static const uint8_t course_heading = UINT8_MAX;

static uint8_t mark_index;
static uint32_t mark_bearing;

static void compass_heading_handler(CompassHeadingData heading_data);
static void compass_path_layer_update_callback(Layer *path, GContext *ctx) ;
static void compass_window_load(Window *window);
static void compass_window_unload(Window *window);
void compass_init(uint8_t mark_index);

static void compass_select_click_handler(ClickRecognizerRef recognizer, void *context);
static void compass_up_click_handler(ClickRecognizerRef recognizer, void *context);
static void compass_down_click_handler(ClickRecognizerRef recognizer, void *context);
static void compass_click_config_provider(void *context);