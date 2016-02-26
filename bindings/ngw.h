#pragma once

#ifndef NGWCALLCONV
#    ifdef _WIN32
#        define NGWCALLCONV __stdcall
#    else // _WIN32
#        define NGWCALLCONV
#    endif // _WIN32
#endif // NGWCALLCONV

#ifndef NGWVISIBILITY
#    if defined(_WIN32)
#        define NGWVISIBILITY __declspec(dllexport)
#    elif defined(__GNUC__)
#        define NGWVISIBILITY __attribute__((visibility("default")))
#    else
#        define NGWVISIBILITY
#    endif
#endif // NGWVISIBILITY

#define NGWAPI(type) NGWVISIBILITY type NGWCALLCONV

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

typedef struct _Player     Player;
typedef struct _Discoverer Discoverer;

NGWAPI(void)        ngw_add_plugin_path(const char* path);
NGWAPI(void)        ngw_add_binary_path(const char* path);
NGWAPI(Player*)     ngw_player_make(void);
NGWAPI(void)        ngw_player_free(Player* player);
NGWAPI(Discoverer*) ngw_discoverer_make(void);
NGWAPI(void)        ngw_discoverer_free(Discoverer* discoverer);

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
