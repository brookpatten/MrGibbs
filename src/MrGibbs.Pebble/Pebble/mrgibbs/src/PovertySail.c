#include <pebble.h>
#include "Dashboard.h"
#include "Messages.h"


void init(void) {
	dashboard_init();
  messages_init();
}

void deinit(void) {
	messages_deinit();
  dashboard_deinit();
}

int main( void ) {
	init();
	app_event_loop();
	deinit();
}
