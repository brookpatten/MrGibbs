#include <pebble.h>
#include "DashboardMapMenu.h"
#include "Messages.h"
#include "Dashboard.h"
  
static uint16_t dashboard_map_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return 13;
}

static void dashboard_map_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context) {
  
  switch(cell_index->row) {
    case 0:
      menu_cell_basic_draw(ctx, cell_layer, "Speed", NULL, NULL);
      break;
    case 1:
      menu_cell_basic_draw(ctx, cell_layer, "VMG", NULL, NULL);
      break;
    case 2:
      menu_cell_basic_draw(ctx, cell_layer, "VMC", NULL, NULL);
      break;
    case 3:
      menu_cell_basic_draw(ctx, cell_layer, "COG", NULL, NULL);
      break;
    case 4:
      menu_cell_basic_draw(ctx, cell_layer, "Heading", NULL, NULL);
      break;
    case 5:
      menu_cell_basic_draw(ctx, cell_layer, "Heel", NULL, NULL);
      break;
    case 6:
      menu_cell_basic_draw(ctx, cell_layer, "Wind Speed (Apparant)", NULL, NULL);
      break;
    case 7:
      menu_cell_basic_draw(ctx, cell_layer, "Wind Speed (True)", NULL, NULL);
      break;
    case 8:
      menu_cell_basic_draw(ctx, cell_layer, "Wind Direction (Apparant)", NULL, NULL);
      break;
    case 9:
      menu_cell_basic_draw(ctx, cell_layer, "Wind Direction (True)", NULL, NULL);
      break;
    case 10:
      menu_cell_basic_draw(ctx, cell_layer, "Nominal Speed", NULL, NULL);
      break;
    case 11:
      menu_cell_basic_draw(ctx, cell_layer, "% Nominal Speed", NULL, NULL);
      break;
    case 12:
      menu_cell_basic_draw(ctx, cell_layer, "Top Speed", NULL, NULL);
      break;
    default:
      break;
  }
}

//static int16_t main_menu_get_cell_height_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  //return MENU_CELL_BASIC_CELL_HEIGHT;
//}

static void dashboard_map_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) {
  uint8_t map = cell_index->row;
  message_send_dash(dashboardRow,map);
  
  //pop back to the dash
  return_to_dashboard(S_FALSE);
  //window_stack_pop(S_FALSE);
  //window_stack_pop(S_FALSE);
  //window_stack_pop(S_FALSE);
}

static void dashboard_map_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context) {
  menu_cell_basic_header_draw(ctx, cell_layer, "Dashboard Item");
}

static int16_t dashboard_map_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context) {
  return MENU_CELL_BASIC_HEADER_HEIGHT;
}

static uint16_t dashboard_map_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context) {
  return 1;
}

static void dashboard_map_window_load(Window *window) {
  Layer *window_layer = window_get_root_layer(window);
  GRect bounds = layer_get_bounds(window_layer);

  dashboard_map_menu_layer = menu_layer_create(bounds);
  menu_layer_set_click_config_onto_window(dashboard_map_menu_layer, window);
  menu_layer_set_callbacks(dashboard_map_menu_layer, NULL, (MenuLayerCallbacks) {
      .get_num_rows = (MenuLayerGetNumberOfRowsInSectionsCallback)dashboard_map_menu_get_num_rows_callback,
      .draw_row = (MenuLayerDrawRowCallback)dashboard_map_menu_draw_row_callback,
      //.get_cell_height = (MenuLayerGetCellHeightCallback)dashboard_map_menu_get_cell_height_callback,
      .select_click = (MenuLayerSelectCallback)dashboard_map_menu_select_callback,
      .draw_header = (MenuLayerDrawHeaderCallback)dashboard_map_menu_draw_header_callback,
      .get_header_height = (MenuLayerGetHeaderHeightCallback)dashboard_map_menu_get_header_height_callback,
      .get_num_sections = (MenuLayerGetNumberOfSectionsCallback)dashboard_map_menu_get_num_sections_callback,
  });
  layer_add_child(window_layer, menu_layer_get_layer(dashboard_map_menu_layer));
}

static void dashboard_map_window_unload(Window *window) {
  menu_layer_destroy(dashboard_map_menu_layer);
}

void dashboard_map_menu_init(uint8_t row) {
  dashboardRow = row;
  dashboard_map_menu_window = window_create();
  window_set_window_handlers(dashboard_map_menu_window, (WindowHandlers) {
      .load = dashboard_map_window_load,
      .unload = dashboard_map_window_unload,
  });
  window_stack_push(dashboard_map_menu_window, true);
}

void dashboard_map_menu_deinit(void) {
  window_destroy(dashboard_map_menu_window);
}