using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    public MediaPlayer  MediaPlayer;
    public RawImage     RawImage;
	
	void Start ()
    {
        if (MediaPlayer == null)
            Debug.LogError("Media Player reference is empty");
        else if (RawImage == null)
            Debug.LogError("Raw Image reference is empty");
        else
        {
            MediaPlayer.OnStreamOpened += () =>
            {
                RawImage.texture = MediaPlayer.Texture;

                using (var discoverer = new ngw.Discoverer())
                {
                    if (discoverer.open(MediaPlayer.Path))
                    {
                        Debug.LogFormat("MediaPlayer Opened a new stream:\nPath: {0}, Width: {1}, Height: {2}, Video Framerate: {3}, Audio Samplerate: {4}, Bitrate: {5}, Duration: {6}, Audio? {7}, Video? {8}, Seekable? {9}",
                            MediaPlayer.Path,
                            discoverer.width,
                            discoverer.height,
                            discoverer.frameRate,
                            discoverer.sampleRate,
                            discoverer.bitRate,
                            discoverer.duration,
                            discoverer.hasAudio,
                            discoverer.hasVideo,
                            discoverer.seekable);
                    }
                }
            };

            MediaPlayer.OnStreamEnded += () =>
            {
                Debug.Log("MediaPlayer ended its playback");
            };

            MediaPlayer.OnStateChanged += () =>
            {
                Debug.LogFormat("MediaPlayer's state changed to: {0}", MediaPlayer.Status.ToString());
            };

            MediaPlayer.OnErrorReceived += (msg) =>
            {
                Debug.LogErrorFormat("MediaPlayer encountered and error: {0}", msg);
            };
        }
	}
}
