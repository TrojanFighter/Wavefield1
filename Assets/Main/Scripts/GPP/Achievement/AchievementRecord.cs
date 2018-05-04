using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName="AchievementRecord")]
public class AchievementRecord : ScriptableObject
{
    public List<AchievementBase> m_Achievements;

}

public class AchievementBase
{
    public string AchievementName;
    public bool BUnlocked;
}

public class TimesAchievement : AchievementBase
{
    public int TotalTimeInNeed;
    public int CurrentTimes;
}

public class TypesAchievement : AchievementBase
{
    public List<int> AchievementNeededToUnlock;
}

public enum AchievementTypes
{
    A,
    B,
    C,
    D,
    E
}
