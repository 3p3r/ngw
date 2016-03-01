namespace ngw
{
    using System;
    using System.Runtime.InteropServices;

    public class Player : IDisposable
    {
        public enum State
        {
            Pending         = 0,
            Null            = 1,
            Ready           = 2,
            Paused          = 3,
            Playing         = 4
        }

        public enum BufferType
        {
            BYTE_POINTER    = 0,
            OPENGL_TEXTURE  = 1
        }

        IntPtr mNativePlayer   = IntPtr.Zero;

        GCHandle                mErrorCallbackHandle;
        GCHandle                mStateCallbackHandle;
        GCHandle                mStEndCallbackHandle;

        public Action<string>   OnErrorReceived;
        public Action<State>    OnStateChanged;
        public Action           OnStreamEnded;

        public Player()
        {
            mNativePlayer = NativeMethods.ngw_player_make();

            var error_delegate = new NativeMethods.ErrorDelegate((msg, player)=> {
                if (OnErrorReceived != null)
                    OnErrorReceived(msg);
            });

            var state_delegate = new NativeMethods.StateDelegate((old, player)=> {
                if (OnStateChanged != null)
                    OnStateChanged(old);
            });

            var stend_delegate = new NativeMethods.StreamEndDelegate((player)=> {
                if (OnStreamEnded != null)
                    OnStreamEnded();
            });

            mErrorCallbackHandle = GCHandle.Alloc(error_delegate);
            mStateCallbackHandle = GCHandle.Alloc(state_delegate);
            mStEndCallbackHandle = GCHandle.Alloc(stend_delegate);

            NativeMethods.ngw_player_set_error_callback(mNativePlayer, error_delegate);
            NativeMethods.ngw_player_set_state_callback(mNativePlayer, state_delegate);
            NativeMethods.ngw_player_set_stream_end_callback(mNativePlayer, stend_delegate);
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

        public void setFrameBuffer(IntPtr pinned_frame_buffer, BufferType type)
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

        public State state
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
        public void pause() { NativeMethods.ngw_player_pause(mNativePlayer); }
        public void replay() { NativeMethods.ngw_player_replay(mNativePlayer); }
        public void update() { NativeMethods.ngw_player_update(mNativePlayer); }

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
                    mErrorCallbackHandle.Free();
                    mStateCallbackHandle.Free();
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

    public class Discoverer : IDisposable
    {
        IntPtr mNativeDiscoverer = IntPtr.Zero;

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
            get { return NativeMethods.ngw_discoverer_get_framerate(mNativeDiscoverer); }
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

    internal static class NativeMethods
    {
        public delegate void ErrorDelegate(string message, IntPtr player);
        public delegate void StateDelegate(Player.State state, IntPtr player);
        public delegate void StreamEndDelegate(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_add_plugin_path(string path);

        [DllImport("ngw")]
        public static extern void ngw_add_binary_path(string path);

        [DllImport("ngw")]
        public static extern IntPtr ngw_player_make();

        [DllImport("ngw")]
        public static extern void ngw_player_free(IntPtr player);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open_resize_format(IntPtr player, string path, int width, int height, string fmt);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open_resize(IntPtr player, string path, int width, int height);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open_format(IntPtr player, string path, string fmt);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open(IntPtr player, string path);

        [DllImport("ngw")]
        public static extern void ngw_player_close(IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_state(IntPtr player, Player.State state);

        [DllImport("ngw")]
        public static extern Player.State ngw_player_get_state(IntPtr player);

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
        public static extern void ngw_player_set_frame_buffer(IntPtr player, IntPtr buffer, Player.BufferType type);

        [DllImport("ngw")]
        public static extern void ngw_player_set_error_callback(IntPtr player, ErrorDelegate cb);

        [DllImport("ngw")]
        public static extern void ngw_player_set_state_callback(IntPtr player, StateDelegate cb);

        [DllImport("ngw")]
        public static extern void ngw_player_set_stream_end_callback(IntPtr player, StreamEndDelegate cb);

        [DllImport("ngw")]
        public static extern double ngw_player_get_rate(IntPtr player);

        [DllImport("ngw")]
        public static extern IntPtr ngw_discoverer_make();

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_open(IntPtr discoverer, string path);

        [DllImport("ngw")]
        public static extern IntPtr ngw_discoverer_get_path(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern int ngw_discoverer_get_width(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern int ngw_discoverer_get_height(IntPtr discoverer);

        [DllImport("ngw")]
        public static extern float ngw_discoverer_get_framerate(IntPtr discoverer);

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

    } // class NativeMethods

} // namespace ngw
