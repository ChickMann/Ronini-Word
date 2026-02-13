#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SmallHedge.AudioManager
{
    [CustomEditor(typeof(MusicsSO))]
    public class MusicsSOEditor : Editor
    {
        private void OnEnable()
        {
            ref MusicList[] musicList = ref ((MusicsSO)target).musics;

            if (musicList == null)
                return;

            string[]names = Enum.GetNames(typeof(MusicType));
            bool differentSize = names.Length != musicList.Length;

            Dictionary<string, MusicList> muiscs = new();

            if (differentSize)
            {
                for (int i = 0; i < musicList.Length; ++i)
                {
                    muiscs.Add(musicList[i].name, musicList[i]);
                }
            }

            Array.Resize(ref musicList, names.Length);
            for (int i = 0; i < musicList.Length; i++)
            {
                string currentName = names[i];
                musicList[i].name = currentName;
                if (musicList[i].volume == 0) musicList[i].volume = 1;

                if (differentSize)
                {
                    if (muiscs.ContainsKey(currentName))
                    {
                        MusicList current = muiscs[currentName];
                        UpdateElement(ref musicList[i], current.volume, current.musics);
                    }
                    else
                        UpdateElement(ref musicList[i], 1, new AudioClip[0]);

                    static void UpdateElement(ref MusicList element, float volume, AudioClip[] musics)
                    {
                        element.volume = volume;
                        element.musics = musics;

                    }
                }
            }
        }
    }
}
#endif
