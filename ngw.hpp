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
    GstState        mState;
    GstMapInfo      mCurrentMapInfo;
    GstSample       *mCurrentSample;
    GstBuffer       *mCurrentBuffer;
    GstElement      *mPipeline;
    GstBus          *mGstBus;

    mutable gint    mWidth;
    mutable gint    mHeight;
    mutable gdouble mDuration;
    mutable gdouble mTime;
    mutable gdouble mVolume;

    volatile gint   mBufferDirty;
    mutable gdouble mPendingSeek;
    mutable bool    mSeekingLock;
    bool            mLoop;
    bool            mMute;
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
    friend          class        Internal;
    gint            mWidth       = 0;
    gint            mHeight      = 0;
    gfloat          mFramerate   = 0;
    gdouble         mDuration    = 0;
    bool            mHasVideo    = false;
    bool            mHasAudio    = false;
    bool            mSeekable    = false;
};

}
