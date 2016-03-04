#include "ngw.h"
#include "ngw.hpp"

#ifdef __APPLE__
#   include <OpenGL/gl.h>
#else
#   ifdef _WIN32
#       include <windows.h>
#   endif
#   include <GL/gl.h>
#endif

struct _Player final : public ngw::Player {
public:
    void        setUserData(gpointer data);
    gpointer    getUserData() const;
    void        setFrameBuffer(void* buffer, NgwBuffer type);
    void        setErrorCallback(NGW_ERROR_CALLBACK_TYPE cb);
    void        setStateCallback(NGW_STATE_CALLBACK_TYPE cb);
    void        setStreamEndCallback(NGW_STREAM_END_CALLBACK_TYPE cb);

protected:
    void        onFrame(guchar* buf, gsize size) const override;
    void        onError(const gchar* msg) const override;
    void        onState(GstState state) const override;
    void        onStreamEnd() const override;

private:
    gpointer    mBuffer     = nullptr;
    gpointer    mUserData   = nullptr;
    NgwBuffer   mBufferType = NGW_BUFFER_BYTE_POINTER;

    NGW_ERROR_CALLBACK_TYPE         mErrorCallback      = nullptr;
    NGW_STATE_CALLBACK_TYPE         mStateCallback      = nullptr;
    NGW_STREAM_END_CALLBACK_TYPE    mStreamEndCallback  = nullptr;
};

void _Player::setUserData(gpointer data)
{
    mUserData = data;
}

gpointer _Player::getUserData() const
{
    return mUserData;
}

void _Player::setFrameBuffer(void* buffer, NgwBuffer type)
{
    if (mBuffer == buffer && mBufferType == type) return;
    mBufferType = type;
    mBuffer = buffer;
}

void _Player::onFrame(guchar* buf, gsize size) const
{
    if (mBuffer == nullptr) return;
    if (mBufferType == NGW_BUFFER_BYTE_POINTER) {
        gst_buffer_extract(getBuffer(), 0, mBuffer, size);
    }
    else if (mBufferType == NGW_BUFFER_CALLBACK_FUNCTION) {
        (NGW_FRAME_CALLBACK_TYPE(mBuffer))(buf, static_cast<unsigned int>(size), this);
    }
    else if (mBufferType == NGW_BUFFER_OPENGL_TEXTURE) {
        ::glBindTexture(GL_TEXTURE_2D, (GLuint)(gsize)mBuffer);
        ::glTexSubImage2D(
            GL_TEXTURE_2D,
            0, 0, 0,
            getWidth(),
            getHeight(),
            0x80E1, // GL_BGRA
            GL_UNSIGNED_BYTE,
            buf);
        ::glBindTexture(GL_TEXTURE_2D, 0);
    }
}

void _Player::setErrorCallback(NGW_ERROR_CALLBACK_TYPE cb)
{
    mErrorCallback = cb;
}

void _Player::setStateCallback(NGW_STATE_CALLBACK_TYPE cb)
{
    mStateCallback = cb;
}

void _Player::setStreamEndCallback(NGW_STREAM_END_CALLBACK_TYPE cb)
{
    mStreamEndCallback = cb;
}

void _Player::onError(const gchar* msg) const
{
    if (mErrorCallback != nullptr)
        mErrorCallback(msg, this);
}

void _Player::onState(GstState state) const
{
    if (mStateCallback != nullptr)
        mStateCallback(int(state), this);
}

void _Player::onStreamEnd() const
{
    if (mStreamEndCallback != nullptr)
        mStreamEndCallback(this);
}

struct _Discoverer  final : public ngw::Discoverer { };

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

NGWAPI const char* ngw_get_version(void) {
    return ngw::getVersion();
}

NGWAPI void ngw_add_plugin_path(const char* path) {
    ngw::addPluginPath(path);
}

NGWAPI void ngw_add_binary_path(const char* path) {
    ngw::addBinaryPath(path);
}

NGWAPI Player* ngw_player_make(void) {
    return new Player();
}

NGWAPI void ngw_player_free(Player* player) {
    delete player;
}

NGWAPI Discoverer* ngw_discoverer_make(void) {
    return new Discoverer();
}

NGWAPI NgwBool ngw_discoverer_open(Discoverer* discoverer, const char* path) {
    return discoverer->open(path) ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI const char* ngw_discoverer_get_uri(Discoverer* discoverer) {
    return discoverer->getUri();
}

NGWAPI void ngw_discoverer_free(Discoverer* discoverer) {
    delete discoverer;
}

NGWAPI int ngw_discoverer_get_width(Discoverer* discoverer) {
    return discoverer->getWidth();
}

NGWAPI int ngw_discoverer_get_height(Discoverer* discoverer) {
    return discoverer->getHeight();
}

NGWAPI float ngw_discoverer_get_frame_rate(Discoverer* discoverer) {
    return discoverer->getFrameRate();
}

NGWAPI NgwBool ngw_discoverer_get_has_video(Discoverer* discoverer) {
    return discoverer->getHasVideo() ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI NgwBool ngw_discoverer_get_has_audio(Discoverer* discoverer) {
    return discoverer->getHasAudio() ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI NgwBool ngw_discoverer_get_seekable(Discoverer* discoverer) {
    return discoverer->getSeekable() ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI double ngw_discoverer_get_duration(Discoverer* discoverer) {
    return discoverer->getDuration();
}

NGWAPI unsigned ngw_discoverer_get_sample_rate(Discoverer* discoverer) {
    return discoverer->getSampleRate();
}

NGWAPI unsigned ngw_discoverer_get_bit_rate(Discoverer* discoverer) {
    return discoverer->getBitRate();
}

NGWAPI NgwBool ngw_player_open(Player* player, const char* path) {
    return player->open(path) ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI NgwBool ngw_player_open_format(Player* player, const char* path, const char* fmt) {
    return player->open(path, fmt);
}

NGWAPI NgwBool ngw_player_open_resize(Player* player, const char* path, int width, int height) {
    return player->open(path, width, height);
}

NGWAPI NgwBool ngw_player_open_resize_format(Player* player, const char* path, int width, int height, const char* fmt) {
    return player->open(path, width, height, fmt);
}

NGWAPI void ngw_player_close(Player* player) {
    player->close();
}

NGWAPI void ngw_player_set_state(Player* player, NgwState state) {
    player->setState(GstState(state));
}

NGWAPI NgwState ngw_player_get_state(Player* player) {
    return NgwState(player->getState());
}

NGWAPI void ngw_player_stop(Player* player) {
    player->stop();
}

NGWAPI void ngw_player_play(Player* player) {
    player->play();
}

NGWAPI void ngw_player_replay(Player* player) {
    player->replay();
}

NGWAPI void ngw_player_pause(Player* player) {
    player->pause();
}

NGWAPI void ngw_player_update(Player* player) {
    player->update();
}

NGWAPI double ngw_player_get_duration(Player* player) {
    return player->getDuration();
}

NGWAPI void ngw_player_set_time(Player* player, double time) {
    player->setTime(time);
}

NGWAPI double ngw_player_get_time(Player* player) {
    return player->getTime();
}

NGWAPI void ngw_player_set_volume(Player* player, double volume) {
    player->setVolume(volume);
}

NGWAPI double ngw_player_get_volume(Player* player) {
    return player->getVolume();
}

NGWAPI void ngw_player_set_mute(Player* player, NgwBool on) {
    player->setMute(on != NGW_BOOL_FALSE);
}

NGWAPI NgwBool ngw_player_get_mute(Player* player) {
    return player->getMute() ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI void ngw_player_set_loop(Player* player, NgwBool on) {
    player->setLoop(on != NGW_BOOL_FALSE);
}

NGWAPI NgwBool ngw_player_get_loop(Player* player) {
    return player->getLoop() ? NGW_BOOL_TRUE : NGW_BOOL_FALSE;
}

NGWAPI int ngw_player_get_width(Player* player) {
    return player->getWidth();
}

NGWAPI int ngw_player_get_height(Player* player) {
    return player->getHeight();
}

NGWAPI void ngw_player_set_rate(Player* player, double rate) {
    player->setRate(rate);
}

NGWAPI double ngw_player_get_rate(Player* player) {
    return player->getRate();
}

NGWAPI void ngw_player_set_user_data(Player* player, void *data) {
    player->setUserData(data);
}

NGWAPI void* ngw_player_get_user_data(Player* player) {
    return player->getUserData();
}

NGWAPI void ngw_player_set_frame_buffer(Player* player, void *buffer, NgwBuffer type) {
    player->setFrameBuffer(buffer, type);
}

NGWAPI void ngw_player_set_error_callback(Player* player, NGW_ERROR_CALLBACK_TYPE cb) {
    player->setErrorCallback(cb);
}

NGWAPI void ngw_player_set_state_callback(Player* player, NGW_STATE_CALLBACK_TYPE cb) {
    player->setStateCallback(cb);
}

NGWAPI void ngw_player_set_stream_end_callback(Player* player, NGW_STREAM_END_CALLBACK_TYPE cb) {
    player->setStreamEndCallback(cb);
}

#ifdef __cplusplus
} // extern "C"
#endif // __cplusplus
