#include <pebble.h>
#include "Compass.h"
#include "Messages.h"
#include "Dashboard.h"


static void compass_heading_handler(CompassHeadingData heading_data) {
  mark_bearing = heading_data.magnetic_heading;
  
  // rotate needle accordingly
  //gpath_rotate_to(compass_needle_north, heading_data.magnetic_heading);
  //gpath_rotate_to(compass_needle_south, heading_data.magnetic_heading);

  // display heading in degrees and radians
  static char s_heading_buf[64];
  snprintf(s_heading_buf, sizeof(s_heading_buf),
    " %ld°",
    360-TRIGANGLE_TO_DEG(heading_data.magnetic_heading));
  
  text_layer_set_text(compass_heading_layer, s_heading_buf);

  // Modify alert layout depending on calibration state
  GRect bounds = layer_get_frame(window_get_root_layer(compass_window)); 
  GRect alert_bounds; 
  if(heading_data.compass_status == CompassStatusDataInvalid) {
    // Tell user to move their arm
    alert_bounds = GRect(0, 0, bounds.size.w, bounds.size.h);
    text_layer_set_background_color(compass_text_layer_calib_state, GColorBlack);
    text_layer_set_text_color(compass_text_layer_calib_state, GColorWhite);
    text_layer_set_font(compass_text_layer_calib_state, fonts_get_system_font(FONT_KEY_GOTHIC_24_BOLD));
    text_layer_set_text_alignment(compass_text_layer_calib_state, GTextAlignmentCenter);
  } else {
    // Show status at the top
    alert_bounds = GRect(0, -3, bounds.size.w, bounds.size.h / 7);
    text_layer_set_background_color(compass_text_layer_calib_state, GColorClear);
    text_layer_set_text_color(compass_text_layer_calib_state, GColorBlack);
    text_layer_set_font(compass_text_layer_calib_state, fonts_get_system_font(FONT_KEY_GOTHIC_18));
    text_layer_set_text_alignment(compass_text_layer_calib_state, GTextAlignmentLeft);
  }
  layer_set_frame(text_layer_get_layer(compass_text_layer_calib_state), alert_bounds);

  // Display state of the compass
  static char s_valid_buf[64];
  switch (heading_data.compass_status) {
    case CompassStatusDataInvalid:
      snprintf(s_valid_buf, sizeof(s_valid_buf), "%s", "Compass is calibrating!\n\nMove your arm to aid calibration.");
      break;
    case CompassStatusCalibrating:
      snprintf(s_valid_buf, sizeof(s_valid_buf), "%s", "Fine tuning...");
      break;
    case CompassStatusCalibrated:
      snprintf(s_valid_buf, sizeof(s_valid_buf), "%s", "Calibrated");
      break;
  }
  text_layer_set_text(compass_text_layer_calib_state, s_valid_buf);

  // trigger layer for refresh
  layer_mark_dirty(compass_path_layer);
}

static void compass_path_layer_update_callback(Layer *path, GContext *ctx) {
#ifdef PBL_COLOR
  graphics_context_set_fill_color(ctx, GColorRed);
#endif
  //gpath_draw_filled(ctx, compass_needle_north);       
#ifndef PBL_COLOR
  graphics_context_set_fill_color(ctx, GColorBlack);
#endif  
  //gpath_draw_outline(ctx, compass_needle_south);                     

  // creating centerpoint                 
  //GRect bounds = layer_get_frame(path);          
  //GPoint path_center = GPoint(bounds.size.w / 2, bounds.size.h / 2);  
  //graphics_fill_circle(ctx, path_center, 3);       

  // then put a white circle on top               
  //graphics_context_set_fill_color(ctx, GColorWhite);
  //graphics_fill_circle(ctx, path_center, 2);                      
}

static void compass_select_click_handler(ClickRecognizerRef recognizer, void *context) {
  message_send_mark_bearing(mark_index,mark_bearing);
  return_to_dashboard(S_FALSE);
}

static void compass_up_click_handler(ClickRecognizerRef recognizer, void *context) {
  message_send_mark_bearing(mark_index,mark_bearing);
  return_to_dashboard(S_FALSE);
}

static void compass_down_click_handler(ClickRecognizerRef recognizer, void *context) {
  message_send_mark_bearing(mark_index,mark_bearing);
  return_to_dashboard(S_FALSE);
}

static void compass_click_config_provider(void *context) {
  window_single_click_subscribe(BUTTON_ID_SELECT, compass_select_click_handler);
  window_single_click_subscribe(BUTTON_ID_UP, compass_up_click_handler);
  window_single_click_subscribe(BUTTON_ID_DOWN, compass_down_click_handler);
}

static void compass_window_load(Window *window) {
  Layer *window_layer = window_get_root_layer(window);
  GRect bounds = layer_get_frame(window_layer);

  // Create the bitmap for the background and put it on the screen
  //compass_bitmap_layer = bitmap_layer_create(bounds);
  //compass_background_bitmap = gbitmap_create_with_resource(RESOURCE_ID_IMAGE_COMPASS);
  //bitmap_layer_set_bitmap(compass_bitmap_layer, compass_background_bitmap);
  
  // Make needle background 'transparent' with GCompOpAnd
  //bitmap_layer_set_compositing_mode(compass_bitmap_layer, GCompOpAnd);
  //layer_add_child(window_layer, bitmap_layer_get_layer(compass_bitmap_layer));

  // Create the layer in which we will draw the compass needles
  compass_path_layer = layer_create(bounds);
  
  //  Define the draw callback to use for this layer
  layer_set_update_proc(compass_path_layer, compass_path_layer_update_callback);
  layer_add_child(window_layer, compass_path_layer);

  // Initialize and define the two paths used to draw the needle to north and to south
  //compass_needle_north = gpath_create(&NEEDLE_NORTH_POINTS);
  //compass_needle_south = gpath_create(&NEEDLE_SOUTH_POINTS);

  // Move the needles to the center of the screen.
  //GPoint center = GPoint(bounds.size.w / 2, bounds.size.h / 2);
  //gpath_move_to(compass_needle_north, center);
  //gpath_move_to(compass_needle_south, center);

  // Place text layers onto screen: one for the heading and one for calibration status
  compass_heading_layer = text_layer_create(GRect(12, bounds.size.h * 3 / 4, bounds.size.w / 4, bounds.size.h / 5));
  text_layer_set_text(compass_heading_layer, "No Data");
  layer_add_child(window_layer, text_layer_get_layer(compass_heading_layer));

  compass_text_layer_calib_state = text_layer_create(GRect(0, 0, bounds.size.w, bounds.size.h / 7));
  text_layer_set_text_alignment(compass_text_layer_calib_state, GTextAlignmentLeft);
  text_layer_set_background_color(compass_text_layer_calib_state, GColorClear);

  layer_add_child(window_layer, text_layer_get_layer(compass_text_layer_calib_state));
}

static void compass_window_unload(Window *window) {
  compass_service_unsubscribe();
  text_layer_destroy(compass_heading_layer);
  text_layer_destroy(compass_text_layer_calib_state);
  //gpath_destroy(compass_needle_north);
  //gpath_destroy(compass_needle_south);
  layer_destroy(compass_path_layer);
  //gbitmap_destroy(compass_background_bitmap);
  //bitmap_layer_destroy(compass_bitmap_layer);
  window_destroy(compass_window);
}

void compass_init(uint8_t markindex) {
  mark_index=markindex;
  // initialize compass and set a filter to 2 degrees
  //compass_service_set_heading_filter(2 * (TRIG_MAX_ANGLE / 360));
  compass_service_set_heading_filter(0.5 * (TRIG_MAX_ANGLE / 360));
  //leave it at 1 degree
  compass_service_subscribe(&compass_heading_handler);

  compass_window = window_create();
#ifdef PBL_SDK_2
  window_set_fullscreen(compass_window, true);
#endif
  window_set_window_handlers(compass_window, (WindowHandlers) {
    .load = compass_window_load,
    .unload = compass_window_unload,
  });
  
  window_set_click_config_provider(compass_window, compass_click_config_provider);
  window_stack_push(compass_window, true);
}