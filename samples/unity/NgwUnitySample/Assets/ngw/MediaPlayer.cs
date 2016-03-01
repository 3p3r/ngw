using System;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;

public class MediaPlayer : MonoBehaviour
{
    #region Private Members

    ngw.Player  mPlayer         = new ngw.Player();
    bool        mOpenGlRenderer = false;
    byte[]      mDoubleBuffer   = null;
    Texture2D   mTexture        = null;
    IntPtr      mFrameBuffer    = IntPtr.Zero;
    string      mMediaPath      = "";
    GCHandle    mBufferHandle;

    #endregion

    #region Public API

    public UnityAction          OnStreamEnded;
    public UnityAction          OnStreamOpened;
    public UnityAction          OnStateChanged;
    public UnityAction<string>  OnErrorReceived;

    [Serializable]
    public enum State
    {
        Playing,
        Stopped,
        Paused
    }

    public State Status
    {
        get
        {
            if (mPlayer.state == ngw.Player.State.Playing)
                return State.Playing;
            else if (mPlayer.state == ngw.Player.State.Paused)
                return State.Paused;
            else return State.Stopped;
        }
    }

    public Texture2D Texture
    {
        get { return mTexture; }
    }

    public string Path
    {
        get { return mMediaPath; }
        set { Open(value); }
    }

    public Vector2 Dimension
    {
        get { return new Vector2(mPlayer.width, mPlayer.height); }
    }

    public bool Mute
    {
        get { return mPlayer.mute; }
        set { mPlayer.mute = value; }
    }

    public bool Loop
    {
        get { return mPlayer.loop; }
        set { mPlayer.loop = value; }
    }

    public double Time
    {
        get { return mPlayer.time; }
        set { mPlayer.time = value; }
    }

    public double Rate
    {
        get { return mPlayer.rate; }
        set { mPlayer.rate = value; }
    }

    public double Volume
    {
        get { return mPlayer.volume; }
        set { mPlayer.volume = value; }
    }

    public double Duration
    {
        get { return mPlayer.duration; }
    }

    public void Play() { mPlayer.play(); }
    public void Stop() { mPlayer.stop(); }
    public void Close() { mPlayer.close(); }
    public void Pause() { mPlayer.pause(); }
    public void Replay() { mPlayer.replay(); }

    public void Open(string media)
    {
        if (mPlayer.open(media))
        {
            AllocateFrameBuffer(mPlayer.width, mPlayer.height);
            mPlayer.setFrameBuffer(mFrameBuffer, mOpenGlRenderer
                ? ngw.Player.BufferType.OPENGL_TEXTURE
                : ngw.Player.BufferType.BYTE_POINTER);
            mMediaPath = media;

            if (OnStreamOpened != null)
                OnStreamOpened();
        }
        else {
            ReleaseFrameBuffer();
            mMediaPath = string.Empty;
        }
    }

    #endregion

    #region Private API

    void AllocateFrameBuffer(int width, int height)
    {
        ReleaseFrameBuffer();

        mTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);

        if (mOpenGlRenderer) {
            mFrameBuffer = mTexture.GetNativeTexturePtr();
        }
        else {
            mDoubleBuffer = new byte[width * height * 4];
            mBufferHandle = GCHandle.Alloc(mDoubleBuffer, GCHandleType.Pinned);
            mFrameBuffer = mBufferHandle.AddrOfPinnedObject();
        }
    }

    void ReleaseFrameBuffer()
    {
        if (!mOpenGlRenderer && mBufferHandle.IsAllocated) {
            mBufferHandle.Free();
        }
        else if (mOpenGlRenderer) {
            Destroy(mTexture);
        }

        mPlayer.setFrameBuffer(IntPtr.Zero, ngw.Player.BufferType.BYTE_POINTER);
    }

    void Awake()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore ||
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2 ||
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
        {
            mOpenGlRenderer = true;
        }

        Application.runInBackground = true;

        mPlayer.OnStreamEnded += () => { if (OnStreamEnded != null) OnStreamEnded(); };
        mPlayer.OnStateChanged += (s) => { if (OnStateChanged != null) OnStateChanged(); };
        mPlayer.OnErrorReceived += (msg) => { if (OnErrorReceived != null) OnErrorReceived(msg); };
    }

    void Update()
    {
        mPlayer.update();

        if (!mOpenGlRenderer && mTexture)
        {
            mTexture.LoadRawTextureData(mDoubleBuffer);
            mTexture.Apply();
        }
    }

    void OnDestroy()
    {
        ReleaseFrameBuffer();
        mPlayer.Dispose();
    }

    #endregion
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(MediaPlayer))]
public class MediaPlayerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.EditorGUILayout.LabelField("You cannot use this editor while in EDIT mode.");
            UnityEditor.EditorGUILayout.LabelField("Hit Play and change the Media Path.");
            return;
        }

        MediaPlayer player = (MediaPlayer)target;
        if (player == null) return;

        UnityEditor.EditorGUILayout.Vector2Field("Dimension", player.Dimension);

        string editor_path = UnityEditor.EditorGUILayout.TextField("Media Absolute Path or URI", player.Path);
        if (editor_path != player.Path) {
            if (Uri.IsWellFormedUriString(editor_path, UriKind.Absolute) ||
                System.IO.File.Exists(editor_path)) {
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
