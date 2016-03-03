using System;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;

/// <summary>
/// A MonoBehaviour class which takes advantage of NGW API to
/// play back audio/video into a Unity Texture2D. This is just
/// a sample usage of NGW API. You may directly use methods
/// inside ngw.NativeMethods to suit your special needs.
/// NOTE: In order to achieve maximum performance, you may run
/// Unity in OpenGL mode and GstreamerPlayer render directly
/// into a texture instead of double buffering the video.
/// </summary>
public class GstreamerPlayer : MonoBehaviour
{
    #region Private Members

    private ngw.Player          mPlayer = new ngw.Player();
    private bool                mOpenGl = false;
    private IntPtr              mFrameBuffer;
    private byte[]              mDoubleBuffer;
    private Texture2D           mTexture;
    private string              mMediaPath;
    private GCHandle            mBufferHandle;

    #endregion

    #region Public API

    public UnityAction          OnStreamEnded;
    public UnityAction          OnStreamOpened;
    public UnityAction          OnStateChanged;
    public UnityAction<string>  OnErrorReceived;

    /// <summary>
    /// Unity applications need to be only concerned with three
    /// states. Simply GStreamer Null and Ready states are
    /// considered "Stopped" for the sake of simplicity.
    /// </summary>
    public enum State
    {
        Playing,
        Stopped,
        Paused
    }

    /// <summary>
    /// Field to obtain the current status of the player. Status
    /// could be Playing, Paused or Stopped.
    /// </summary>
    public State Status
    {
        get
        {
            if (mPlayer.state == ngw.NativeTypes.State.Playing)
                return State.Playing;
            else if (mPlayer.state == ngw.NativeTypes.State.Paused)
                return State.Paused;
            else return State.Stopped;
        }

        set
        {
            if (value == State.Playing)
                Play();
            else if (value == State.Paused)
                Pause();
            else if (value == State.Stopped)
                Stop();
        }
    }

    /// <summary>
    /// Texture2D which video frames are being rendered to. Use
    /// OnStreamOpened event to capture a valid texture after
    /// open(...) is called.
    /// </summary>
    public Texture2D Texture
    {
        get { return mTexture; }
    }

    /// <summary>
    /// Last path passed to this instance of GstreamerPlayer class.
    /// If you need a URI, use ngw.Discoverer instead.
    /// </summary>
    public string Path
    {
        get { return mMediaPath; }
        set { Open(value); }
    }

    /// <summary>
    /// Video Texture dimension. Might be (0,0) in case of audio.
    /// </summary>
    public Vector2 Dimension
    {
        get { return new Vector2(mPlayer.width, mPlayer.height); }
    }

    /// <summary>
    /// Field representing mute status of the media. If true,
    /// Volume field also returns zero.
    /// </summary>
    public bool Mute
    {
        get { return mPlayer.mute; }
        set { mPlayer.mute = value; }
    }

    /// <summary>
    /// Field representing looping status of the media. If true,
    /// playback loops after it finishes the stream.
    /// </summary>
    public bool Loop
    {
        get { return mPlayer.loop; }
        set { mPlayer.loop = value; }
    }

    /// <summary>
    /// Current time of the stream between zero and "Duration" field.
    /// </summary>
    public double Time
    {
        get { return mPlayer.time; }
        set { mPlayer.time = value; }
    }

    /// <summary>
    /// Rate of playback. Negative values mean reverse playback.
    /// NOTE: This is an experimental feature. DO NOT rely on it. Rate
    /// is also not supported for all media types. Use OnErrorReceived
    /// to capture any errors related to setting Rate.
    /// </summary>
    public double Rate
    {
        get { return mPlayer.rate; }
        set { mPlayer.rate = value; }
    }

    /// <summary>
    /// Playback volume. Always Returns zero if Mute is true. Value
    /// is between zero and one (percentage).
    /// </summary>
    public double Volume
    {
        get { return mPlayer.volume; }
        set { mPlayer.volume = value; }
    }

    /// <summary>
    /// Duration of opened media (READ ONLY).
    /// </summary>
    public double Duration
    {
        get { return mPlayer.duration; }
    }

    /// <summary>
    /// Sets Status to Playing
    /// </summary>
    public void Play() { mPlayer.play(); }

    /// <summary>
    /// Sets Status to Stopped
    /// </summary>
    public void Stop() { mPlayer.stop(); }

    /// <summary>
    /// Sets Status to Paused
    /// </summary>
    public void Pause() { mPlayer.pause(); }

    /// <summary>
    /// Closes the current stream and releases resources allocated
    /// for it. You hardly ever need to call this.
    /// </summary>
    public void Close() { mPlayer.close(); }
    
    /// <summary>
    /// Replays the current stream from the beginning to the end.
    /// Essentially calls stop() and play() in order.
    /// </summary>
    public void Replay() { mPlayer.replay(); }

    /// <summary>
    /// Opens a stream and render it to a BGRA texture. You may change
    /// the texture format by modifying AllocateFrameBuffer.
    /// NOTE: Be careful about changing the format! You need to have an
    /// in-depth knowledge of supported formats by both GStreamer and
    /// Unity AND their corresponding memory size. uploading BGRA to
    /// GPU is one of the fastest and most common ways of video playback
    /// even if video does not have any alpha channels!
    /// </summary>
    /// <param name="media">
    /// absolute path to a local file or a network URL
    /// Example 1: D:\movies\clip.mp4
    /// Example 2: http://docs.gstreamer.com/media/sintel_trailer-480p.webm
    /// </param>
    /// <remarks>
    /// This is a Synchronous call, meaning that it can block caller up to
    /// 10 seconds before it gives up on opening! Internet URLs usually
    /// take more than 5/6 seconds to open. It is advised you Open the URL
    /// at the start of your application and Play it whenever needed later.
    /// </remarks>
    public void Open(string media)
    {
        if (mPlayer.open(media))
        {
            AllocateFrameBuffer(mPlayer.width, mPlayer.height);

            if (mFrameBuffer != IntPtr.Zero)
            {
                mPlayer.setFrameBuffer(mFrameBuffer, mOpenGl
                    ? ngw.NativeTypes.Buffer.OpenGlTexture
                    : ngw.NativeTypes.Buffer.BytePointer);
            }

            mMediaPath = media;

            if (OnStreamOpened != null)
                OnStreamOpened();
        }
        else
        {
            ReleaseFrameBuffer();
            mMediaPath = string.Empty;
        }
    }

    #endregion

    #region Private API

    /// <summary>
    /// Allocates a frame buffer to be filled withing the unmanaged native
    /// code. This buffer can be either a byte pointer (for non OpenGL renderer)
    /// or a an OpenGL texture (for OpenGL renderer). In case of a byte pointer
    /// its data is copied once in the managed code and then transfered to the
    /// Texture2D in the Update() loop of this MonoBehaviour.
    /// </summary>
    /// <param name="width">expected width, can be zero if audio</param>
    /// <param name="height">expected height, can be zero if audio</param>
    void AllocateFrameBuffer(int width, int height)
    {
        ReleaseFrameBuffer();

        if (width > 0 && height > 0)
        {
            mTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);

            if (mOpenGl)
            {
                mBufferHandle = GCHandle.Alloc(mTexture, GCHandleType.Pinned);
                mFrameBuffer = mTexture.GetNativeTexturePtr();
            }
            else
            {
                mDoubleBuffer = new byte[width * height * 4];
                mBufferHandle = GCHandle.Alloc(mDoubleBuffer, GCHandleType.Pinned);
                mFrameBuffer = mBufferHandle.AddrOfPinnedObject();
            }
        }
    }

    /// <summary>
    /// Releases the frame buffer allocated by AllocateFrameBuffer(...)
    /// </summary>
    void ReleaseFrameBuffer()
    {
        if (mBufferHandle.IsAllocated)
        {
            mBufferHandle.Free();
        }

        if (mTexture != null)
        {
            Destroy(mTexture);
        }

        mDoubleBuffer = null;
        mFrameBuffer = IntPtr.Zero;
        mPlayer.setFrameBuffer(IntPtr.Zero, ngw.NativeTypes.Buffer.BytePointer);
    }

    /// <summary>
    /// In Awake(), we check if we have an OpenGL renderer first. We subscribe to
    /// native events of the ngw.Player instance and set Application.runInBackground
    /// to "true".
    /// </summary>
    /// <remarks>
    /// Application.runInBackground MUST be true otherwise GStreamer buffers will
    /// pile up and eventually crash Unity due to memory exhaustion.
    /// </remarks>
    void Awake()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore ||
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 ||
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
        {
            mOpenGl = true;
        }

        Application.runInBackground = true;

        mPlayer.OnStreamEnded += () => { if (OnStreamEnded != null) OnStreamEnded(); };
        mPlayer.OnStateChanged += (s) => { if (OnStateChanged != null) OnStateChanged(); };
        mPlayer.OnErrorReceived += (msg) => { if (OnErrorReceived != null) OnErrorReceived(msg); };
    }

    /// <summary>
    /// Update logic. Transfers copied video frames to the Unity texture in
    /// case of a running with a non OpenGL renderer.
    /// </summary>
    void Update()
    {
        mPlayer.update();

        if (!mOpenGl && mTexture != null)
        {
            mTexture.LoadRawTextureData(mDoubleBuffer);
            mTexture.Apply();
        }
    }

    /// <summary>
    /// Disposes the native ngw.Player and releases Unity resources.
    /// </summary>
    void OnDestroy()
    {
        ReleaseFrameBuffer();
        mPlayer.Dispose();
    }

    #endregion
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GstreamerPlayer))]
public class GstreamerPlayerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.EditorGUILayout.LabelField("You cannot use this editor while in EDIT mode.");
            UnityEditor.EditorGUILayout.LabelField("Hit Play and change the Media Path to begin.");
            return;
        }

        GstreamerPlayer player = (GstreamerPlayer)target;
        if (player == null) return;

        if (player.Dimension != Vector2.zero)
        {
            UnityEditor.EditorGUILayout.LabelField("Texture Width", player.Dimension.x.ToString());
            UnityEditor.EditorGUILayout.LabelField("Texture Height", player.Dimension.y.ToString());
        }

        string editor_path = UnityEditor.EditorGUILayout.TextField("Media Absolute Path or URI", player.Path);
        if (editor_path != player.Path)
        {
            if (Uri.IsWellFormedUriString(editor_path, UriKind.Absolute) ||
                System.IO.File.Exists(editor_path))
            {
                player.Open(editor_path);
            }
        }

        if (GUILayout.Button("Open")) { player.Open(player.Path); }
        if (GUILayout.Button("Play")) { player.Play(); }
        if (GUILayout.Button("Pause")) player.Pause();
        if (GUILayout.Button("Stop")) player.Stop();
        if (GUILayout.Button("Close")) player.Close();
        if (GUILayout.Button("Replay")) player.Replay();

        UnityEditor.EditorGUILayout.LabelField("Current Status", player.Status.ToString());

        bool editor_mute = UnityEditor.EditorGUILayout.Toggle("Mute", player.Mute);
        if (editor_mute != player.Mute) player.Mute = editor_mute;

        bool editor_loop = UnityEditor.EditorGUILayout.Toggle("Loop", player.Loop);
        if (editor_loop != player.Loop) player.Loop = editor_loop;

        if (player.Duration > 0)
        {
            float editor_pos = UnityEditor.EditorGUILayout.Slider("Time (s)", (float)player.Time, 0.0f, (float)player.Duration);
            if (Mathf.Abs((float)player.Time - editor_pos) > 0.001f * player.Duration) player.Time = editor_pos;

            float editor_vol = UnityEditor.EditorGUILayout.Slider("Volume (%)", (float)player.Volume, 0.0f, 1.0f);
            if (editor_vol != player.Volume) player.Volume = editor_vol;

            float editor_rate = UnityEditor.EditorGUILayout.FloatField("Playback Rate (negative is reverse)", (float)player.Rate);
            if (editor_rate != player.Rate) player.Rate = editor_rate;
        }

        Repaint();
    }

}
#endif
