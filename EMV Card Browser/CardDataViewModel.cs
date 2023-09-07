using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EMV_Card_Browser
{
    public class CardDataViewModel : INotifyPropertyChanged
    {
        private string cardholderName;
        private string cardType;
        private string pan;
        private string expiry;

        public string CardholderName
        {

            get => cardholderName;
            set
            {
                cardholderName = value;
                OnPropertyChanged(nameof(CardholderName));
                var logger = new Logger();
                logger.WriteLog($"CardholderName {cardholderName}");
            }
        }

        public string CardType
        {
            get => cardType;
            set
            {
                cardType = value;
                OnPropertyChanged(nameof(CardType));
                var logger = new Logger();
                logger.WriteLog($"CardType {CardType}");
            }
        }

        public string PAN
        {
            get => pan;
            set
            {
                pan = value;
                OnPropertyChanged(nameof(PAN));
                var logger = new Logger();
                logger.WriteLog($"PAN {PAN}");
            }
        }

        public string Expiry
        {
            get => expiry;
            set
            {
                expiry = value;
                OnPropertyChanged(nameof(Expiry));
                var logger = new Logger();
                logger.WriteLog($"Expiry {Expiry}");
            }
        }
        public ObservableCollection<CardRecord> CardRecords { get; } = new ObservableCollection<CardRecord>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
