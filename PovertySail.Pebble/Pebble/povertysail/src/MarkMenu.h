#include <pebble.h>

static Window *mark_menu_window;
static MenuLayer *mark_menu_layer;

static uint8_t mark_index;

static uint16_t mark_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context);

static void mark_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context) ;

static void mark_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context) ;

static void mark_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context) ;

static int16_t mark_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context) ;

static uint16_t mark_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context) ;

static void mark_window_load(Window *window) ;

static void mark_window_unload(Window *window);

void mark_menu_init(uint8_t markindex);