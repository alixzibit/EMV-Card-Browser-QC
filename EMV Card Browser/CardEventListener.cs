using System.Windows.Controls;
using EMV_Card_Browser;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System;
using System.Windows.Input;

namespace EMV_Card_Browser
{
    public class CardEventListener
    {
        private PCSCReader _pcscReader;
        private Label _statusLabel;
        public ObservableCollection<Asn1NodeViewModel> _treeNodes; // Using the original naming
        private string _selectedReaderName;
        public string GetSelectedReaderName()


        {
            return _selectedReaderName;
        }
        public delegate void CardReadFinishedEventHandler();
        public event CardReadFinishedEventHandler CardReadFinished;


        private Action _readCardAction;
        public CardEventListener(Action readCardAction, Label statusLabel, ObservableCollection<Asn1NodeViewModel> treeNodes)
        {
            _readCardAction = readCardAction;
            _statusLabel = statusLabel ?? throw new ArgumentNullException(nameof(statusLabel));
            _treeNodes = treeNodes ?? throw new ArgumentNullException(nameof(treeNodes)); // Assigning the passed collection

            _pcscReader = new PCSCReader();

            var readers = _pcscReader.Readers;
            if (readers.Count() == 0)
            {
                _statusLabel.Content = "No reader detected. Please attach card reader and restart app.";
                return;
            }

            // Automatically select the first reader
            _selectedReaderName = readers.First();

            _pcscReader.CardInserted += PcscReader_CardInserted;
            _pcscReader.CardRemoved += PcscReader_CardRemoved;
        }



        private CardRecord tempRecord = new CardRecord();
        private void PcscReader_CardInserted(string reader, byte[] atr)

        {
            _statusLabel.Dispatcher.Invoke(() =>
            {
                _statusLabel.Content = "Card inserted.";
            });

            // Maximum attempts to read the card.
            int maxAttempts = 3;
            bool isReadSuccessful = false;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    _statusLabel.Dispatcher.Invoke(() =>
                    {
                        _statusLabel.Content = $"Card reading attempt {attempt} in progress...";
                    });
                    
                    _readCardAction();

                    _statusLabel.Dispatcher.Invoke(() =>
                    {
                        _statusLabel.Content = "Card reading finished.";

                        // If all data is read, push the tempRecord to the DataGrid
                        CardReadFinished?.Invoke(); // Trigger the event

                        // Always reset the tempRecord after it's pushed
                        tempRecord = new CardRecord();

                    });
                    isReadSuccessful = true;
                    break; // Exit the loop if reading is successful.

                }
                catch
                {
                    if (attempt < maxAttempts)
                    {
                        // If it's not the last attempt, wait for 2 seconds before retrying.
                        System.Threading.Thread.Sleep(2000);
                        continue;
                    }
                }
            }

            if (!isReadSuccessful)
            {
                _statusLabel.Dispatcher.Invoke(() =>
                {
                    _statusLabel.Content = "Card reading error.";
                });
            }
        }

        //private void PcscReader_CardInserted(string reader, byte[] atr)
        //{
        //    _statusLabel.Dispatcher.Invoke(() =>
        //    {
        //        _statusLabel.Content = "Card inserted.";
        //    });

        //    // Maximum attempts to read the card.
        //    int maxAttempts = 3;
        //    bool isReadSuccessful = false;

        //    for (int attempt = 1; attempt <= maxAttempts; attempt++)
        //    {
        //        try
        //        {
        //            _statusLabel.Dispatcher.Invoke(() =>
        //            {
        //                _statusLabel.Content = $"Card reading attempt {attempt} in progress...";
        //            });

        //            _readCardAction();

        //            _statusLabel.Dispatcher.Invoke(() =>
        //            {
        //                _statusLabel.Content = "Card reading finished.";

        //            });

        //            isReadSuccessful = true;
        //            break; // Exit the loop if reading is successful.
        //        }
        //        catch
        //        {
        //            if (attempt < maxAttempts)
        //            {
        //                // If it's not the last attempt, wait for 1 second before retrying.
        //                System.Threading.Thread.Sleep(2000);
        //                continue;
        //            }
        //        }
        //    }

        //    if (!isReadSuccessful)
        //    {
        //        _statusLabel.Dispatcher.Invoke(() =>
        //        {
        //            _statusLabel.Content = "Card reading error.";
        //        });
        //    }
        //}


        private void PcscReader_CardRemoved(string reader)
        {
            _statusLabel.Dispatcher.Invoke(() =>
            {
                _statusLabel.Content = "Card removed.";
            });
        }

        //private void ReadCard()
        //{
        //    _treeNodes.Clear(); // Using the original collection name

        //    string logPath = @"C:\EMV_CB_log\emvcard_log.txt";

        //    // Instead of fetching from ReaderComboBox, use the auto-selected reader
        //    string readerName = _selectedReaderName;
        //    // ... [rest of the method]
        //}

        public void Dispose()
        {
            _pcscReader.Dispose();
        }
    }
}