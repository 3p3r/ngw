using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    public InputField       InputField;
    public Slider           TimeSlider;
    public Slider           VolumeSlider;
    public GstreamerPlayer  MediaPlayer;
    public RawImage         RawImage;
	
	void Start ()
    {
        if (MediaPlayer == null)
            Debug.LogError("Media Player reference is empty");
        else if (RawImage == null)
            Debug.LogError("Raw Image reference is empty");
        else if (InputField == null)
            Debug.LogError("Input Field reference is empty");
        else if (VolumeSlider == null)
            Debug.LogError("Volume Slider reference is empty");
        else if (TimeSlider == null)
            Debug.LogError("Time Slider reference is empty");
        else
        {
            MediaPlayer.OnStreamOpened += () =>
            {
                RawImage.texture = MediaPlayer.Texture;
                TimeSlider.maxValue = (float)MediaPlayer.Duration;

                using (var discoverer = new ngw.Discoverer())
                {
                    if (discoverer.open(MediaPlayer.Path))
                    {
                        Debug.LogFormat("MediaPlayer Opened a new stream:\nURI: {0}, Width: {1}, Height: {2}, Video Framerate: {3}, Audio Samplerate: {4}, Bitrate: {5}, Duration: {6}, Audio? {7}, Video? {8}, Seekable? {9}",
                            discoverer.uri,
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

                TimeSlider.value = 0.0f;
            };

            MediaPlayer.OnStateChanged += () =>
            {
                Debug.LogFormat("MediaPlayer's state changed to: {0}", MediaPlayer.Status.ToString());

                if (MediaPlayer.Status == GstreamerPlayer.State.Stopped)
                    TimeSlider.value = 0.0f;
            };

            MediaPlayer.OnErrorReceived += (msg) =>
            {
                Debug.LogErrorFormat("MediaPlayer encountered and error: {0}", msg);

                TimeSlider.value = 0.0f;
                MediaPlayer.Close();
            };
        }
	}

    public void OpenInputField()
    {
        if (InputField.text != MediaPlayer.Path)
        {
            if (System.Uri.IsWellFormedUriString(InputField.text, System.UriKind.Absolute) ||
                System.IO.File.Exists(InputField.text))
            {
                MediaPlayer.Open(InputField.text);
            }
        }
    }

    public void AssignVolume()
    {
        MediaPlayer.Volume = VolumeSlider.value;
    }

    public void AssignTime()
    {
        if (Input.GetMouseButton(0))
            MediaPlayer.Time = TimeSlider.value;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
            return;

        if (MediaPlayer.Status == GstreamerPlayer.State.Playing)
            TimeSlider.value = (float)MediaPlayer.Time;
    }
}
