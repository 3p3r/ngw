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
#define NGW_BOOL_FALSE 0
#define NGW_BOOL_TRUE  1

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

typedef struct      _Player     Player;
typedef struct      _Discoverer Discoverer;
typedef int         NgwBool;
typedef int         NgwState;
typedef enum {
    NGW_BUFFER_BYTE_POINTER         = 0,
    NGW_BUFFER_OPENGL_TEXTURE       = 1,
    NGW_BUFFER_CALLBACK_FUNCTION    = 2,
} NgwBuffer;

typedef void        (*NGW_FRAME_CALLBACK_TYPE)(unsigned char*, unsigned int, const Player*);
typedef void        (*NGW_ERROR_CALLBACK_TYPE)(const char*, const Player*);
typedef void        (*NGW_STATE_CALLBACK_TYPE)(int, const Player*);
typedef void        (*NGW_STREAM_END_CALLBACK_TYPE)(const Player*);

NGWAPI(void)        ngw_add_plugin_path(const char* path);
NGWAPI(void)        ngw_add_binary_path(const char* path);

NGWAPI(Player*)     ngw_player_make(void);
NGWAPI(NgwBool)     ngw_player_open(Player* player, const char* path);
NGWAPI(NgwBool)     ngw_player_open_format(Player* player, const char* path, const char* fmt);
NGWAPI(NgwBool)     ngw_player_open_resize(Player* player, const char* path, int width, int height);
NGWAPI(NgwBool)     ngw_player_open_resize_format(Player* player, const char* path, int width, int height, const char* fmt);
NGWAPI(void)        ngw_player_close(Player* player);
NGWAPI(void)        ngw_player_set_state(Player* player, NgwState state);
NGWAPI(NgwState)    ngw_player_get_state(Player* player);
NGWAPI(void)        ngw_player_stop(Player* player);
NGWAPI(void)        ngw_player_play(Player* player);
NGWAPI(void)        ngw_player_replay(Player* player);
NGWAPI(void)        ngw_player_pause(Player* player);
NGWAPI(void)        ngw_player_update(Player* player);
NGWAPI(void)        ngw_player_set_time(Player* player, double time);
NGWAPI(double)      ngw_player_get_time(Player* player);
NGWAPI(void)        ngw_player_set_volume(Player* player, double volume);
NGWAPI(double)      ngw_player_get_volume(Player* player);
NGWAPI(void)        ngw_player_set_mute(Player* player, NgwBool on);
NGWAPI(NgwBool)     ngw_player_get_mute(Player* player);
NGWAPI(int)         ngw_player_get_width(Player* player);
NGWAPI(int)         ngw_player_get_height(Player* player);
NGWAPI(void)        ngw_player_set_rate(Player* player, double rate);
NGWAPI(double)      ngw_player_get_rate(Player* player);
NGWAPI(void)        ngw_player_set_user_data(Player* player, void *data);
NGWAPI(void*)       ngw_player_get_user_data(Player* player);
NGWAPI(void)        ngw_player_set_sample_buffer(Player* player, void *buffer, NgwBuffer type);
NGWAPI(void)        ngw_player_set_error_callback(Player* player, NGW_ERROR_CALLBACK_TYPE cb);
NGWAPI(void)        ngw_player_set_state_callback(Player* player, NGW_STATE_CALLBACK_TYPE cb);
NGWAPI(void)        ngw_player_set_stream_end_callback(Player* player, NGW_STREAM_END_CALLBACK_TYPE cb);
NGWAPI(void)        ngw_player_free(Player* player);

NGWAPI(Discoverer*) ngw_discoverer_make(void);
NGWAPI(int)         ngw_discoverer_get_width(Discoverer* discoverer);
NGWAPI(int)         ngw_discoverer_get_height(Discoverer* discoverer);
NGWAPI(float)       ngw_discoverer_get_framerate(Discoverer* discoverer);
NGWAPI(NgwBool)     ngw_discoverer_get_has_video(Discoverer* discoverer);
NGWAPI(NgwBool)     ngw_discoverer_get_has_audio(Discoverer* discoverer);
NGWAPI(NgwBool)     ngw_discoverer_get_seekable(Discoverer* discoverer);
NGWAPI(double)      ngw_discoverer_get_duration(Discoverer* discoverer);
NGWAPI(void)        ngw_discoverer_free(Discoverer* discoverer);

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
