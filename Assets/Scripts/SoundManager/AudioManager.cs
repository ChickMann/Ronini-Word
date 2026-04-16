//Author: Small Hedge Games
//Updated: 13/06/2024

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.AudioManager
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO soundsSO;
        [SerializeField] private MusicsSO musicsSO;
        public static AudioManager instance = null;
        
        [Header("Audio Sources")]
        [SerializeField] AudioSource musicSource; 
        [SerializeField] AudioSource sfxSource;

        private Coroutine _musicCoroutine;

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
            }
        }

        public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
        {
            SoundList soundList = instance.soundsSO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = instance.soundsSO.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume;
                source.Play();
            }
            else
            {
    
                instance.sfxSource.outputAudioMixerGroup = instance.soundsSO.mixer;
                instance.sfxSource.PlayOneShot(randomClip, volume * soundList.volume);
            }
        }

        public static void PlayMusic(MusicType music, AudioSource source = null, float volume = 1,
            float fadeDuration = 1.0f)
        {
        
            var targetSource = source ? source : instance.musicSource;

            if (targetSource == instance.musicSource)
            {
                if (instance._musicCoroutine != null)
                    instance.StopCoroutine(instance._musicCoroutine);

                instance._musicCoroutine = instance.StartCoroutine(
                    instance.PlayMusicRandomInList(music, targetSource, volume, fadeDuration, -1)
                );
            }
            else
            {
                instance.StartCoroutine(
                    instance.PlayMusicRandomInList(music, targetSource, volume, fadeDuration, -1)
                );
            }
        }

        private IEnumerator PlayMusicRandomInList(MusicType type, AudioSource source, float maxVolume, float fadeTime,
            int lastIndex)
        {
            // FADE OUT 
            if (source.isPlaying && source.volume > 0)
            {
                float startVol = source.volume;
                float t = 0;
                while (t < fadeTime)
                {
                    t += Time.unscaledDeltaTime;
                    source.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
                    yield return null;
                }

                source.volume = 0;
                source.Stop();
            }

            // CHỌN BÀI MỚI 
            int typeIndex = (int)type;
            if (typeIndex < 0 || typeIndex >= musicsSO.musics.Length) yield break;

            MusicList musicData = musicsSO.musics[typeIndex];
            AudioClip[] clips = musicData.musics;

            if (clips == null || clips.Length == 0) yield break;

            int nextIndex = 0;
            if (clips.Length > 1)
            {
                int attempts = 20; 
                do
                {
                    nextIndex = UnityEngine.Random.Range(0, clips.Length);
                    attempts--;
                } while (nextIndex == lastIndex && attempts > 0);
            }

            //  PLAY & FADE IN
            AudioClip nextClip = clips[nextIndex];
            float finalVolume = maxVolume * musicData.volume;

            source.clip = nextClip;
            source.outputAudioMixerGroup = instance.musicsSO.mixer; 
            source.Play();

            float timer = 0;
            while (timer < fadeTime)
            {
                timer += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(0f, finalVolume, timer / fadeTime);
                yield return null;
            }

            source.volume = finalVolume;

            //  CHỜ HẾT BÀI 
            float exitTime = nextClip.length - fadeTime;
            while (source.isPlaying && source.time < exitTime)
            {
                yield return null;
            }

            // ĐỆ QUY GỌI TIẾP
            instance._musicCoroutine = instance.StartCoroutine(
                instance.PlayMusicRandomInList(type, source, maxVolume, fadeTime, nextIndex)
            );
        }

        public void SetMuteMusicAndSound(bool isMute)
        {
            musicSource.mute = isMute;
            sfxSource.mute = isMute;
           
        }

        [ContextMenu(" Debug: Skip To End")]
        public void DebugSkipToEnd()
        {
            if (musicSource != null && musicSource.clip != null && musicSource.isPlaying)
            {
                float debugTime = musicSource.clip.length - 3.0f;
                if (debugTime > 0)
                {
                    musicSource.time = debugTime;
                    Debug.Log($"[SoundManager] ⏩ Skipped. Next random song (non-repeating) coming up!");
                }
            }
        }
    }

    [Serializable]
    public struct SoundList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioClip[] sounds;
    }
    
    [Serializable]
    public struct MusicList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioClip[] musics;
    }
}
