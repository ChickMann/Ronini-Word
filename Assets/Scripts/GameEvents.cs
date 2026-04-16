using System;
using UnityEngine;

public static class GameEvents
{

    
    /// <summary>
    /// khi kết thúc màn chơi.
    /// Payload (bool): True = Thắng, False = Thua.
    /// </summary>
    public static Action<bool> OnLevelComplete;

    
    /// <summary>
    /// Khi người chơi đã điền đủ các ô -> Gửi đáp án về CombatManager check.
    /// Payload (string): Chuỗi ký tự người chơi đã ghép (VD: "neko").
    /// </summary>
    public static Action<bool> OnSubmitAnswer;

    // 4. FEEDBACK (Hiệu ứng Âm thanh/Hình ảnh)

    /// <summary>
    /// Trả lời ĐÚNG (CombatManager báo về).
    /// Dùng để phát âm thanh , hiện hiệu ứng xanh, chém quái.
    /// Payload (bool): True nếu là ký tự cuối cùng của từ (Finish Blow).
    /// </summary>
    public static Action OnCharCorrect;
    
    
    /// <summary>
    /// Trả lời SAI 
    /// Dùng để rung màn hình, hiện màu đỏ, trừ máu.
    /// </summary>
    public static Action OnCharWrong;
    
}
