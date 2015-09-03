#include <pebble.h>
#include "MainMenu.h"
#include "DashboardMenu.h"
#include "SystemMenu.h"
#include "RaceMenu.h"

static uint16_t main_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return 3;
}

static void main_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
      menu_cell_basic_draw(ctx, cell_layer, "Dashboard", NULL, NULL);
      break;
    case 1:
      menu_cell_basic_draw(ctx, cell_layer, "Race", NULL, NULL);
      break;
    case 2:
      menu_cell_basic_draw(ctx, cell_layer, "System", NULL, NULL);
      break;
    default:
      break;
  }
}

//static int16_t main_menu_get_cell_height_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  //return MENU_CELL_BASIC_CELL_HEIGHT;
//}

static void main_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
      dashboard_menu_init();
      break;
    case 1:
      race_menu_init();
      break;
    case 2:
      system_menu_init();
      break;
    default:
      break;
  }
}

static void main_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context) {
  menu_cell_basic_header_draw(ctx, cell_layer, "Mr. Gibbs");
}

static int16_t main_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return MENU_CELL_BASIC_HEADER_HEIGHT;
}

static uint16_t main_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context) {
  return 1;
}

static void main_window_load(Window *window) {
  Layer *window_layer = window_get_root_layer(window);
  GRect bounds = layer_get_bounds(window_layer);

  main_menu_layer = menu_layer_create(bounds);
  menu_layer_set_click_config_onto_window(main_menu_layer, window);
  menu_layer_set_callbacks(main_menu_layer, NULL, (MenuLayerCallbacks) {
      .get_num_rows = (MenuLayerGetNumberOfRowsInSectionsCallback)main_menu_get_num_rows_callback,
      .draw_row = (MenuLayerDrawRowCallback)main_menu_draw_row_callback,
      //.get_cell_height = (MenuLayerGetCellHeightCallback)main_menu_get_cell_height_callback,
      .select_click = (MenuLayerSelectCallback)main_menu_select_callback,
      .draw_header = (MenuLayerDrawHeaderCallback)main_menu_draw_header_callback,
      .get_header_height = (MenuLayerGetHeaderHeightCallback)main_menu_get_header_height_callback,
      .get_num_sections = (MenuLayerGetNumberOfSectionsCallback)main_menu_get_num_sections_callback,
  });
  layer_add_child(window_layer, menu_layer_get_layer(main_menu_layer));
}

static void main_window_unload(Window *window) {
  menu_layer_destroy(main_menu_layer);
  
  window_destroy(main_menu_window);
}

void main_menu_init(void) {
  main_menu_window = window_create();
  window_set_window_handlers(main_menu_window, (WindowHandlers) {
      .load = main_window_load,
      .unload = main_window_unload,
  });
  window_stack_push(main_menu_window, true);
}
