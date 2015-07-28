#include <pebble.h>
#include "DashboardMenu.h"
#include "DashboardMapMenu.h"

static uint16_t dashboard_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return 3;
}

static void dashboard_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
      menu_cell_basic_draw(ctx, cell_layer, "Top", NULL, NULL);
      break;
    case 1:
      menu_cell_basic_draw(ctx, cell_layer, "Middle", NULL, NULL);
      break;
    case 2:
      menu_cell_basic_draw(ctx, cell_layer, "Bottom", NULL, NULL);
      break;
    default:
      break;
  }
}

//static int16_t main_menu_get_cell_height_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  //return MENU_CELL_BASIC_CELL_HEIGHT;
//}

static void dashboard_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  switch(cell_index->row) {
    case 0:
      dashboard_map_menu_init(cell_index->row);
      break;
    case 1:
      dashboard_map_menu_init(cell_index->row);
      break;
    case 2:
      dashboard_map_menu_init(cell_index->row);
      break;
    default:
      break;
  }
}

static void dashboard_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context) {
  menu_cell_basic_header_draw(ctx, cell_layer, "Dashboard Row");
}

static int16_t dashboard_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return MENU_CELL_BASIC_HEADER_HEIGHT;
}

static uint16_t dashboard_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context) {
  return 1;
}

static void dashboard_window_load(Window *window) {
  Layer *window_layer = window_get_root_layer(window);
  GRect bounds = layer_get_bounds(window_layer);

  dashboard_menu_layer = menu_layer_create(bounds);
  menu_layer_set_click_config_onto_window(dashboard_menu_layer, window);
  menu_layer_set_callbacks(dashboard_menu_layer, NULL, (MenuLayerCallbacks) {
      .get_num_rows = (MenuLayerGetNumberOfRowsInSectionsCallback)dashboard_menu_get_num_rows_callback,
      .draw_row = (MenuLayerDrawRowCallback)dashboard_menu_draw_row_callback,
      //.get_cell_height = (MenuLayerGetCellHeightCallback)main_menu_get_cell_height_callback,
      .select_click = (MenuLayerSelectCallback)dashboard_menu_select_callback,
      .draw_header = (MenuLayerDrawHeaderCallback)dashboard_menu_draw_header_callback,
      .get_header_height = (MenuLayerGetHeaderHeightCallback)dashboard_menu_get_header_height_callback,
      .get_num_sections = (MenuLayerGetNumberOfSectionsCallback)dashboard_menu_get_num_sections_callback,
  });
  layer_add_child(window_layer, menu_layer_get_layer(dashboard_menu_layer));
}

static void dashboard_window_unload(Window *window) {
  menu_layer_destroy(dashboard_menu_layer);
  
  window_destroy(dashboard_menu_window);
}

void dashboard_menu_init(void) {
  dashboard_menu_window = window_create();
  window_set_window_handlers(dashboard_menu_window, (WindowHandlers) {
      .load = dashboard_window_load,
      .unload = dashboard_window_unload,
  });
  window_stack_push(dashboard_menu_window, true);
}
