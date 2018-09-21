namespace CheckoutBot.Interfaces
{
    public interface IHasSession
    {
        void Start(bool hidden = false);
        void Stop();
    }
}