namespace ngw
{
    using System;
    using System.Runtime.InteropServices;

    public class Player : IDisposable
    {
        public enum State
        {
            Pending,
            Null,
            Ready,
            Paused,
            Playing
        }

        IntPtr m_NativePlayer   = IntPtr.Zero;

        GCHandle                m_ErrorCallbackHandle;
        GCHandle                m_StateCallbackHandle;
        GCHandle                m_StEndCallbackHandle;

        public Action<string>   OnErrorReceived;
        public Action<State>    OnStateChanged;
        public Action           OnStreamEnded;

        public Player()
        {
            m_NativePlayer = NativeMethods.ngw_player_make();

            var error_delegate = new NativeMethods.ERROR_CALLBACK_TYPE((msg, player)=> {
                if (OnErrorReceived != null)
                    OnErrorReceived(msg);
            });

            var state_delegate = new NativeMethods.STATE_CALLBACK_TYPE((old, player)=> {
                if (OnStateChanged != null)
                    OnStateChanged(old);
            });

            var stend_delegate = new NativeMethods.STREAM_END_CALLBACK_TYPE((player)=> {
                if (OnStreamEnded != null)
                    OnStreamEnded();
            });

            m_ErrorCallbackHandle = GCHandle.Alloc(error_delegate);
            m_StateCallbackHandle = GCHandle.Alloc(state_delegate);
            m_StEndCallbackHandle = GCHandle.Alloc(stend_delegate);

            NativeMethods.ngw_player_set_error_callback(m_NativePlayer, error_delegate);
            NativeMethods.ngw_player_set_state_callback(m_NativePlayer, state_delegate);
            NativeMethods.ngw_player_set_stream_end_callback(m_NativePlayer, stend_delegate);
        }

        public bool open(string path)
        {
            return NativeMethods.ngw_player_open(m_NativePlayer, path);
        }

        public bool open(string path, string format)
        {
            return NativeMethods.ngw_player_open_format(m_NativePlayer, path, format);
        }

        public bool open(string path, int width, int height)
        {
            return NativeMethods.ngw_player_open_resize(m_NativePlayer, path, width, height);
        }

        public bool open(string path, int width, int height, string format)
        {
            return NativeMethods.ngw_player_open_resize_format(m_NativePlayer, path, width, height, format);
        }

        public void setBuffer(IntPtr pinned_byte_pointer)
        {
            NativeMethods.ngw_player_set_sample_buffer(m_NativePlayer, pinned_byte_pointer, NativeMethods.NgwBuffer.BYTE_POINTER);
        }

        public void setBuffer(uint opengl_texture_name)
        {
            NativeMethods.ngw_player_set_sample_buffer(m_NativePlayer, (IntPtr)opengl_texture_name, NativeMethods.NgwBuffer.OPENGL_TEXTURE);
        }

        public int width
        {
            get { return NativeMethods.ngw_player_get_width(m_NativePlayer); }
        }

        public int height
        {
            get { return NativeMethods.ngw_player_get_height(m_NativePlayer); }
        }

        public State state
        {
            get { return NativeMethods.ngw_player_get_state(m_NativePlayer); }
            set { NativeMethods.ngw_player_set_state(m_NativePlayer, value); }
        }

        public bool Mute
        {
            get { return NativeMethods.ngw_player_get_mute(m_NativePlayer); }
            set { NativeMethods.ngw_player_set_mute(m_NativePlayer, value ? NativeMethods.NgwBool.TRUE : NativeMethods.NgwBool.FALSE); }
        }

        public bool Loop
        {
            get { return NativeMethods.ngw_player_get_loop(m_NativePlayer); }
            set { NativeMethods.ngw_player_set_loop(m_NativePlayer, value ? NativeMethods.NgwBool.TRUE : NativeMethods.NgwBool.FALSE); }
        }

        public double Volume
        {
            get { return NativeMethods.ngw_player_get_volume(m_NativePlayer); }
            set { NativeMethods.ngw_player_set_volume(m_NativePlayer, value); }
        }

        public double Time
        {
            get { return NativeMethods.ngw_player_get_time(m_NativePlayer); }
            set { NativeMethods.ngw_player_set_time(m_NativePlayer, value); }
        }

        public double Rate
        {
            get { return NativeMethods.ngw_player_get_rate(m_NativePlayer); }
            set { NativeMethods.ngw_player_set_rate(m_NativePlayer, value); }
        }

        public double Duration
        {
            get { return NativeMethods.ngw_player_get_duration(m_NativePlayer); }
        }

        public void play() { NativeMethods.ngw_player_play(m_NativePlayer); }
        public void stop() { NativeMethods.ngw_player_stop(m_NativePlayer); }
        public void pause() { NativeMethods.ngw_player_pause(m_NativePlayer); }
        public void replay() { NativeMethods.ngw_player_replay(m_NativePlayer); }
        public void update() { NativeMethods.ngw_player_update(m_NativePlayer); }

        #region IDisposable Support
        bool m_DisposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_DisposedValue)
            {
                NativeMethods.ngw_player_free(m_NativePlayer);
                m_NativePlayer = IntPtr.Zero;
                m_DisposedValue = true;

                if (disposing)
                {
                    m_ErrorCallbackHandle.Free();
                    m_StateCallbackHandle.Free();
                    m_StEndCallbackHandle.Free();
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
        IntPtr m_NativeDiscoverer = IntPtr.Zero;

        public Discoverer()
        {
            m_NativeDiscoverer = NativeMethods.ngw_discoverer_make();
        }

        public bool open(string path)
        {
            return NativeMethods.ngw_discoverer_open(m_NativeDiscoverer, path);
        }

        public int width
        {
            get { return NativeMethods.ngw_discoverer_get_width(m_NativeDiscoverer); }
        }

        public int height
        {
            get { return NativeMethods.ngw_discoverer_get_height(m_NativeDiscoverer); }
        }

        public bool seekable
        {
            get { return NativeMethods.ngw_discoverer_get_seekable(m_NativeDiscoverer); }
        }

        public bool hasAudio
        {
            get { return NativeMethods.ngw_discoverer_get_has_audio(m_NativeDiscoverer); }
        }

        public bool hasVideo
        {
            get { return NativeMethods.ngw_discoverer_get_has_video(m_NativeDiscoverer); }
        }

        public float frameRate
        {
            get { return NativeMethods.ngw_discoverer_get_framerate(m_NativeDiscoverer); }
        }

        public double duration
        {
            get { return NativeMethods.ngw_discoverer_get_duration(m_NativeDiscoverer); }
        }

        public uint sampleRate
        {
            get { return NativeMethods.ngw_discoverer_get_sample_rate(m_NativeDiscoverer); }
        }

        public uint bitRate
        {
            get { return NativeMethods.ngw_discoverer_get_bit_rate(m_NativeDiscoverer); }
        }

        #region IDisposable Support
        bool m_DisposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!m_DisposedValue)
            {
                if (disposing)
                {
                    // no managed resources to be freed.
                }

                NativeMethods.ngw_discoverer_free(m_NativeDiscoverer);
                m_NativeDiscoverer = IntPtr.Zero;
                m_DisposedValue = true;
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
        public enum NgwBool
        {
            FALSE               = 0,
            TRUE                = 1
        }

        public enum NgwBuffer
        {
            BYTE_POINTER        = 0,
            OPENGL_TEXTURE      = 1,
            CALLBACK_FUNCTION   = 2
        }

        public delegate void ERROR_CALLBACK_TYPE(string message, IntPtr player);
        public delegate void STATE_CALLBACK_TYPE(Player.State state, IntPtr player);
        public delegate void STREAM_END_CALLBACK_TYPE(IntPtr player);

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
        public static extern void ngw_player_set_loop(IntPtr player, NgwBool on);

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
        public static extern void ngw_player_set_mute(IntPtr player, NgwBool on);

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
        public static extern void ngw_player_set_sample_buffer(IntPtr player, IntPtr buffer, NgwBuffer type);

        [DllImport("ngw")]
        public static extern void ngw_player_set_error_callback(IntPtr player, ERROR_CALLBACK_TYPE cb);

        [DllImport("ngw")]
        public static extern void ngw_player_set_state_callback(IntPtr player, STATE_CALLBACK_TYPE cb);

        [DllImport("ngw")]
        public static extern void ngw_player_set_stream_end_callback(IntPtr player, STREAM_END_CALLBACK_TYPE cb);

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
