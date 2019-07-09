namespace Aktiv.RtAdmin
{
    public class PinChangeOperationParams : BaseTokenOperationParams
    {
        public string OldPin
        {
            get => LoginPin;
            set => LoginPin = value;
        }

        public string NewPin { get; set; }
    }
}