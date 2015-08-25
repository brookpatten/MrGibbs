#include <pebble.h>
#include "Compass.h"
#include "Messages.h"
#include "MarkMenu.h"
#include "Dashboard.h"
  
static uint16_t mark_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return 2;
}

static void mark_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context) {
  
  switch(cell_index->row) {
    case 0:
      menu_cell_basic_draw(ctx, cell_layer, "Location", NULL, NULL);
      break;
    case 1:
      menu_cell_basic_draw(ctx, cell_layer, "Bearing", NULL, NULL);
      break;
    default:
      break;
  }
}

static void mark_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
        message_send_mark_location(mark_index);
        return_to_dashboard(S_FALSE);
      break;
    case 1:
        compass_init(mark_index);
      break;
    default:
      break;
  }
}

static void mark_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context) {
  menu_cell_basic_header_draw(ctx, cell_layer, "Acquisition Method");
}

static int16_t mark_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return MENU_CELL_BASIC_HEADER_HEIGHT;
}

static uint16_t mark_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context) {
  return 1;
}

static void mark_window_load(Window *window) {
  Layer *window_layer = window_get_root_layer(window);
  GRect bounds = layer_get_bounds(window_layer);

  mark_menu_layer = menu_layer_create(bounds);
  menu_layer_set_click_config_onto_window(mark_menu_layer, window);
  menu_layer_set_callbacks(mark_menu_layer, NULL, (MenuLayerCallbacks) {
      .get_num_rows = (MenuLayerGetNumberOfRowsInSectionsCallback)mark_menu_get_num_rows_callback,
      .draw_row = (MenuLayerDrawRowCallback)mark_menu_draw_row_callback,
      //.get_cell_height = (MenuLayerGetCellHeightCallback)mark_menu_get_cell_height_callback,
      .select_click = (MenuLayerSelectCallback)mark_menu_select_callback,
      .draw_header = (MenuLayerDrawHeaderCallback)mark_menu_draw_header_callback,
      .get_header_height = (MenuLayerGetHeaderHeightCallback)mark_menu_get_header_height_callback,
      .get_num_sections = (MenuLayerGetNumberOfSectionsCallback)mark_menu_get_num_sections_callback,
  });
  layer_add_child(window_layer, menu_layer_get_layer(mark_menu_layer));
}

static void mark_window_unload(Window *window) {
  menu_layer_destroy(mark_menu_layer);
  
  window_destroy(mark_menu_window);
}

void mark_menu_init(uint8_t markindex) {
  mark_index=markindex;
  mark_menu_window = window_create();
  window_set_window_handlers(mark_menu_window, (WindowHandlers) {
      .load = mark_window_load,
      .unload = mark_window_unload,
  });
  window_stack_push(mark_menu_window, true);
}