namespace ngw
{
    using UnityEngine;

    public class MediaPlayer : MonoBehaviour
    {
        Player mPlayer;

        void Start()
        {
            mPlayer = new Player();
        }

        void Update()
        {
            mPlayer.update();
        }

        void OnDestroy()
        {
            mPlayer.Dispose();
        }
    }
}