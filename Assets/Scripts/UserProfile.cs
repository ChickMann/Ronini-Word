using System.Collections.Generic;

[System.Serializable]
public class UserProfile
{
    public string UserName;
    public int TotalScore;
    
    public Dictionary<string, int> VocabProgress = new Dictionary<string, int>();

    public StateVocab GetState(int id)
    {
        string key = id.ToString();
        if (VocabProgress.TryGetValue(key, out int stateVal))
        {
            return (StateVocab)stateVal;
        }
        return StateVocab.None;
    }
}
