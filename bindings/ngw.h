#pragma once

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

typedef struct _Player     Player;
typedef struct _Discoverer Discoverer;

void ngw_add_plugin_path(const char* path);
void ngw_add_binary_path(const char* path);

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
