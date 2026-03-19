using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.AudioManager
{
    [CreateAssetMenu(menuName = "Small Hedge/Musics SO", fileName = "Musics SO")]
    public class MusicsSO : ScriptableObject
    {
        public AudioMixerGroup mixer;
        public MusicList[] musics;
    }
}
