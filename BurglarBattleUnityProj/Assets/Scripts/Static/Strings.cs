// Author: Zack Collins

// <summary>
// This class is a wrapper for pre-allocating strings, so that they can be accessed at runtime without additional allocations
// </summary>
public static class Strings
{
    private const int allocationCount = 1500;
    public static readonly string[] numbers = new string[allocationCount];
    public static readonly string[] tips = new string[] {
        "Watch out for goblins, they will steal your loot forever!",
    "Be careful of watchers, they’ll alert your presence to nearby guards.",
    "Try not to be sent to jail too often, as the more you’re caught the longer it takes to escape.",
    "Potions can be brewed in the cauldron on the second floor.",
    "Loot cannot be turned in until you reach the second floor, get moving!",
    "Controls can be viewed from the main menu.",
    "You can stun guards and other players, even the ones on your team!",
    "Once you unlock the vault, you won’t have much time left to drop off your treasure.",
    "If a guard is holding you, someone can stun them to free you.",
    "The number of lines on the picture is the key.",
    "You can only carry a maximum of 100 gold."};
    static Strings()
    {
        for (int i = 0; i < allocationCount; ++i)
        {
            numbers[i] = i.ToString("D2");
        }
    }
}
