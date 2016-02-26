#pragma once

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

typedef struct _Player     Player;
typedef struct _Discoverer Discoverer;

void        ngw_add_plugin_path(const char* path);
void        ngw_add_binary_path(const char* path);
Player*     ngw_player_make(void);
void        ngw_player_free(Player* player);
Discoverer* ngw_discoverer_make(void);
void        ngw_discoverer_free(Discoverer* discoverer);

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
