using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TipManager
{
    private static List<string> basicTips = new List<string>
    {
        "You can modify the setting mid-game from the pause menu, but remember some settings only apply after a level restart.",
        "You can drag less to do a lower power jump it can be handy sometimes.",
        "A precise 89-degree high-power jump can get you up some super tall platforms.",
        "A simple tap after a jump will try to repeat your last jump.",
        "You can click on hints to confirm which paths are easier, but they tend to be pretty slow.",
        "Use low-power jumps to micro-adjust your character.",
        "Click on 'Local' to see the record of your own scores and track how you’ve improved.",
        "You can choose to pull or drag the arrow from the settings menu.",
        "You don’t have to immediately swipe you can hold your jumps and adjust until ready.",
        "You don’t have to click on the character you can start pulling or dragging from anywhere on screen.",
        "You are a champ, but can your friends clear the levels you can?",
        "You can change your profile name from the profile icon on the main menu.",
        "Pink slime is waiting for your rescue never give up!",
        "The witch wants to make a beauty potion out of pink slime rescue her before that happens!"
    };
    private static List<string> badTips = new List<string>
    {
        "Git gud.",
        "Skill issue.",
        "***"
    };
    static int tipCount = 0;
    public static string GetRandomTip()
    {
        tipCount++;
        List<string> tipsPool = new List<string>();
        tipsPool.AddRange(basicTips);
        if (tipCount > 30)
            tipsPool.AddRange(badTips);



        int randomIndex = UnityEngine.Random.Range(0, tipsPool.Count);
        return tipsPool[randomIndex];
    }
}
