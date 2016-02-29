#pragma once

#include <gst/gst.h>

namespace ngw
{

void                addPluginPath(const gchar* path);
void                addBinaryPath(const gchar* path);

class Player
{
public:
    Player();
    virtual         ~Player();
    bool            open(const gchar *path, gint width, gint height, const gchar* fmt);
    bool            open(const gchar *path, gint width, gint height);
    bool            open(const gchar *path, const gchar* fmt);
    bool            open(const gchar *path);
    void            close();
    void            setState(GstState state);
    GstState        getState() const;
    void            stop();
    void            play();
    void            replay();
    void            pause();
    void            update();
    gdouble         getDuration() const;
    void            setLoop(bool on);
    bool            getLoop() const;
    void            setTime(gdouble time);
    gdouble         getTime() const;
    void            setVolume(gdouble vol);
    gdouble         getVolume() const;
    void            setMute(bool on);
    bool            getMute() const;
    gint            getWidth() const;
    gint            getHeight() const;
    void            setRate(gdouble rate);
    gdouble         getRate() const;

protected:
    virtual void    onFrame(guchar* buf, gsize size) const {};
    virtual void    onError(const gchar* msg) const {};
    virtual void    onState(GstState) const {};
    virtual void    onStreamEnd() const {};

    GstMapInfo      getMapInfo() const;
    GstSample*      getSample() const;
    GstBuffer*      getBuffer() const;

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

class Discoverer
{
public:
    bool            open(const gchar* path);
    gint            getWidth() const;
    gint            getHeight() const;
    gfloat          getFramerate() const;
    bool            getHasVideo() const;
    bool            getHasAudio() const;
    bool            getSeekable() const;
    gdouble         getDuration() const;

private:
    friend          class       Internal;
    gint            mWidth      = 0;        //!< Width of the discovered media
    gint            mHeight     = 0;        //!< Height of the discovered media
    gfloat          mFramerate  = 0;        //!< Frame rate of the discovered media
    gdouble         mDuration   = 0;        //!< Duration of the discovered media
    bool            mHasVideo   = false;    //!< Indicates whether media has video or not
    bool            mHasAudio   = false;    //!< Indicates whether media has audio or not
    bool            mSeekable   = false;    //!< Indicates whether media is seek able or not
};

}
