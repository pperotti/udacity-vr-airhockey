class MatchStats
{

    public static MatchStats instance = new MatchStats();

    private int host;
    private int client;

    public int Host
    {
        get
        {
            return host;
        }
    }

    public int Client
    {
        get
        {
            return client;
        }
    }

    internal void scoreInHostGoalLine()
    {
        client++;
    }

    internal void scoreInClientGoalLine()
    {
        host++;
    }

}
