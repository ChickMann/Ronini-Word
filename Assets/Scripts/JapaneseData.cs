using System.Collections.Generic;

public static class JapaneseData
{
    // 1. Kho dữ liệu gốc (Private - để nội bộ class quản lý)
    // Sử dụng HashSet để đảm bảo không trùng lặp và tra cứu siêu tốc
    private static readonly HashSet<string> _basicHiragana = new HashSet<string>
    {
        // --- 5 Nguyên âm (A I U E O) ---
        "あ", "い", "う", "え", "お",

        // --- Hàng K (Ka Ki Ku Ke Ko) ---
        "か", "き", "く", "け", "こ",

        // --- Hàng S (Sa Shi Su Se So) ---
        "さ", "し", "す", "せ", "そ",

        // --- Hàng T (Ta Chi Tsu Te To) ---
        "た", "ち", "つ", "て", "と",

        // --- Hàng N (Na Ni Nu Ne No) ---
        "な", "に", "ぬ", "ね", "の",

        // --- Hàng H (Ha Hi Fu He Ho) ---
        "は", "ひ", "ふ", "へ", "ほ",

        // --- Hàng M (Ma Mi Mu Me Mo) ---
        "ま", "み", "む", "め", "も",

        // --- Hàng Y (Ya Yu Yo) ---
        "や", "ゆ", "よ",

        // --- Hàng R (Ra Ri Ru Re Ro) ---
        "ら", "り", "る", "れ", "ろ",

        // --- Hàng W (Wa Wo) ---
        "わ", "を",

        // --- Ký tự mũi (N) ---
        "ん",

        // ==========================================
        // --- BỔ SUNG: DAKUTEN (Biến âm đục) ---
        // ==========================================
    
        // --- Hàng G (Ga Gi Gu Ge Go) ---
        "が", "ぎ", "ぐ", "げ", "ご",

        // --- Hàng Z (Za Ji Zu Ze Zo) ---
        "ざ", "じ", "ず", "ぜ", "ぞ",

        // --- Hàng D (Da Ji Zu De Do) ---
        "だ", "ぢ", "づ", "で", "ど",

        // --- Hàng B (Ba Bi Bu Be Bo) ---
        "ば", "び", "ぶ", "べ", "ぼ",

        // ==========================================
        // --- BỔ SUNG: HANDAKUTEN (Biến âm bán đục) ---
        // ==========================================

        // --- Hàng P (Pa Pi Pu Pe Po) ---
        "ぱ", "ぴ", "ぷ", "ぺ", "ぽ"
    };
    public static IReadOnlyCollection<string> BasicHiragana => _basicHiragana;
    
    private static readonly HashSet<string> _basicKatakana = new HashSet<string>
    {
        // --- 5 Nguyên âm (A I U E O) ---
        "ア", "イ", "ウ", "エ", "オ",

        // --- Hàng K (Ka Ki Ku Ke Ko) ---
        "カ", "キ", "ク", "ケ", "コ",

        // --- Hàng S (Sa Shi Su Se So) ---
        "サ", "シ", "ス", "セ", "ソ",

        // --- Hàng T (Ta Chi Tsu Te To) ---
        "タ", "チ", "ツ", "テ", "ト",

        // --- Hàng N (Na Ni Nu Ne No) ---
        "ナ", "ニ", "ヌ", "ネ", "ノ",

        // --- Hàng H (Ha Hi Fu He Ho) ---
        "ハ", "ヒ", "フ", "ヘ", "ホ",

        // --- Hàng M (Ma Mi Mu Me Mo) ---
        "マ", "ミ", "ム", "メ", "モ",

        // --- Hàng Y (Ya Yu Yo) ---
        "ヤ", "ユ", "ヨ",

        // --- Hàng R (Ra Ri Ru Re Ro) ---
        "ラ", "リ", "ル", "レ", "ロ",

        // --- Hàng W (Wa Wo) ---
        "ワ", "ヲ",

        // --- Ký tự mũi (N) ---
        "ン",

        // ==========================================
        // --- BỔ SUNG: DAKUTEN (Biến âm đục - Katakana) ---
        // ==========================================

        // --- Hàng G (Ga Gi Gu Ge Go) ---
        "ガ", "ギ", "グ", "ゲ", "ゴ",

        // --- Hàng Z (Za Ji Zu Ze Zo) ---
        "ザ", "ジ", "ズ", "ゼ", "ゾ",

        // --- Hàng D (Da Ji Zu De Do) ---
        "ダ", "ヂ", "ヅ", "デ", "ド",

        // --- Hàng B (Ba Bi Bu Be Bo) ---
        "バ", "ビ", "ブ", "ベ", "ボ",

        // ==========================================
        // --- BỔ SUNG: HANDAKUTEN (Biến âm bán đục - Katakana) ---
        // ==========================================

        // --- Hàng P (Pa Pi Pu Pe Po) ---
        "パ", "ピ", "プ", "ペ", "ポ"
    };

    // Public Property để truy cập an toàn (Read-only)
    public static IReadOnlyCollection<string> BasicKatakana => _basicKatakana;
}