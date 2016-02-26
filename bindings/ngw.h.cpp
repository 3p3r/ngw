#include "ngw.h"
#include "ngw.hpp"

struct _Player      final : public ngw::Player { };
struct _Discoverer  final : public ngw::Discoverer { };

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

void ngw_add_plugin_path(const char* path) {
    ngw::addPluginPath(path);
}

void ngw_add_binary_path(const char* path) {
    ngw::addBinaryPath(path);
}

Player* ngw_player_make(void) {
    return new Player();
}

void ngw_player_free(Player* player) {
    delete player;
}

Discoverer* ngw_discoverer_make(void) {
    return new Discoverer();
}

void ngw_discoverer_free(Discoverer* discoverer) {
    delete discoverer;
}

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
