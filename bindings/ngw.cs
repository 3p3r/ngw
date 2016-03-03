namespace ngw
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper GStreamer player class, provides the same functionality
    /// of its C++ counterpart. Use of this class is optional. You can
    /// directly use DllImported methods inside NativeMethods class.
    /// NOTE: remember before passing any callbacks or pointers inside
    /// any of the DllImported methods, they need to be Pinned by GC.
    /// </summary>
    public class Player : IDisposable
    {
        #region Private Members

        IntPtr mNativePlayer                = IntPtr.Zero;

        GCHandle                            mErrorCallbackHandle;
        GCHandle                            mStateCallbackHandle;
        GCHandle                            mStEndCallbackHandle;

        public Action<NativeTypes.State>    OnStateChanged;
        public Action<string>               OnErrorReceived;
        public Action                       OnStreamEnded;

        #endregion

        #region Player API

        /// <summary>
        /// Constructor, taking care of registering GC pinned callbacks
        /// properly and turning them into C# events instead. Users of
        /// this class can subscribe to On[event name] events.
        /// </summary>
        public Player()
        {
            mNativePlayer = NativeMethods.ngw_player_make();

            if (mNativePlayer != IntPtr.Zero)
            {
                var error_delegate = new NativeTypes.ErrorDelegate((msg, player) =>
                            {
                                if (OnErrorReceived != null)
                                    OnErrorReceived(msg);
                            });

                var state_delegate = new NativeTypes.StateDelegate((old, player) =>
                {
                    if (OnStateChanged != null)
                        OnStateChanged(old);
                });

                var stend_delegate = new NativeTypes.StreamEndDelegate((player) =>
                {
                    if (OnStreamEnded != null)
                        OnStreamEnded();
                });

                mErrorCallbackHandle = GCHandle.Alloc(error_delegate, GCHandleType.Pinned);
                mStateCallbackHandle = GCHandle.Alloc(state_delegate, GCHandleType.Pinned);
                mStEndCallbackHandle = GCHandle.Alloc(stend_delegate, GCHandleType.Pinned);

                NativeMethods.ngw_player_set_error_callback(mNativePlayer, error_delegate);
                NativeMethods.ngw_player_set_state_callback(mNativePlayer, state_delegate);
                NativeMethods.ngw_player_set_stream_end_callback(mNativePlayer, stend_delegate);
            }
        }

        public bool open(string path)
        {
            return NativeMethods.ngw_player_open(mNativePlayer, path);
        }

        public bool open(string path, string format)
        {
            return NativeMethods.ngw_player_open_format(mNativePlayer, path, format);
        }

        public bool open(string path, int width, int height)
        {
            return NativeMethods.ngw_player_open_resize(mNativePlayer, path, width, height);
        }

        public bool open(string path, int width, int height, string format)
        {
            return NativeMethods.ngw_player_open_resize_format(mNativePlayer, path, width, height, format);
        }

        public void setFrameBuffer(IntPtr pinned_frame_buffer, NativeTypes.Buffer type)
        {
            NativeMethods.ngw_player_set_frame_buffer(mNativePlayer, pinned_frame_buffer, type);
        }

        public int width
        {
            get { return NativeMethods.ngw_player_get_width(mNativePlayer); }
        }

        public int height
        {
            get { return NativeMethods.ngw_player_get_height(mNativePlayer); }
        }

        public NativeTypes.State state
        {
            get { return NativeMethods.ngw_player_get_state(mNativePlayer); }
            set { NativeMethods.ngw_player_set_state(mNativePlayer, value); }
        }

        public bool mute
        {
            get { return NativeMethods.ngw_player_get_mute(mNativePlayer); }
            set { NativeMethods.ngw_player_set_mute(mNativePlayer, value); }
        }

        public bool loop
        {
            get { return NativeMethods.ngw_player_get_loop(mNativePlayer); }
            set { NativeMethods.ngw_player_set_loop(mNativePlayer, value); }
        }

        public double volume
        {
            get { return NativeMethods.ngw_player_get_volume(mNativePlayer); }
            set { NativeMethods.ngw_player_set_volume(mNativePlayer, value); }
        }

        public double time
        {
            get { return NativeMethods.ngw_player_get_time(mNativePlayer); }
            set { NativeMethods.ngw_player_set_time(mNativePlayer, value); }
        }

        public double rate
        {
            get { return NativeMethods.ngw_player_get_rate(mNativePlayer); }
            set { NativeMethods.ngw_player_set_rate(mNativePlayer, value); }
        }

        public double duration
        {
            get { return NativeMethods.ngw_player_get_duration(mNativePlayer); }
        }

        public void play() { NativeMethods.ngw_player_play(mNativePlayer); }
        public void stop() { NativeMethods.ngw_player_stop(mNativePlayer); }
        public void close() { NativeMethods.ngw_player_close(mNativePlayer); }
        public void pause() { NativeMethods.ngw_player_pause(mNativePlayer); }
        public void replay() { NativeMethods.ngw_player_replay(mNativePlayer); }
        public void update() { NativeMethods.ngw_player_update(mNativePlayer); }

        #endregion

        #region IDisposable Support
        bool mDisposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposedValue)
            {
                NativeMethods.ngw_player_free(mNativePlayer);
                mNativePlayer = IntPtr.Zero;
                mDisposedValue = true;

                if (disposing)
                {
                    if (mErrorCallbackHandle.IsAllocated)
                        mErrorCallbackHandle.Free();

                    if (mStateCallbackHandle.IsAllocated)
                        mStateCallbackHandle.Free();

                    if (mStEndCallbackHandle.IsAllocated)
                        mStEndCallbackHandle.Free();
                }
            }
        }

        ~Player()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Wrapper GStreamer discoverer class, provides the same functionality
    /// of its C++ counterpart. Use of this class is optional. You can
    /// directly use DllImported methods inside NativeMethods class.
    /// </summary>
    public class Discoverer : IDisposable
    {
        #region Private Members

        IntPtr mNativeDiscoverer = IntPtr.Zero;

        #endregion

        #region Discoverer API

        public Discoverer()
        {
            mNativeDiscoverer = NativeMethods.ngw_discoverer_make();
        }

        public bool open(string path)
        {
            return NativeMethods.ngw_discoverer_open(mNativeDiscoverer, path);
        }

        public int width
        {
            get { return NativeMethods.ngw_discoverer_get_width(mNativeDiscoverer); }
        }

        public int height
        {
            get { return NativeMethods.ngw_discoverer_get_height(mNativeDiscoverer); }
        }

        public bool seekable
        {
            get { return NativeMethods.ngw_discoverer_get_seekable(mNativeDiscoverer); }
        }

        public bool hasAudio
        {
            get { return NativeMethods.ngw_discoverer_get_has_audio(mNativeDiscoverer); }
        }

        public bool hasVideo
        {
            get { return NativeMethods.ngw_discoverer_get_has_video(mNativeDiscoverer); }
        }

        public float frameRate
        {
            get { return NativeMethods.ngw_discoverer_get_frame_rate(mNativeDiscoverer); }
        }

        public double duration
        {
            get { return NativeMethods.ngw_discoverer_get_duration(mNativeDiscoverer); }
        }

        public uint sampleRate
        {
            get { return NativeMethods.ngw_discoverer_get_sample_rate(mNativeDiscoverer); }
        }

        public uint bitRate
        {
            get { return NativeMethods.ngw_discoverer_get_bit_rate(mNativeDiscoverer); }
        }

        public string uri
        {
            get { return Marshal.PtrToStringAnsi(NativeMethods.ngw_discoverer_get_uri(mNativeDiscoverer)); }
        }

        #endregion

        #region IDisposable Support
        bool mDisposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposedValue)
            {
                if (disposing)
                {
                    // no managed resources to be freed.
                }

                NativeMethods.ngw_discoverer_free(mNativeDiscoverer);
                mNativeDiscoverer = IntPtr.Zero;
                mDisposedValue = true;
            }
        }

        ~Discoverer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class NativeTypes
    {
        #region C Callback Types

        public delegate void ErrorDelegate([MarshalAs(UnmanagedType.LPStr)] string message, IntPtr player);
        public delegate void FrameDelegate(IntPtr buffer, uint size, IntPtr player);
        public delegate void StateDelegate(State state, IntPtr player);
        public delegate void StreamEndDelegate(IntPtr player);

        #endregion

        #region C Data Types

        public enum State
        {
            Pending,
            Null,
            Ready,
            Paused,
            Playing
        }

        public enum Buffer
        {
            BytePointer,
            OpenGlTexture,
            CallbackFunction
        }

        #endregion
    }

    internal static class NativeMethods
    {
        [DllImport("ngw")]
        public static extern void ngw_add_plugin_path([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("ngw")]
        public static extern void ngw_add_binary_path([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("ngw")]
        public static extern IntPtr ngw_player_make();

        [DllImport("ngw")]
        public static extern void ngw_player_free(IntPtr player);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open_resize_format(IntPtr player, [MarshalAs(UnmanagedType.LPStr)] string path, int width, int height, [MarshalAs(UnmanagedType.LPStr)] string fmt);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open_resize(IntPtr player, [MarshalAs(UnmanagedType.LPStr)] string path, int width, int height);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open_format(IntPtr player, [MarshalAs(UnmanagedType.LPStr)] string path, [MarshalAs(UnmanagedType.LPStr)] string fmt);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open(IntPtr player, [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("ngw")]
        public static extern void ngw_player_close(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_state(IntPtr player, NativeTypes.State state);

        [DllImport("ngw")]
        public static extern NativeTypes.State ngw_player_get_state(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_stop(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_play(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_replay(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_pause(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_update(IntPtr player);

        [DllImport("ngw")]
        public static extern double ngw_player_get_duration(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_loop(IntPtr player, [MarshalAs(UnmanagedType.Bool)] bool on);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_get_loop(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_time(IntPtr player, double time);

        [DllImport("ngw")]
        public static extern double ngw_player_get_time(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_volume(IntPtr player, double vol);

        [DllImport("ngw")]
        public static extern double ngw_player_get_volume(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_mute(IntPtr player, [MarshalAs(UnmanagedType.Bool)] bool on);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_get_mute(IntPtr player);

        [DllImport("ngw")]
        public static extern int ngw_player_get_width(IntPtr player);

        [DllImport("ngw")]
        public static extern int ngw_player_get_height(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_rate(IntPtr player, double rate);

        [DllImport("ngw")]
        public static extern void ngw_player_set_user_data(IntPtr player, IntPtr data);

        [DllImport("ngw")]
        public static extern IntPtr ngw_player_get_user_data(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_frame_buffer(IntPtr player, IntPtr buffer, NativeTypes.Buffer type);

        [DllImport("ngw")]
        public static extern void ngw_player_set_error_callback(IntPtr player, NativeTypes.ErrorDelegate cb);

        [DllImport("ngw")]
        public static extern void ngw_player_set_state_callback(IntPtr player, NativeTypes.StateDelegate cb);

        [DllImport("ngw")]
        public static extern void ngw_player_set_stream_end_callback(IntPtr player, NativeTypes.StreamEndDelegate cb);

        [DllImport("ngw")]
        public static extern double ngw_player_get_rate(IntPtr player);

        [DllImport("ngw")]
        public static extern IntPtr ngw_discoverer_make();

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_open(IntPtr discoverer, [MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("ngw")]
        public static extern int ngw_discoverer_get_width(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern int ngw_discoverer_get_height(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern float ngw_discoverer_get_frame_rate(IntPtr discoverer);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_get_has_video(IntPtr discoverer);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_get_has_audio(IntPtr discoverer);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_get_seekable(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern double ngw_discoverer_get_duration(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern uint ngw_discoverer_get_sample_rate(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern uint ngw_discoverer_get_bit_rate(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern void ngw_discoverer_free(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern IntPtr ngw_discoverer_get_uri(IntPtr discoverer);

    } // class NativeMethods

} // namespace ngw
