#include <pebble.h>

static Window *system_menu_window;
static MenuLayer *system_menu_layer;

static uint16_t system_menu_get_num_rows_callback(MenuLayer *menu_layer, uint16_t section_index, void *context);

static void system_menu_draw_row_callback(GContext *ctx, Layer *cell_layer, MenuIndex *cell_index, void *context);

static void system_menu_select_callback(struct MenuLayer *menu_layer, MenuIndex *cell_index, void *context);

static void system_menu_draw_header_callback(GContext *ctx, const Layer *cell_layer, uint16_t section_index, void *context);

static int16_t system_menu_get_header_height_callback(struct MenuLayer *menu_layer, uint16_t section_index, void *context);

static uint16_t system_menu_get_num_sections_callback(struct MenuLayer *menu_layer, void *context);

static void system_window_load(Window *window);

static void system_window_unload(Window *window);

void system_menu_init(void);