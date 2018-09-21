namespace CheckoutBot.Interfaces
{
    public interface IBrowserSession
    {
        void Start(bool hidden = false, string proxy = null);
        void Stop();
    }
}