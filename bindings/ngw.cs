namespace ngw
{
    using System.Runtime.InteropServices;
    
    internal static class NativeMethods
    {
        public enum NgwBool
        {
            FALSE             = 0,
            TRUE              = 1
        }
        
        public enum NgwBuffer
        {
            BYTE_POINTER      = 0,
            OPENGL_TEXTURE    = 1,
            CALLBACK_FUNCTION = 2
        }
        
        public enum NgwState
        {
            VOID_PENDING      = 0,
            NULL              = 1,
            READY             = 2,
            PAUSED            = 3,
            PLAYING           = 4
        }
        
        [DllImport("ngw")]
        public static extern void ngw_add_plugin_path(string path);
        
        [DllImport("ngw")]
        public static extern void ngw_add_binary_path(string path);

        [DllImport("ngw")]
        public static extern System.IntPtr ngw_player_make();
        
        [DllImport("ngw")]
        public static extern void ngw_player_free(System.IntPtr player);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open(System.IntPtr player, string path, int width, int height, string fmt);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open(System.IntPtr player, string path, int width, int height);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open(System.IntPtr player, string path, string fmt);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_open(System.IntPtr player, string path);

        [DllImport("ngw")]
        public static extern void ngw_player_close(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_state(System.IntPtr player, NgwState state);

        [DllImport("ngw")]
        public static extern NgwState ngw_player_get_state(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_stop(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_play(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_replay(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_pause(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_update(System.IntPtr player);

        [DllImport("ngw")]
        public static extern double ngw_player_get_duration(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_loop(System.IntPtr player, NgwBool on);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_get_loop(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_time(System.IntPtr player, double time);

        [DllImport("ngw")]
        public static extern double ngw_player_get_time(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_volume(System.IntPtr player, double vol);

        [DllImport("ngw")]
        public static extern double ngw_player_get_volume(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_mute(System.IntPtr player, NgwBool on);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_player_get_mute(System.IntPtr player);

        [DllImport("ngw")]
        public static extern int ngw_player_get_width(System.IntPtr player);

        [DllImport("ngw")]
        public static extern int ngw_player_get_height(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_rate(System.IntPtr player, double rate);

        [DllImport("ngw")]
        public static extern void ngw_player_set_user_data(System.IntPtr player, System.IntPtr data);

        [DllImport("ngw")]
        public static extern System.IntPtr ngw_player_get_user_data(System.IntPtr player);

        [DllImport("ngw")]
        public static extern void ngw_player_set_sample_buffer(System.IntPtr player, System.IntPtr buffer, NgwBuffer type);

        [DllImport("ngw")]
        public static extern double ngw_player_get_rate(System.IntPtr player);
        
        [DllImport("ngw")]
        public static extern System.IntPtr ngw_discoverer_make();
        
        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_open(System.IntPtr discoverer, string path);

        [DllImport("ngw")]
        public static extern System.IntPtr ngw_discoverer_get_path(System.IntPtr discoverer);

        [DllImport("ngw")]
        public static extern int ngw_discoverer_get_width(System.IntPtr discoverer);

        [DllImport("ngw")]
        public static extern int ngw_discoverer_get_height(System.IntPtr discoverer);

        [DllImport("ngw")]
        public static extern float ngw_discoverer_get_framerate(System.IntPtr discoverer);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_get_has_video(System.IntPtr discoverer);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_get_has_audio(System.IntPtr discoverer);

        [DllImport("ngw")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ngw_discoverer_get_seekable(System.IntPtr discoverer);

        [DllImport("ngw")]
        public static extern double ngw_discoverer_get_duration(System.IntPtr discoverer);

        [DllImport("ngw")]
        public static extern uint ngw_discoverer_get_sample_rate(System.IntPtr discoverer);

        [DllImport("ngw")]
        public static extern uint ngw_discoverer_get_bit_rate(System.IntPtr discoverer);
        
        [DllImport("ngw")]
        public static extern void ngw_discoverer_free(System.IntPtr discoverer);
        
    } // class NativeMethods
    
} // namespace ngw
