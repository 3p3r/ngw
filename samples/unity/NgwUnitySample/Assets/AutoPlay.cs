using UnityEngine;
using UnityEngine.UI;

public class AutoPlay : MonoBehaviour
{
    public MediaPlayer  MediaPlayer;
    public RawImage     RawImage;
	
	void Start ()
    {
        MediaPlayer.OnStreamOpened += () =>
        {
            RawImage.texture = MediaPlayer.Texture;
            Debug.LogFormat("MediaPlayer Opened a new stream. Path: {0}, Dim:{1}", MediaPlayer.Path, MediaPlayer.Dimension);
        };

        MediaPlayer.OnStreamEnded += ()=> {
            Debug.Log("MediaPlayer ended its playback");
        };

        MediaPlayer.OnStateChanged += ()=> {
            Debug.LogFormat("MediaPlayer's state changed to: {0}", MediaPlayer.Status.ToString());
        };

        MediaPlayer.OnErrorReceived += (msg) => {
            Debug.LogErrorFormat("MediaPlayer encountered and error: {0}", msg);
        };
	}
}
