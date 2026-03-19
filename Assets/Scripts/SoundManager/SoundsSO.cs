//Author: Small Hedge Games
//Updated: 13/06/2024

using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.AudioManager
{
    [CreateAssetMenu(menuName = "Small Hedge/Sounds SO", fileName = "Sounds SO")]
    public class SoundsSO : ScriptableObject
    {
        public AudioMixerGroup mixer;
        public SoundList[] sounds;
    }
}