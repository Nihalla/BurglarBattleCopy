// NOTE(Zack): this has been changed to be a purely static class, as there
// is no reason that it should be a MonoBehaviour. If something is static,
// this means that it will exist for the entire runtime of the application,
// which means that we can remove the need for expensive calls such as [FindObjectOfType]
// and it also means the we won't get memory access bugs.
public static class GoldTransferToEnd
{
    public static int team1Gold;
    public static int team2Gold;
}
