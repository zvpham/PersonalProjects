[System.Serializable]
public class StatusData
{
    public Status status;
    public int duration;
    public bool noDuration;

    public StatusData(Status status, int duration, bool noDuration)
    {
        this.status = status;
        this.duration = duration;
        this.noDuration = noDuration;
    }
}
