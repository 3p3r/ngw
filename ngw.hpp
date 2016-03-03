#pragma once

#include <gst/gst.h>

/*!
 * @namespace   ngw
 * @brief       Encloses two main classes of this library.
 *              ngw::Player is used to play media files.
 *              ngw::Discoverer is used to gather meta data.
 * @note        You need to also link against GStreamer SDK
 *              to be able to use ngw. Please consult the main
 *              README.md file shipped with this library.
 */
namespace ngw
{

/*!
 * @fn      addPluginPath
 * @brief   Adds a path to GStreamer's plug-in directories
 * @note    Uses GStreamer API, not modifying GST_PLUGIN_PATH
 * @param   path directory to be added to search directories
 */
void addPluginPath(const gchar* path);

/*!
 * @fn      addBinaryPath
 * @brief   Appends path to the end of PATH variable
 * @param   path directory to be appended to PATH
 */
void addBinaryPath(const gchar* path);

/*!
 * @class   Player
 * @brief   Media player class. Designed to play audio through system's
 *          default audio output (speakers) and hand of video frames to
 *          the user of the library.
 * @note    API of this class is not MT safe. Designed to be exclusively
 *          used in one thread and embedded in other game engines.
 * @usage   To obtain video frames, you need to subclass and override
 *          onFrame(...) method. Same goes for receiving events. To get
 *          event callbacks, on[name of function] should be overridden.
 */
class Player
{
public:
    Player();
    virtual         ~Player();
    //! opens a media file, can resize and reformat the video (if any). Returns true on success
    bool            open(const gchar *path, gint width, gint height, const gchar* fmt);
    //! opens a media file, can resize the video (if any). Returns true on success
    bool            open(const gchar *path, gint width, gint height);
    //! opens a media file, can reformat the video (if any). Returns true on success
    bool            open(const gchar *path, const gchar* fmt);
    //! opens a media file and auto detects its meta data and outputs 32bit BGRA. Returns true on success
    bool            open(const gchar *path);
    //! closes the current media file and its associated resources (no op if no media)
    void            close();
    //! sets state of the player (GST_STATE_PAUSED, etc.)
    void            setState(GstState state);
    //! answers the current state of the player (GST_STATE_PAUSED, etc.)
    GstState        getState() const;
    //! stops playback (setting time to 0)
    void            stop();
    //! resumes playback from its current time.
    void            play();
    //! replays the media from the beginning
    void            replay();
    //! pauses playback (leaving time at its current position)
    void            pause();
    //! update loop logic, MUST be called often in your engine's update loop
    void            update();
    //! answers duration of the media file. Valid after call to open(...)
    gdouble         getDuration() const;
    //! sets if the player should loop playback in the end (true) or not (false)
    void            setLoop(bool on);
    //! answers true if the player is currently looping playback
    bool            getLoop() const;
    //! seeks the media to a given time. NOTE: this is an async call, seek might
    //! not happen immediately. Cache occurs if an attempt is already in progress
    void            setTime(gdouble time);
    //! answers the current position of the player between [ 0. , getDuration() ]
    gdouble         getTime() const;
    //! sets the current volume of the player between [ 0. , 1. ]
    void            setVolume(gdouble vol);
    //! gets the current volume of the player between [ 0. , 1. ]
    gdouble         getVolume() const;
    //! sets if the player should mute playback (true) or not (false)
    void            setMute(bool on);
    //! answers true if the player is muted
    bool            getMute() const;
    //! answers width of the video, 0 if audio is being played. Valid after open(...)
    gint            getWidth() const;
    //! answers height of the video, 0 if audio is being played. Valid after open(...)
    gint            getHeight() const;
    //! sets playback rate (negative rate means reverse playback). NOTE: reverse playback
    //! might not be supported by all plug-ins. Do not rely on this feature heavily
    void            setRate(gdouble rate);
    //! gets the current rate of the playback (1. is normal speed forward playback)
    gdouble         getRate() const;

protected:
    //! Video frame callback, video buffer data and its size are passed in
    virtual void    onFrame(guchar* buf, gsize size) const {};
    //! Error callback, will be called if player encounters any errors. With a string message
    virtual void    onError(const gchar* msg) const {};
    //! State change event, propagated by the pipeline. Old state passed in, obtain new state with getState()
    virtual void    onState(GstState) const {};
    //! Called on end of the stream. Playback is finished at this point
    virtual void    onStreamEnd() const {};

    //! @cond inside onFrame API
    //! These APIs are present in case user of ngw needs to hold on to a frame beyond scope of the onFrame(...)
    //! API. for example the ABI stable target uses them to extract a buffer safely via gst_buffer_extract.
    //! NOTE: These APIs are ONLY valid inside onFrame(...) API!
    GstMapInfo      getMapInfo() const;
    GstSample*      getSample() const;
    GstBuffer*      getBuffer() const;
    //! @endcond

private:
    friend          class Internal;
    GstState        mState;                 //!< Current state of the player (playing, paused, etc.)
    GstMapInfo      mCurrentMapInfo;        //!< Mapped Buffer info, ONLY valid inside onFrame(...)
    GstSample       *mCurrentSample;        //!< Mapped Sample, ONLY valid inside onFrame(...)
    GstBuffer       *mCurrentBuffer;        //!< Mapped Buffer, ONLY valid inside onFrame(...)
    GstElement      *mPipeline;             //!< GStreamer pipeline (play-bin) object
    GstBus          *mGstBus;               //!< Bus associated with mPipeline

    mutable gint    mWidth      = 0;        //!< Width of the video being played. Valid after a call to open(...)
    mutable gint    mHeight     = 0;        //!< Height of the video being played. Valid after a call to open(...)
    mutable gdouble mDuration   = 0.;       //!< Duration of the media being played
    mutable gdouble mTime       = 0.;       //!< Current time of the media being played (current position)
    mutable gdouble mVolume     = 1.;       //!< Volume of the media being played
    mutable gdouble mRate       = 1.;       //!< Rate of playback, negative number for reverse playback

    volatile gint   mBufferDirty;           //!< Atomic boolean, representing a new frame is ready by GStreamer
    mutable gdouble mPendingSeek;           //!< Value of the seek operation pending to be executed
    mutable bool    mSeekingLock;           //!< Boolean flag, indicating a seek operation pending to be executed
    bool            mLoop       = false;    //!< Flag, indicating whether the player is looping or not
    bool            mMute       = false;    //!< Flag, indicating whether the player is muted or not
};

/*!
 * @class   Discoverer
 * @brief   Used to obtain meta data information about a media file without
 *          opening / playing it. Player class uses this internally to tell
 *          if a media file is audio-only or not and get its media duration
 */
class Discoverer
{
public:
    Discoverer();
    Discoverer(const Discoverer&);
    virtual         ~Discoverer();
    //! Attempts to open a media for discovery. NOTE: does not work very well for
    //! Internet based URLs. Some URLs are not discoverable but some are. More
    //! reliable to be used with local medias. Returns true on success
    bool            open(const gchar* path);
    //! Returns path to discovered media or empty string ("") on failure
    const gchar*    getUri() const;
    //! Returns width of the media if it contains video (0 otherwise)
    gint            getWidth() const;
    //! Returns height of the media if it contains video (0 otherwise)
    gint            getHeight() const;
    //! Returns frame rate of the media if it contains video (0 otherwise)
    gfloat          getFramerate() const;
    //! Answers true if media contains video
    bool            getHasVideo() const;
    //! Answers true if media contains audio
    bool            getHasAudio() const;
    //! Answers true if media is seek-able (false for streams)
    bool            getSeekable() const;
    //! Answers duration of the discovered media
    gdouble         getDuration() const;
    //! Returns sample rate of the associated audio stream (0 if missing)
    guint           getSampleRate() const;
    //! Returns bit rate of the associated audio stream (0 if missing)
    guint           getBitRate() const;

private:
    friend          class       Internal;
    gchar*          mMediaUri   = nullptr;  //!< URI to the discovered media
    gint            mWidth      = 0;        //!< Width of the discovered media
    gint            mHeight     = 0;        //!< Height of the discovered media
    gfloat          mFramerate  = 0;        //!< Frame rate of the discovered media
    gdouble         mDuration   = 0;        //!< Duration of the discovered media
    guint           mSampleRate = 0;        //!< Sample rate of the discovered media (audio)
    guint           mBitRate    = 0;        //!< Bit rate of the discovered media in bits/second (audio)
    bool            mHasVideo   = false;    //!< Indicates whether media has video or not
    bool            mHasAudio   = false;    //!< Indicates whether media has audio or not
    bool            mSeekable   = false;    //!< Indicates whether media is seek able or not
};

} // !namespace ngw
