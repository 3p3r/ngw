using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoPlay : MonoBehaviour
{
    public MediaPlayer  MediaPlayer;
    public RawImage     RawImage;
	
	void Start ()
    {
        MediaPlayer.OnStreamOpened.AddListener(() =>
        {
            RawImage.texture = MediaPlayer.Texture;
            Debug.LogFormat("Opened a new stream. Path: {0}, Dim:{1}", MediaPlayer.Path, MediaPlayer.Dimension);
        });

        MediaPlayer.OnStreamEnded.AddListener(()=> {
            Debug.Log("Stream ended its playback");
        });
	}
}
