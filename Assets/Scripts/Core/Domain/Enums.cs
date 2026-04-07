namespace DessertKingdom.Core.Domain
{
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public enum StatType
    {
        HP,
        Charm,
        Intelligence,
        Art,
        Morality,
        Stress
    }

    public enum ActivityType
    {
        Lesson,
        PartTime,
        Rest,
        Outing,
        Special
    }

    public enum EventType
    {
        Prologue,
        Fixed,
        Random,
        NPC,
        Special,
        Hidden
    }

    public enum GamePhase
    {
        NameSetting,        // 이름 설정
        Prologue,            // 프로로그
        ScheduleSelection,     // 스케줄 선택
        ActivityProcessing, // 활동 진행
        EventCheck,          // 이벤트 체크
        EventProcessing,     // 이벤트 진행
        TurnEnd,             // 턴 종료
        Ending               // 엔딩
    }

    public enum StatGrade
    {
        F,  // 0~99
        E,  // 100~199
        D,  // 200~299
        C,  // 300~399
        B,  // 400~499
        A,  // 500~599
        S,  // 600~699
        SS  // 700~999
    }

    public enum PersonalityType
    {
        Energetic,      // 체력 우세
        Intellectual,   // 지능 우세
        Artistic,       // 예술 우세
        Charming,       // 매력 우세
        Virtuous,       // 도덕성 우세
        Balanced        // 균형잡힌
    }

    public enum RelationshipLevel
    {
        Stranger,       // 0~20
        Friend,         // 21~40
        CloseFriend,    // 41~60
        Trusted,        // 61~80
        Bonded          // 81~100
    }

    public enum StressLevel
    {
        Normal,     // 0~30
        Good,       // 31~50
        Tired,      // 51~70
        Overworked, // 71~90
        Critical,   // 91~100
        Dangerous   // 100+
    }

    public enum MoralityLevel
    {
        Evil,       // 0~199
        Normal,     // 200~399
        Good        // 400+
    }
}
