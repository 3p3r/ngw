#include "ngw.hpp"

#include <gst/gstregistry.h>
#include <gst/app/gstappsink.h>
#include <gst/pbutils/gstdiscoverer.h>

namespace ngw
{

template<typename T>
struct BindToScope
{
    BindToScope(T*& ptr) : pointer(ptr) {}
    ~BindToScope();
    T*& pointer;
};

template< class T > struct no_ptr        { typedef T type; };
template< class T > struct no_ptr<T*>    { typedef T type; };
#define BIND_TO_SCOPE(var) BindToScope<\
    no_ptr<decltype(var)>::type> scoped_##var(var);

class Internal
{
public:
    static bool            isNullOrEmpty(const gchar* const str);
    static gchar*          processPath(const gchar* path);
    static void            reset(ngw::Player& player);
    static void            reset(Discoverer& discoverer);
    static bool            gstreamerInitialized();
    static GstFlowReturn   onPreroll(GstElement* appsink, ngw::Player* player);
    static GstFlowReturn   onSampled(GstElement* appsink, ngw::Player* player);
    static void            processSample(ngw::Player *const player, GstSample* const sample);
    static void            processDuration(Player& player);
};

Player::Player()
    : mLoop(false)
    , mMute(false)
{
    Internal::reset(*this);
    if (!Internal::gstreamerInitialized())
    {
        onError("GStreamer could not be initialized.");
    }
}

Player::~Player()
{
    close();
}

void addPluginPath(const gchar* path)
{
    if (Internal::isNullOrEmpty(path))
    {
        g_debug("Plug-in path supplied is empty.");
        return;
    }

    if (!Internal::gstreamerInitialized())
    {
        g_debug("You are not able to add plug-in path. %s",
                "GStreamer could not be initialized.");
    }

    if (GstRegistry *registry = gst_registry_get())
    {
        gst_registry_scan_path(registry, path);
    }
}

void addBinaryPath(const gchar* path)
{
    if (Internal::isNullOrEmpty(path))
    {
        g_debug("Binary path supplied is empty.");
        return;
    }

    gchar* path_var = nullptr;
    BIND_TO_SCOPE(path_var);

    if (Internal::isNullOrEmpty(g_getenv("PATH")))
    {
        path_var = g_strdup(path);
    }
    else
    {
        path_var = g_strdup_printf("%s;%s", g_getenv("PATH"), path);
    }

    if (g_setenv("PATH", scoped_path_var.pointer, TRUE) == FALSE)
    {
        g_debug("Unable to append %s to PATH.", path);
    }
}

bool Player::open(const gchar *path, gint width, gint height, const gchar* fmt)
{
    bool success = false;
    if (!Internal::gstreamerInitialized())
    {
        onError("You cannot open a media with ngw.");
        return success;
    }

    // First close any current streams.
    close();

    if (Internal::isNullOrEmpty(path))
    {
        onError("Supplied media path is empty.");
        return success;
    }

    if (Internal::isNullOrEmpty(fmt))
    {
        onError("Supplied media format is empty.");
        return success;
    }

    // Acquire the new path
    gchar *uri = Internal::processPath(path);
    BIND_TO_SCOPE(uri);

    // Check if passed format is null, if yes supply a default one.
    const gchar* format = Internal::isNullOrEmpty(fmt) ? "BGRA" : fmt;

    // Create the pipeline expression
    gchar* pipeline_cmd = g_strdup_printf(
        "playbin uri=\"%s\" video-sink=\""
        "appsink drop=yes async=no qos=yes sync=yes max-lateness=%lld "
        "caps=video/x-raw,width=%d,height=%d,format=%s\"",
        scoped_uri.pointer,
        GST_SECOND,
        width,
        height,
        format);

    if (!Internal::isNullOrEmpty(pipeline_cmd))
    {
        BIND_TO_SCOPE(pipeline_cmd);

        mPipeline = gst_parse_launch(scoped_pipeline_cmd.pointer, nullptr);
        if (mPipeline == nullptr)
        {
            close();
            onError("Unable to launch the pipeline.");
            return success;
        }

        mGstBus = gst_pipeline_get_bus(GST_PIPELINE(mPipeline));
        if (mGstBus == nullptr)
        {
            close();
            onError("Unable to obtain pipeline's bus.");
            return success;
        }

        GstAppSink *app_sink = nullptr;
        BIND_TO_SCOPE(app_sink);

        g_object_get(mPipeline, "video-sink", &app_sink, nullptr);
        if (app_sink == nullptr)
        {
            close();
            onError("Unable to obtain pipeline's video sink.");
            return success;
        }

        // Configure VideoSink's appsink:
        typedef GstFlowReturn(*APP_SINK_CB) (GstAppSink*, gpointer);
        GstAppSinkCallbacks callbacks;

        callbacks.eos = nullptr;
        callbacks.new_preroll = APP_SINK_CB(&Internal::onPreroll);
        callbacks.new_sample  = APP_SINK_CB(&Internal::onSampled);

        gst_app_sink_set_callbacks(scoped_app_sink.pointer, &callbacks, this, nullptr);

        // Going from NULL => READY => PAUSE forces the
        // pipeline to pre-roll so we can get video dim

        GstState state;

        gst_element_set_state(mPipeline, GST_STATE_READY);
        if (gst_element_get_state(mPipeline, &state, nullptr, GST_SECOND) == GST_STATE_CHANGE_FAILURE ||
            state != GST_STATE_READY)
        {
            onError("Failed to put pipeline in READY state.");
            return success;
        }

        gst_element_set_state(mPipeline, GST_STATE_PAUSED);
        if (gst_element_get_state(mPipeline, &state, nullptr, GST_SECOND) == GST_STATE_CHANGE_FAILURE ||
            state != GST_STATE_PAUSED)
        {
            onError("Failed to put pipeline in PAUSE state.");
            return success;
        }

        success = true;
    }
    else
    {
        onError("Pipeline string is empty.");
    }

    return success;
}

bool Player::open(const gchar *path, gint width, gint height)
{
    return open(path, width, height, "BGRA");
}

bool Player::open(const gchar *path, const gchar* fmt)
{
    Discoverer discoverer;
    return discoverer.open(path) && open(path, discoverer.getWidth(), discoverer.getHeight(), fmt);

}

bool Player::open(const gchar *path)
{
    Discoverer discoverer;
    return discoverer.open(path) && open(path, discoverer.getWidth(), discoverer.getHeight());
}

void Player::close()
{
    stop();

    if (mPipeline != nullptr)      gst_object_unref(mPipeline);
    if (mGstBus != nullptr)        gst_object_unref(mGstBus);
    if (mCurrentBuffer != nullptr) gst_buffer_unmap(mCurrentBuffer, &mCurrentMapInfo);
    if (mCurrentSample != nullptr) gst_sample_unref(mCurrentSample);

    Internal::reset(*this);
}

void Player::setState(GstState state)
{
    g_return_if_fail(mPipeline != nullptr);
    gst_element_set_state(mPipeline, state);
}

GstState Player::getState() const
{
    return mState;
}

void Player::stop()
{
    setState(GST_STATE_NULL);
}

void Player::play()
{
    setState(GST_STATE_PLAYING);
    // This can happen if current instance is used to open a second URI
    if (getMute() && getVolume() != 0.) setMute(true);
}

void Player::replay()
{
    stop();
    play();
}

void Player::pause()
{
    setState(GST_STATE_PAUSED);
}

void Player::update()
{
    if (mGstBus != nullptr)
    {
        while (gst_bus_have_pending(mGstBus) != FALSE)
        {
            if (GstMessage* msg = gst_bus_pop(mGstBus))
            {
				BIND_TO_SCOPE(msg);

                switch (GST_MESSAGE_TYPE(scoped_msg.pointer))
                {
                case GST_MESSAGE_ERROR:
                {
                    GError *err = nullptr;
                    BIND_TO_SCOPE(err);
                    gst_message_parse_error(msg, &err, nullptr);
                    onError(scoped_err.pointer->message);
                    close();
                }
                break;

                case GST_MESSAGE_STATE_CHANGED:
                {
					if (GST_MESSAGE_SRC(msg) != GST_OBJECT(mPipeline))
						break;

                    GstState old_state = GST_STATE_NULL;
                    gst_message_parse_state_changed(msg, &old_state, &mState, nullptr);

                    if (old_state != mState)
                    {
                        onState(old_state);
                    }
                }
                break;

                case GST_MESSAGE_ASYNC_DONE:
                {
                    Internal::processDuration(*this);

                    if (mSeekingLock)
                    {
                        mSeekingLock = false;
                    }

                    if (mPendingSeek >= 0.)
                    {
                        setTime(mPendingSeek);
                    }
                }
                break;

                case GST_MESSAGE_DURATION_CHANGED:
                {
                    Internal::processDuration(*this);
                }
                break;

                case GST_MESSAGE_EOS:
                {
                    onStreamEnd();

                    if (getLoop())
                    {
                        replay();
                    }
                    else
                    {
                        pause();
                    }
                }
                break;

                default:
                    break;
                }
            }
        }
    }

    if (g_atomic_int_get(&mBufferDirty) != FALSE)
    {
        onFrame(
            mCurrentMapInfo.data,
            mCurrentMapInfo.size);

        // free current resources on previous frame
        if (mCurrentBuffer) gst_buffer_unmap(mCurrentBuffer, &mCurrentMapInfo);
        if (mCurrentSample) gst_sample_unref(mCurrentSample);

        mCurrentBuffer = nullptr;
        mCurrentSample = nullptr;

        // Signal Streaming thread it can produce
        g_atomic_int_set(&mBufferDirty, FALSE);
    }
}

gdouble Player::getDuration() const
{
    return mDuration;
}

void Player::setLoop(bool on)
{
    g_return_if_fail(mLoop != on);
    mLoop = on;
}

bool Player::getLoop() const
{
    return mLoop;
}

void Player::setTime(gdouble time)
{
    g_return_if_fail(mPipeline != nullptr);

    if (mSeekingLock)
    {
        mPendingSeek = time;
        return;
    }
    else if (gst_element_seek_simple(
        mPipeline,
        GST_FORMAT_TIME,
        GstSeekFlags(
            GST_SEEK_FLAG_FLUSH |
            GST_SEEK_FLAG_ACCURATE),
        gint64(CLAMP(time, 0, mDuration) * GST_SECOND)))
    {
        mSeekingLock = true;
        mPendingSeek = -1.;
    }
}

gdouble Player::getTime() const
{
    g_return_val_if_fail(mPipeline != nullptr, 0.);

    gint64 time_ns;
    if (gst_element_query_position(mPipeline, GST_FORMAT_TIME, &time_ns) != FALSE)
    {
        mTime = time_ns / gdouble(GST_SECOND);
    }

    return mTime;
}

void Player::setVolume(gdouble vol)
{
    g_return_if_fail(mPipeline != nullptr || mVolume != vol || !getMute());

    mVolume = CLAMP(vol, 0., 1.);
    mMute = false;

    if (mPipeline)
    {
        g_object_set(mPipeline, "volume", mVolume, nullptr);
    }
}

gdouble Player::getVolume() const
{
    g_return_val_if_fail(mPipeline != nullptr, 0.);

    if (mPipeline)
    {
        g_object_get(mPipeline, "volume", &mVolume, nullptr);
    }

    return mVolume;
}

void Player::setMute(bool on)
{
    static gdouble saved_volume = 1.;
    g_return_if_fail(mPipeline != nullptr || (on && saved_volume == 0.));

    if (on)
    {
        saved_volume = getVolume();
        setVolume(0.);
        mMute = true;
    }
    else
    {
        mMute = false;
        setVolume(saved_volume);
        saved_volume = 1.;
    }
}

bool Player::getMute() const
{
    return mMute;
}

gint Player::getWidth() const
{
    return mWidth;
}

gint Player::getHeight() const
{
    return mHeight;
}

GstMapInfo Player::getMapInfo() const
{
    return mCurrentMapInfo;
}

GstSample* Player::getSample() const
{
    return mCurrentSample;
}

GstBuffer* Player::getBuffer() const
{
    return mCurrentBuffer;
}

bool Discoverer::open(const gchar* path)
{
    bool success = false;

    if (!Internal::gstreamerInitialized())
    {
        g_debug("You cannot open a media with ngw. %s",
                "GStreamer could not be initialized.");
        return success;
    }

    try
    {
        Internal::reset(*this);
        if (Internal::isNullOrEmpty(path)) return success;

        gchar* uri = Internal::processPath(path);
        if (Internal::isNullOrEmpty(uri)) return success;
        BIND_TO_SCOPE(uri);

        if (GstDiscoverer *discoverer = gst_discoverer_new(5 * GST_SECOND, nullptr))
        {
            BIND_TO_SCOPE(discoverer);
            if (GstDiscovererInfo *info = gst_discoverer_discover_uri(discoverer, scoped_uri.pointer, nullptr))
            {
                BIND_TO_SCOPE(info);
                if (gst_discoverer_info_get_result(scoped_info.pointer) == GST_DISCOVERER_OK)
                {
                    if (GList *video_streams = gst_discoverer_info_get_video_streams(info))
                    {
                        BIND_TO_SCOPE(video_streams);
                        mHasVideo = (scoped_video_streams.pointer != nullptr);
                    }

                    if (GList *audio_streams = gst_discoverer_info_get_audio_streams(info))
                    {
                        BIND_TO_SCOPE(audio_streams);
                        mHasAudio = (scoped_audio_streams.pointer != nullptr);
                    }

                    mSeekable = gst_discoverer_info_get_seekable(info) != FALSE;
                    mDuration = gst_discoverer_info_get_duration(info) / gdouble(GST_SECOND);
                    success = true; // rest is video-specific

                    if (GstDiscovererStreamInfo *sinfo = gst_discoverer_info_get_stream_info(info))
                    {
                        BIND_TO_SCOPE(sinfo);
                        if (GST_IS_DISCOVERER_CONTAINER_INFO(scoped_sinfo.pointer))
                        {
                            if (GList *streams = gst_discoverer_container_info_get_streams(GST_DISCOVERER_CONTAINER_INFO(sinfo)))
                            {
                                BIND_TO_SCOPE(streams);
                                for (GList *curr = scoped_streams.pointer; curr; curr = curr->next)
                                {
                                    GstDiscovererStreamInfo *curr_sinfo = (GstDiscovererStreamInfo *)curr->data;
                                    if (GST_IS_DISCOVERER_VIDEO_INFO(curr_sinfo))
                                    {
                                        mWidth = gst_discoverer_video_info_get_width(GST_DISCOVERER_VIDEO_INFO(curr_sinfo));
                                        mHeight = gst_discoverer_video_info_get_height(GST_DISCOVERER_VIDEO_INFO(curr_sinfo));
                                        mFramerate = gst_discoverer_video_info_get_framerate_num(GST_DISCOVERER_VIDEO_INFO(curr_sinfo))
                                            / float(gst_discoverer_video_info_get_framerate_denom(GST_DISCOVERER_VIDEO_INFO(curr_sinfo)));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    catch (...) { /* we don't care about exceptions here */ }

    return success;
}

gint Discoverer::getWidth() const
{
    return mWidth;
}

gint Discoverer::getHeight() const
{
    return mHeight;
}

gfloat Discoverer::getFramerate() const
{
    return mFramerate;
}

bool Discoverer::getHasVideo() const
{
    return mHasVideo;
}

bool Discoverer::getHasAudio() const
{
    return mHasAudio;
}

bool Discoverer::getSeekable() const
{
    return mSeekable;
}

gdouble Discoverer::getDuration() const
{
    return mDuration;
}

//////////////////////////////////////////////////////////////////////////
// Internal implementation
//////////////////////////////////////////////////////////////////////////

template<> BindToScope<gchar>::~BindToScope()                   { g_free(pointer); pointer = nullptr; }
template<> BindToScope<GList>::~BindToScope()                   { gst_discoverer_stream_info_list_free(pointer); pointer = nullptr; }
template<> BindToScope<GError>::~BindToScope()                  { g_error_free(pointer); pointer = nullptr; }
template<> BindToScope<GstMessage>::~BindToScope()              { gst_message_unref(pointer); pointer = nullptr; }
template<> BindToScope<GstAppSink>::~BindToScope()              { g_object_unref(pointer); pointer = nullptr; }
template<> BindToScope<GstDiscoverer>::~BindToScope()           { g_object_unref(pointer); pointer = nullptr; }
template<> BindToScope<GstDiscovererInfo>::~BindToScope()       { gst_discoverer_info_unref(pointer); pointer = nullptr; }
template<> BindToScope<GstDiscovererStreamInfo>::~BindToScope() { gst_discoverer_stream_info_unref(pointer); pointer = nullptr; }

bool Internal::isNullOrEmpty(const gchar* const str)
{
    return str == nullptr || !*str;
}

gchar* Internal::processPath(const gchar* path)
{
    if (isNullOrEmpty(path))
    {
        g_debug("Cannot process an empty path.");
        return nullptr;
    }

    gchar* processed_path = nullptr;

    if (g_file_test(path, G_FILE_TEST_EXISTS) != FALSE) {
        // This will be NULL if path is already a valid URI
        gchar* uri = g_filename_to_uri(path, nullptr, nullptr);

        if (!isNullOrEmpty(uri)) {
            processed_path = uri;
        } else {
            processed_path = g_strdup(path);
        }
    }

    return processed_path;
}

void Internal::reset(ngw::Player& player)
{
    player.mState         = GST_STATE_NULL;
    player.mPipeline      = nullptr;
    player.mGstBus        = nullptr;
    player.mCurrentBuffer = nullptr;
    player.mCurrentSample = nullptr;
    player.mWidth         = 0;
    player.mHeight        = 0;
    player.mDuration      = 0;
    player.mTime          = 0.;
    player.mVolume        = 1.;
    player.mPendingSeek   = 0.;
    player.mSeekingLock   = false;
    g_atomic_int_set(&player.mBufferDirty, FALSE);
}

void Internal::reset(Discoverer& discoverer)
{
    discoverer.mWidth     = 0;
    discoverer.mHeight    = 0;
    discoverer.mFramerate = 0;
    discoverer.mHasAudio  = false;
    discoverer.mHasVideo  = false;
    discoverer.mSeekable  = false;
    discoverer.mDuration  = 0;
}

bool Internal::gstreamerInitialized()
{
    GError *init_error = nullptr;
    BIND_TO_SCOPE(init_error);

    if (gst_is_initialized() == FALSE &&
        gst_init_check(nullptr, nullptr, &init_error) == FALSE) {
        g_debug("GStreamer failed to initialize: %s.", scoped_init_error.pointer->message);
        return false;
    } else {
        return true;
    }
}

GstFlowReturn Internal::onPreroll(GstElement* appsink, ngw::Player* player)
{
    GstSample* sample = gst_app_sink_pull_preroll(GST_APP_SINK(appsink));

    // Here's our chance to get the actual dimension of the media.
    // The actual dimension might be slightly different from what
    // is passed into and requested from the pipeline.
    if (sample != nullptr)
    {
        if (GstCaps *caps = gst_sample_get_caps(sample))
        {
            if (gst_caps_is_fixed(caps) != FALSE)
            {
                if (const GstStructure *str = gst_caps_get_structure(caps, 0))
                {
                    if (gst_structure_get_int(str, "width", &player->mWidth) == FALSE ||
                        gst_structure_get_int(str, "height", &player->mHeight) == FALSE)
                    {
                        player->onError("No width/height information available.");
                    }
                }
            }
            else
            {
                player->onError("caps is not fixed for this media.");
            }
        }
    }

    processSample(player, sample);
    return GST_FLOW_OK;
}

GstFlowReturn Internal::onSampled(GstElement* appsink, ngw::Player* player)
{
    processSample(player, gst_app_sink_pull_sample(GST_APP_SINK(appsink)));
    return GST_FLOW_OK;
}

void Internal::processSample(ngw::Player *const player, GstSample* const sample)
{
    // Check if UI thread has consumed the last frame
    if (g_atomic_int_get(&player->mBufferDirty) != FALSE) {
        // Simply, skip this sample. UI is not consuming fast enough.
        gst_sample_unref(sample);
        return;
    }

    // Acquire and hold onto the new frame (until UI consumes it)
    player->mCurrentSample = sample;
    player->mCurrentBuffer = gst_sample_get_buffer(sample);
    gst_buffer_map(player->mCurrentBuffer, &player->mCurrentMapInfo, GST_MAP_READ);

    // Signal UI thread it can consume
    g_atomic_int_set(&player->mBufferDirty, TRUE);
}

void Internal::processDuration(Player& player)
{
    g_return_if_fail(player.mPipeline != nullptr);

    // Nanoseconds
    gint64 duration_ns = 0;
    if (gst_element_query_duration(GST_ELEMENT(player.mPipeline), GST_FORMAT_TIME, &duration_ns) != FALSE) {
        // Seconds
        player.mDuration = duration_ns / gdouble(GST_SECOND);
    }
}

}
