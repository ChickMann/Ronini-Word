//Author: Small Hedge Games
//Updated: 13/06/2024

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace SmallHedge.AudioManager
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO soundsSO;
        [SerializeField] private MusicsSO musicsSO;
        public static AudioManager instance = null;
        
        // [FIX 1] Tách biệt 2 AudioSource
        private AudioSource musicSource; 
        private AudioSource sfxSource;

        private Coroutine _musicCoroutine;

        private void Awake()
        {
            if (!instance)
            {
                instance = this;
                // Nguồn phát nhạc (Dùng Component có sẵn trên GameObject)
                musicSource = GetComponent<AudioSource>();
                
                // Tự động tạo thêm 1 nguồn phát SFX ẩn ngay lúc Runtime
                sfxSource = gameObject.AddComponent<AudioSource>();
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
                // [FIX 2] Dùng sfxSource độc lập, không đụng chạm đến nhạc nền
                instance.sfxSource.outputAudioMixerGroup = instance.soundsSO.mixer;
                instance.sfxSource.PlayOneShot(randomClip, volume * soundList.volume);
            }
        }

        public static void PlayMusic(MusicType music, AudioSource source = null, float volume = 1,
            float fadeDuration = 1.0f)
        {
            // [FIX 3] Mặc định dùng musicSource
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
            // --- BƯỚC 1: FADE OUT ---
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

            // --- BƯỚC 2: CHỌN BÀI MỚI ---
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

            // --- BƯỚC 3: PLAY & FADE IN ---
            AudioClip nextClip = clips[nextIndex];
            float finalVolume = maxVolume * musicData.volume;

            source.clip = nextClip;
            // [FIX 4] Trực tiếp gọi instance để code tường minh, tránh lỗi luồng ngầm của Coroutine
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

            // --- BƯỚC 4: CHỜ HẾT BÀI ---
            float exitTime = nextClip.length - fadeTime;
            while (source.isPlaying && source.time < exitTime)
            {
                yield return null;
            }

            // --- BƯỚC 5: ĐỆ QUY GỌI TIẾP ---
            instance._musicCoroutine = instance.StartCoroutine(
                instance.PlayMusicRandomInList(type, source, maxVolume, fadeTime, nextIndex)
            );
        }

        public void SetMuteMusicAndSound(bool isMute)
        {
            musicSource.mute = isMute;
            sfxSource.mute = isMute;
           
        }

        [ContextMenu("🐛 Debug: Skip To End")]
        public void DebugSkipToEnd()
        {
            // Sửa lại thành kiểm tra musicSource
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