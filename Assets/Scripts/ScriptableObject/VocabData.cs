using System;
using System.Collections.Generic;
using UnityEngine;

public enum StateVobcab
{
    none,
    notPerfect,
    Perfect
}
[CreateAssetMenu(fileName = "VocabData", menuName = "Scriptable Objects/VocabData")]
public class VocabData : ScriptableObject
{
    public String Meaning;
    public String Answer;

    public StateVobcab StateVobcab;

    public VocabData()
    {
        Meaning = "";
        Answer = "";
        StateVobcab = StateVobcab.none;
    }
}
