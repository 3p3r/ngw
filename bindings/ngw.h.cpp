#include "ngw.hpp"

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

void ngw_add_plugin_path(const char* path) {
    ngw::Player::addPluginPath(path);
}

void ngw_add_binary_path(const char* path) {
    ngw::Player::addBinaryPath(path);
}

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
