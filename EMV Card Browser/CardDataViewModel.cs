using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            }
        }

        public string CardType
        {
            get => cardType;
            set
            {
                cardType = value;
                OnPropertyChanged(nameof(CardType));
            }
        }

        public string PAN
        {
            get => pan;
            set
            {
                pan = value;
                OnPropertyChanged(nameof(PAN));
            }
        }

        public string Expiry
        {
            get => expiry;
            set
            {
                expiry = value;
                OnPropertyChanged(nameof(Expiry));
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
