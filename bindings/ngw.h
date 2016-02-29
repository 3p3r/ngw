#pragma once

// Compiler specific shared library symbol visibility
#if defined(_WIN32) && defined(NGW_BUILD_DLL)
 /* We are building NGW as a DLL */
 #define NGWAPI __declspec(dllexport)
#elif defined(_WIN32)
 /* We are calling NGW as a DLL */
 #define NGWAPI __declspec(dllimport)
#elif defined(__GNUC__) && defined(NGW_BUILD_DLL)
 /* We are building NGW as a shared / dynamic library */
 #define NGWAPI __attribute__((visibility("default")))
#else
 /* We are building or calling NGW as a static library */
 #define NGWAPI
#endif

// For the sake of keeping header portable,
// following represent gboolean literals.
#define NGW_BOOL_FALSE 0
#define NGW_BOOL_TRUE  1

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

//! subclass of ngw::Player which is shared library specific
//! this subclass extends the API to expose virtual methods.
typedef struct      _Player     Player;
//! subclass of ngw::Discoverer to stay typed in C bindings.
typedef struct      _Discoverer Discoverer;
//! boolean type, identical to gboolean
typedef int         NgwBool;
//! State type, identical to GstState enum
typedef int         NgwState;
//! buffer types, specific to shared library target
typedef enum {
    NGW_BUFFER_BYTE_POINTER         = 0, //!< a typical unsigned char* pointer
    NGW_BUFFER_OPENGL_TEXTURE       = 1, //!< an OpenGL texture name
    NGW_BUFFER_CALLBACK_FUNCTION    = 2, //!< a C-style callback function
} NgwBuffer;

//! Frame virtual callback. Instance of the Player is passed in.
typedef void       (*NGW_FRAME_CALLBACK_TYPE)(unsigned char*, unsigned int, const Player*);
//! Error virtual callback. Instance of the Player is passed in.
typedef void       (*NGW_ERROR_CALLBACK_TYPE)(const char*, const Player*);
//! State virtual callback. Instance of the Player is passed in.
typedef void       (*NGW_STATE_CALLBACK_TYPE)(int, const Player*);
//! Stream End virtual callback. Instance of the Player is passed in.
typedef void       (*NGW_STREAM_END_CALLBACK_TYPE)(const Player*);

//! @cond NGW C api. For documentation please consult ngw.hpp
NGWAPI void        ngw_add_plugin_path(const char* path);
NGWAPI void        ngw_add_binary_path(const char* path);

NGWAPI Player*     ngw_player_make(void);
NGWAPI NgwBool     ngw_player_open(Player* player, const char* path);
NGWAPI NgwBool     ngw_player_open_format(Player* player, const char* path, const char* fmt);
NGWAPI NgwBool     ngw_player_open_resize(Player* player, const char* path, int width, int height);
NGWAPI NgwBool     ngw_player_open_resize_format(Player* player, const char* path, int width, int height, const char* fmt);
NGWAPI void        ngw_player_close(Player* player);
NGWAPI void        ngw_player_set_state(Player* player, NgwState state);
NGWAPI NgwState    ngw_player_get_state(Player* player);
NGWAPI void        ngw_player_stop(Player* player);
NGWAPI void        ngw_player_play(Player* player);
NGWAPI void        ngw_player_replay(Player* player);
NGWAPI void        ngw_player_pause(Player* player);
NGWAPI void        ngw_player_update(Player* player);
NGWAPI void        ngw_player_set_time(Player* player, double time);
NGWAPI double      ngw_player_get_time(Player* player);
NGWAPI void        ngw_player_set_volume(Player* player, double volume);
NGWAPI double      ngw_player_get_volume(Player* player);
NGWAPI void        ngw_player_set_mute(Player* player, NgwBool on);
NGWAPI NgwBool     ngw_player_get_mute(Player* player);
NGWAPI int         ngw_player_get_width(Player* player);
NGWAPI int         ngw_player_get_height(Player* player);
NGWAPI void        ngw_player_set_rate(Player* player, double rate);
NGWAPI double      ngw_player_get_rate(Player* player);
NGWAPI void        ngw_player_set_user_data(Player* player, void *data);
NGWAPI void*       ngw_player_get_user_data(Player* player);
NGWAPI void        ngw_player_set_sample_buffer(Player* player, void *buffer, NgwBuffer type);
NGWAPI void        ngw_player_set_error_callback(Player* player, NGW_ERROR_CALLBACK_TYPE cb);
NGWAPI void        ngw_player_set_state_callback(Player* player, NGW_STATE_CALLBACK_TYPE cb);
NGWAPI void        ngw_player_set_stream_end_callback(Player* player, NGW_STREAM_END_CALLBACK_TYPE cb);
NGWAPI void        ngw_player_free(Player* player);

NGWAPI Discoverer* ngw_discoverer_make(void);
NGWAPI const char* ngw_discoverer_get_path(Discoverer* discoverer);
NGWAPI int         ngw_discoverer_get_width(Discoverer* discoverer);
NGWAPI int         ngw_discoverer_get_height(Discoverer* discoverer);
NGWAPI float       ngw_discoverer_get_framerate(Discoverer* discoverer);
NGWAPI NgwBool     ngw_discoverer_get_has_video(Discoverer* discoverer);
NGWAPI NgwBool     ngw_discoverer_get_has_audio(Discoverer* discoverer);
NGWAPI NgwBool     ngw_discoverer_get_seekable(Discoverer* discoverer);
NGWAPI double      ngw_discoverer_get_duration(Discoverer* discoverer);
NGWAPI unsigned    ngw_discoverer_get_sample_rate(Discoverer* discoverer);
NGWAPI unsigned    ngw_discoverer_get_bit_rate(Discoverer* discoverer);
NGWAPI void        ngw_discoverer_free(Discoverer* discoverer);
//! @endcond

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
