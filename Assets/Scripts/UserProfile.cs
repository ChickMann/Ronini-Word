using System.Collections.Generic;

[System.Serializable]
public class UserProfile
{
    public string UserName;
    public int TotalScore;
    
    // Key: VocabID (string), Value: State (int)
    // Dùng Dictionary để tìm kiếm cực nhanh O(1)
    public Dictionary<string, int> VocabProgress = new Dictionary<string, int>();

    // Hàm tiện ích để lấy trạng thái
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