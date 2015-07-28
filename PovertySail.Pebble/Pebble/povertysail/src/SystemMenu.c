#include <pebble.h>
#include "SystemMenu.h"
#include "Messages.h"
#include "Dashboard.h"

static uint16_t system_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return 4;
}

static void system_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
      menu_cell_basic_draw(ctx, cell_layer, "Status", NULL, NULL);
      break;
    case 1:
      menu_cell_basic_draw(ctx, cell_layer, "Calibrate", NULL, NULL);
      break;
    case 2:
      menu_cell_basic_draw(ctx, cell_layer, "Restart", NULL, NULL);
      break;
    case 3:
      menu_cell_basic_draw(ctx, cell_layer, "Reboot", NULL, NULL);
      break;
    default:
      break;
  }
}

//static int16_t main_menu_get_cell_height_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  //return MENU_CELL_BASIC_CELL_HEIGHT;
//}

static void system_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
      break;
    case 1:
      message_send_calibrate();
      return_to_dashboard(S_FALSE);
      break;
    case 2:
      message_send_restart();
      return_to_dashboard(S_FALSE);
      break;
    case 3:
      message_send_reboot();
      return_to_dashboard(S_FALSE);
      break;
    default:
      break;
  }
}

static void system_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context) {
  menu_cell_basic_header_draw(ctx, cell_layer, "system Row");
}

static int16_t system_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return MENU_CELL_BASIC_HEADER_HEIGHT;
}

static uint16_t system_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context) {
  return 1;
}

static void system_window_load(Window *window) {
  Layer *window_layer = window_get_root_layer(window);
  GRect bounds = layer_get_bounds(window_layer);

  system_menu_layer = menu_layer_create(bounds);
  menu_layer_set_click_config_onto_window(system_menu_layer, window);
  menu_layer_set_callbacks(system_menu_layer, NULL, (MenuLayerCallbacks) {
      .get_num_rows = (MenuLayerGetNumberOfRowsInSectionsCallback)system_menu_get_num_rows_callback,
      .draw_row = (MenuLayerDrawRowCallback)system_menu_draw_row_callback,
      //.get_cell_height = (MenuLayerGetCellHeightCallback)main_menu_get_cell_height_callback,
      .select_click = (MenuLayerSelectCallback)system_menu_select_callback,
      .draw_header = (MenuLayerDrawHeaderCallback)system_menu_draw_header_callback,
      .get_header_height = (MenuLayerGetHeaderHeightCallback)system_menu_get_header_height_callback,
      .get_num_sections = (MenuLayerGetNumberOfSectionsCallback)system_menu_get_num_sections_callback,
  });
  layer_add_child(window_layer, menu_layer_get_layer(system_menu_layer));
}

static void system_window_unload(Window *window) {
  menu_layer_destroy(system_menu_layer);
}

void system_menu_init(void) {
  system_menu_window = window_create();
  window_set_window_handlers(system_menu_window, (WindowHandlers) {
      .load = system_window_load,
      .unload = system_window_unload,
  });
  window_stack_push(system_menu_window, true);
}

void system_menu_deinit(void) {
  window_destroy(system_menu_window);
}