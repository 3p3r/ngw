#include "ngw.h"
#include "ngw.hpp"

struct _Player      final : public ngw::Player { };
struct _Discoverer  final : public ngw::Discoverer { };

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

NGWAPI(void) ngw_add_plugin_path(const char* path) {
    ngw::addPluginPath(path);
}

NGWAPI(void) ngw_add_binary_path(const char* path) {
    ngw::addBinaryPath(path);
}

NGWAPI(Player*) ngw_player_make(void) {
    return new Player();
}

NGWAPI(void) ngw_player_free(Player* player) {
    delete player;
}

NGWAPI(Discoverer*) ngw_discoverer_make(void) {
    return new Discoverer();
}

NGWAPI(void) ngw_discoverer_free(Discoverer* discoverer) {
    delete discoverer;
}

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
