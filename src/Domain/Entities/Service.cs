namespace Domain.Entities;

public class Service : Advertisement
{
    protected Service() { }

    public Service(string name, string description, string summary, User advertiser)
        : base(name, description, summary, advertiser)
    {
    }

    public void Update(string name, string description, string summary)
    {
        UpdateBase(name, description, summary);
    }
}
