using System;
using System.Linq;
using System.Windows;
using PCSC;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Security.Principal;
using System.Windows.Input;
using PdfSharp.Pdf.Content;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Windows.Automation;


namespace EMV_Card_Browser
{

    public partial class MainWindow : Window
    {

        private PCSCReader _cardReader;
        public ObservableCollection<Asn1NodeViewModel> _treeNodes = new ObservableCollection<Asn1NodeViewModel>();
        private Asn1NodeViewModel rootNode;
        private CardEventListener _cardEventListener;
        private CardDataViewModel viewModel = new CardDataViewModel();
        private PCSCReader connectReader;
        private string _selectedReaderName;
        private int exceptionCounter = 0;
        public bool IsValidCardRead = false;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Initialize rootNode
            rootNode = new Asn1NodeViewModel("Root");

            // Bind CardDataTree.ItemsSource to rootNode.Children
            CardDataTree.ItemsSource = rootNode.Children;

            // Only create one instance of CardEventListener
            _cardEventListener = new CardEventListener(ReadCard, statusLabel, rootNode.Children);

            // Now, you just have to subscribe to the event of the _cardEventListener
            _cardEventListener.CardReadFinished += CheckAndAddToDataGrid;
            
            string currentUsername = WindowsIdentity.GetCurrent().Name.Split('\\').Last();
            usernameLabel.Content = "Windows User: " + currentUsername;
            this.Closing += MainWindow_Closing;
        }

        //public MainWindow()
        //{
        //    InitializeComponent();
        //    DataContext = viewModel;
        //    viewModel.PropertyChanged += ViewModel_PropertyChanged;
        //    Initialize rootNode
        //    rootNode = new Asn1NodeViewModel("Root");
        //    Bind CardDataTree.ItemsSource to rootNode.Children
        //    CardDataTree.ItemsSource = rootNode.Children;
        //    _cardEventListener = new CardEventListener(ReadCard, statusLabel, rootNode.Children);
        //    CardEventListener cardEventListener = new CardEventListener();
        //    cardEventListener.CardReadFinished += CheckAndAddToDataGrid;

        //}

        private void Reader_CardNotPresent(object sender, EventArgs e)
        {
            statusLabel.Content = "Card not present.";
            MessageBox.Show("Please insert a card into the reader.", "Card Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
       
        private bool isCardInfoBeingUpdated = false;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!isCardInfoBeingUpdated)
            {
                isCardInfoBeingUpdated = true;

                if (e.PropertyName == "CardholderName" || e.PropertyName == "CardType"
                                                       || e.PropertyName == "PAN" || e.PropertyName == "Expiry")
                {
                    AddToDataGrid();
                    isCardInfoBeingUpdated = false;
                }
            }
        }



        private async void OnConnectCard(object sender, RoutedEventArgs e) // make the method async
        {
            connectReader = new PCSCReader();

            var readers = connectReader.Readers;
            if (readers.Count() == 0)
            {
                statusLabel.Content = "No reader detected. Please attach card reader and connect.";
                return;
            }

            // Automatically select the first reader
            _selectedReaderName = readers.First();
            statusLabel.Content = "Reader Connected";
            connectReader.CardNotPresent += Reader_CardNotPresent;

            // Initial check for card presence
            if (!connectReader.IsCardPresent())
            {
                // Wait for a short delay
                await Task.Delay(500); // 500 milliseconds (0.5 seconds)

                // Re-check for card presence
                if (!connectReader.IsCardPresent())
                {
                    statusLabel.Content = "Reader Connected, but no card detected. Please insert a card.";
                    Mouse.OverrideCursor = null;
                    return;
                }
            }

            // If you've reached here, a card is present. 
            ReadCard();
            CheckAndAddToDataGrid();
            Mouse.OverrideCursor = null;
        }



        //private void OnConnectCard(object sender, RoutedEventArgs e)
        //{
        //    DisconnectCard();
        //    // Check if the existing reader instance is not null, and if so, dispose of it
        //    if (connectReader != null)
        //    {
        //        connectReader.Dispose();
        //        connectReader = null; // Optional but it's good to nullify it after disposing
        //    }

        //    connectReader = new PCSCReader();

        //    var readers = connectReader.Readers;
        //    if (readers.Count() == 0)
        //    {
        //        statusLabel.Content = "No reader detected. Please attach card reader and connect.";
        //        return;
        //    }

        //    // Automatically select the first reader
        //    _selectedReaderName = readers.First();

        //    // Check if a card is present before proceeding
        //    if (!connectReader.IsCardPresent())
        //    {

        //        statusLabel.Content = "Reader Connected, but no card detected. Please insert a card.";

        //        return;
        //    }

        //    statusLabel.Content = "Reader Connected, reading card...";
        //    ReadCard();
        //    CheckAndAddToDataGrid();

        //}

        private void AddToDataGrid()
        {
            string cardType = DeduceCardTypeFromPAN(viewModel.PAN);
            // ... other code

            CardRecord newRecord = new CardRecord
            {
                CardholderName = viewModel.CardholderName,
                CardType = cardType,
                CardNumber = viewModel.PAN,
                ExpiryDate = viewModel.Expiry,
                Timestamp = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
            };

            var existingRecord = viewModel.CardRecords.FirstOrDefault(record =>
                record.CardNumber == newRecord.CardNumber &&
                record.ExpiryDate == newRecord.ExpiryDate &&
                record.CardholderName == newRecord.CardholderName &&
                record.CardType == newRecord.CardType
            );

            if (existingRecord == null)
            {
                viewModel.CardRecords.Add(newRecord);

            }
        }

        //private void AddToDataGrid()
        //{
        //    string cardType = DeduceCardTypeFromPAN(viewModel.PAN);
        //    // ... other code

        //    CardRecord newRecord = new CardRecord
        //    {
        //        CardholderName = viewModel.CardholderName,
        //        CardType = cardType,
        //        CardNumber = viewModel.PAN,
        //        ExpiryDate = viewModel.Expiry,
        //        Timestamp = DateTime.Now
        //    };

        //    viewModel.CardRecords.Add(newRecord);

        //}
        private string DeduceCardTypeFromPAN(string pan)
        {
            if (!string.IsNullOrEmpty(pan))
            {
                char firstDigit = pan[0];

                switch (firstDigit)
                {
                    case '4':
                        return "Visa";
                    case '5':
                        return "MasterCard";
                        // ... You can add more cases for other card types if needed.
                }
            }

            return "Unknown"; // default
        }

        public string MaskPAN(string pan)
        {
            const int startLength = 6;  // Number of digits to show from start
            const int endLength = 4;    // Number of digits to show from end

            if (string.IsNullOrEmpty(pan) || pan.Length < startLength + endLength)
            {
                return pan; // return as is, because the PAN is not in the expected format
            }

            string firstDigits = pan.Substring(0, startLength);
            string lastDigits = pan.Substring(pan.Length - endLength, endLength);
            int maskLength = pan.Length - (startLength + endLength);

            string maskedPart = new string('X', maskLength); // Create a string of 'X' with length of maskLength

            return $"{firstDigits}{maskedPart}{lastDigits}";
        }

        private void InitializeCardReader()
        {
            if (_cardReader == null)
            {
                _cardReader = new PCSCReader();
            }
        }

        private void ConnectCard(string readerName)
        {
            try
            {
                // Instantiate PCSCReader
                _cardReader = new PCSCReader();

                // Connect to the card
                bool success = _cardReader.Connect(readerName);

                if (!success)
                {
                    throw new Exception("Could not connect to card.");
                }
                else
                {
                    // Connection is successful, write ATR to RibbonTextBox
                    var atr = BitConverter.ToString(_cardReader.ATR).Replace("-", " "); // Converts byte[] to string
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        statusLabel.Content = $"ATR: {atr}";
                    });
                }
            }
            catch (Exception ex)
            {
                // Display the exception in a message box
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DisconnectCard()
        {
            if (_cardReader != null)
            {
                try
                {
                    _cardReader.Disconnect();
                    _cardReader.Dispose();
                    _cardReader = null;

                    // Clear the ATR value from the statusLabel
                    statusLabel.Content = "";
                }
                catch (Exception ex)
                {
                    // Display the exception in a message box
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ConnectToCard(string readerName)
        {
            var logger = new Logger();
            bool success = _cardReader.Connect(readerName);
           logger.WriteLog(success ? "Successfully connected to card reader." : "Failed to connect to card reader.");
            return success;
        }

        //public bool IsValidCardRead;
        internal void ReadCard()
        {
          
            // Use Dispatcher to ensure UI operations are executed on the main thread
            Dispatcher.Invoke(() =>
            {
                var logger = new Logger();
                Mouse.OverrideCursor = Cursors.Wait;
                var rootNodeCollection = (ObservableCollection<Asn1NodeViewModel>)CardDataTree.ItemsSource;
                rootNodeCollection.Clear();

                InitializeCardReader();

                // If you are auto-selecting the reader in CardEventListener
                string readerName = _cardEventListener.GetSelectedReaderName();
                // assuming _selectedReaderName is the name of the selected reader, and is accessible.
                if (string.IsNullOrEmpty(readerName))
                {
                    MessageBox.Show("No reader selected.");
                    return;
                }

                if (!ConnectToCard(readerName))
                {
                    MessageBox.Show("Failed to connect to the card.");
                    statusLabel.Content = "Please insert card";

                    return;
                }
                try
                {
                    byte[] pse = Encoding.ASCII.GetBytes("1PAY.SYS.DDF01");
                   logger.WriteLog($"Sending APDU command to select the PSE: {BitConverter.ToString(pse)}");

                    APDUCommand apdu = new APDUCommand(0x00, 0xA4, 0x04, 0x00, pse, (byte)pse.Length);
                    APDUResponse response = _cardReader.Transmit(apdu);
                   logger.WriteLog($"Received response: SW1 = {response.SW1}, SW2 = {response.SW2}");
                    // Check if the response is successful and contains ASN.1 data.
                    if (response.SW1 == 0x90 && response.Data != null && response.Data.Length > 0)
                    {
                        ASN1 asn1Response = new ASN1(response.Data);
                        AddRecordNodes(asn1Response, rootNode);
                    }
                    else
                    {
                        AddResponseNodes(response, rootNode);  // This will add nodes only if there's any non-status data.
                    }

                    if (response.SW1 != 0x90)
                    {
                        AIDSelection aidSelector = new AIDSelection(_cardReader);
                        var aidSelectionResult = aidSelector.SelectKnownAID();
                        response = aidSelectionResult.Response;
                        // Check if the response is successful and contains ASN.1 data.
                        if (response.SW1 == 0x90 && response.Data != null && response.Data.Length > 0)
                        {
                            ASN1 asn1Response = new ASN1(response.Data);
                            AddRecordNodes(asn1Response, rootNode);
                        }
                        else
                        {
                            AddResponseNodes(response, rootNode);  // This will add nodes only if there's any non-status data.
                        }
                        logger.WriteLog($"Selected AID: {BitConverter.ToString(aidSelectionResult.AID)}");

                        if (response == null)
                        {
                            IsValidCardRead = false;
                            statusLabel.Content = "Card not personalized or Failed to select a known AID";
                            MessageBox.Show("Card not personalized or Failed to select a known AID.");
                            return;
                        }

                    }

                    ProcessingOptions processingOptions = new ProcessingOptions(_cardReader);
                    response = processingOptions.GetProcessingOptions();
                    var readRecords = processingOptions.ReadRecords;  // Access the list of read records
                                                                      // Check if the response is successful and contains ASN.1 data.
                    if (response.SW1 == 0x90 && response.Data != null && response.Data.Length > 0)
                    {
                        ASN1 asn1Response = new ASN1(response.Data);
                        AddRecordNodes(asn1Response, rootNode);
                    }
                    else
                    {
                        AddResponseNodes(response, rootNode);  // This will add nodes only if there's any non-status data.
                    }
                    logger.WriteLog($"Processing options response: SW1 = {response.SW1}, SW2 = {response.SW2}");

                    if (!processingOptions.IsGPOResponseSuccessful(response))
                    {
                        IsValidCardRead = false;
                        MessageBox.Show("Failed to get processing options.");
                        return;
                    }

                    RecordReader recordReader = new RecordReader(_cardReader);
                    List<APDUResponse> records = recordReader.ReadAllRecords();

                   logger.WriteLog($"Number of records retrieved: {records.Count}");

                    if (records.Count == 0)
                    {
                        IsValidCardRead = false;
                        MessageBox.Show("No records retrieved.");
                        return;
                    }
                    foreach (var record in records)
                    {
                        ASN1 asn1Response = new ASN1(record.Data);
                        AddRecordNodes(asn1Response, rootNode);

                    }
                    Mouse.OverrideCursor = null;
                    IsValidCardRead = true;
                }

                catch (Exception ex)
                {
                    IsValidCardRead = false;
                    exceptionCounter++;
                    logger.WriteLog($"CARD READ ERROR: {ex.Message}");
                    Mouse.OverrideCursor = null;

                    if (exceptionCounter <= 2)  // Adjust the threshold as necessary
                    {
                       
                        // Prompt the user to retry
                        MessageBoxResult result = MessageBox.Show("Card reading error occurred. Would you like to retry?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            ReadCard();
                        }
                        else
                        {
                            statusLabel.Content = "Card reading error";
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "If you are trying to read the same card and the error persists, then the card has NO or INCOMPLETE data. Please insert a personalized card.", "Persistent Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        statusLabel.Content = "Card reading error";
                        exceptionCounter = 0; // Reset the counter after showing the persistent error message
                        return;
                    }
                }

            });
        }


        private string BcdToString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        private string HexStringToAscii(string hex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 2)
            {
                string hexChar = hex.Substring(i, 2);
                sb.Append((char)Convert.ToInt32(hexChar, 16));
            }
            return sb.ToString();
        }

        private HashSet<string> desiredTags = new HashSet<string> { "50", "5F20", "5F24", "5A" };

        private Dictionary<string, string> extractedTagValues = new Dictionary<string, string>();


        private void AddRecordNodes(ASN1 asn, Asn1NodeViewModel parentNode)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in asn.Tag)
            {
                sb.AppendFormat("{0:X2}", b);
            }

            string tagHex = sb.ToString().ToUpper();  // Convert tag to uppercase hex string

            Asn1NodeViewModel node = new Asn1NodeViewModel(tagHex)
            {
                Asn1Data = asn.Value,
                Value = asn.Value
            };

            parentNode.Children.Add(node);

            // If the current node tag matches one of the desired tags, process its value
            if (desiredTags.Contains(tagHex))
            {
                if (tagHex == "50" || tagHex == "5F20") // Convert value to hex first, then to ASCII
                {
                    StringBuilder valueSb = new StringBuilder();
                    foreach (byte b in asn.Value)
                    {
                        valueSb.AppendFormat("{0:X2}", b);
                    }
                    string hexValue = valueSb.ToString();
                    extractedTagValues[tagHex] = HexStringToAscii(hexValue);
                }
                else if (tagHex == "5A") // Extract BCD value for PAN
                {
                    extractedTagValues[tagHex] = BcdToString(asn.Value);
                }

                else if (tagHex == "5F24") // Extract BCD value and format for date
                {
                    string rawDate = BcdToString(asn.Value);
                    string formattedDate = "";

                    if (rawDate.Length == 4) // For "YYMM" format
                    {
                        formattedDate = rawDate.Substring(2, 2) + "/" + rawDate.Substring(0, 2); // Convert from "YYMM" to "MM/YY"
                    }
                    else if (rawDate.Length == 6) // For "YYMMDD" format
                    {
                        formattedDate = rawDate.Substring(2, 2) + "/" + rawDate.Substring(0, 2); // Convert from "YYMMDD" to "MM/YY" (ignoring the day part)
                    }
                    else
                    {
                        // Handle incorrect date length
                        formattedDate = "Invalid Date";
                    }

                    extractedTagValues[tagHex] = formattedDate;
                }
                if (extractedTagValues.ContainsKey("50"))
                {
                    string cardType = extractedTagValues["50"].ToLower();

                    // Create a new bitmap image.
                    BitmapImage bitmap = new BitmapImage();

                    // Determine which image to set based on the card type.
                    if (cardType.Contains("visa"))
                    {
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri("visa.png", UriKind.Relative);
                        bitmap.EndInit();
                    }
                    else if (cardType.Contains("mastercard"))
                    {
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri("mastercard.png", UriKind.Relative);
                        bitmap.EndInit();
                    }
                    else
                    {
                        // You can set a default image or do nothing if it's neither Visa nor MasterCard.
                    }

                    // Set the image source.
                    CardType.Source = bitmap;
                }
            }
            UpdateCardholderNameUI();
            UpdatePANUI();
            UpdateExpiryUI();

            if (asn.Count > 0)
            {
                foreach (ASN1 a in asn)
                {
                    AddRecordNodes(a, node);
                }
            }
        }

        private void AddResponseNodes(APDUResponse response, Asn1NodeViewModel parentNode)
        {
            if (response == null || response.Data == null || response.Data.Length == 0)
                return;

            // Create a node for the response SW values
            Asn1NodeViewModel node = new Asn1NodeViewModel($"SW1 = {response.SW1}, SW2 = {response.SW2}")
            {
                Asn1Data = response.Data,  // Assign the raw response data to the new property
                Value = response.Data      // Directly assign the raw data to the Value property
            };

            parentNode.Children.Add(node);
        }
        private int _recordCount = 0;

        private void CheckAndAddToDataGrid()
        {
            var logger = new Logger();
            logger.WriteLog($" Entering Card data grid method");
            logger.WriteLog($"tempRecord.CardholderName is '{tempRecord.CardholderName}'");
            logger.WriteLog($"tempRecord.CardNumber is '{tempRecord.CardNumber}'");
            logger.WriteLog($"tempRecord.ExpiryDate is '{tempRecord.ExpiryDate}'");

            if (!string.IsNullOrWhiteSpace(tempRecord.CardholderName) &&
                !string.IsNullOrWhiteSpace(tempRecord.CardNumber) &&
                !string.IsNullOrWhiteSpace(tempRecord.ExpiryDate))
            {
                var existingRecord = viewModel.CardRecords.FirstOrDefault(record =>
                    record.CardNumber == tempRecord.CardNumber &&
                    record.ExpiryDate == tempRecord.ExpiryDate &&
                    record.CardholderName == tempRecord.CardholderName
                );
                if (IsValidCardRead)
                {
                    if (existingRecord != null)
                    {
                        logger.WriteLog(
                            $"Found existing record: {existingRecord.CardNumber}, {existingRecord.ExpiryDate}, {existingRecord.CardholderName}");

                        // Notify user about the duplicate record
                        MessageBox.Show(
                            "The card data read is already present in the table. If you are certain that you have inserted a different card, then you have a DUPLICATE card.",
                            "Duplicate Card Detected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        tempRecord.CardType = DeduceCardTypeFromPAN(tempRecord.CardNumber);
                        tempRecord.Timestamp = DateTime.Now;

                        // Increment the record count and set the Srno for tempRecord.
                        _recordCount++;
                        tempRecord.Srno = _recordCount.ToString();

                        viewModel.CardRecords.Add(tempRecord);
                        logger.WriteLog(
                            $"Card data grid records: {tempRecord.CardNumber}, {tempRecord.ExpiryDate}, {tempRecord.CardholderName}, {tempRecord.Timestamp}");

                        // Update the reccounter label with the current record count.
                        Reccounter.Content = $"{_recordCount}";
                        exceptionCounter = 0;
                        statusLabel.Content = "Card read successfully";
                    }
                }
            }
        }


        private CardRecord tempRecord = new CardRecord();
        private void UpdateCardholderNameUI()
        {
            // Reset the temporary record at the start
            tempRecord = new CardRecord();

            if (extractedTagValues.ContainsKey("5F20"))
            {
                CardholderName.Content = extractedTagValues["5F20"];
                tempRecord.CardholderName = extractedTagValues["5F20"];
            }
            else
            {
                CardholderName.Content = "Error";
            }

            //CheckAndAddToDataGrid();
        }

        // Do similar resets for the other update methods too

        private void UpdatePANUI()
        {
            if (extractedTagValues.ContainsKey("5A"))
            {
                string actualPAN = extractedTagValues["5A"];
                PAN.Content = MaskPAN(actualPAN);
                tempRecord.CardNumber = MaskPAN(actualPAN);
            }
            else
            {
                PAN.Content = "Error"; // Or any other default value indicating no data was found.
            }

        }
        private void UpdateExpiryUI()
        {
            if (extractedTagValues.ContainsKey("5F24"))
            {
                Expiry.Content = extractedTagValues["5F24"];
                tempRecord.ExpiryDate = extractedTagValues["5F24"];
            }
            else
            {
                Expiry.Content = "Error"; // Or any other default value indicating no data was found.
            }
            //CheckAndAddToDataGrid();
        }

        private void CardDataTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        { }
        //{
        //    var selectedNode = (Asn1NodeViewModel)e.NewValue;
        //    if (selectedNode != null)
        //    {
        //        byte[] asn1 = selectedNode.Value;  // Use the Value property
        //        StringBuilder sb = new StringBuilder();

        //        // Check if asn1 is not null before looping over its elements
        //        if (asn1 != null)
        //        {
        //            try
        //            {
        //                foreach (byte b in asn1)
        //                {
        //                    sb.AppendFormat("{0:X2}", b);
        //                }

        //                lblTag.Content = sb.ToString();
        //                lblLength.Content = asn1.Length.ToString();  // byte arrays have a Length property

        //                sb.Clear();  // clear the StringBuilder for reuse

        //                foreach (byte b in asn1)
        //                {
        //                    sb.AppendFormat("{0:X2}", b);
        //                }

        //                txtData.Text = sb.ToString();
        //                lblDescription.Content = "Description not implemented";
        //            }
        //            catch (Exception ex)
        //            {
        //                // Display the exception in a message box
        //                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //        else
        //        {
        //            // Handle the case where asn1 is null
        //            // For example, you might want to log an error or display a message to the user
        //            MessageBox.Show("The selected node does not contain any data.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }
        //    else
        //    {
        //        lblTag.Content = String.Empty;
        //        lblLength.Content = String.Empty;
        //        lblDescription.Content = String.Empty;
        //        txtData.Text = String.Empty;
        //    }
        //}


        // You need to handle the tag description. This could be a lookup in a dictionary or database.
        // For simplicity, this

        //private void PopulateTreeView(byte[] data)
        //        {
        //            // Create a new tree node
        //            TreeNode node = new TreeNode();
        //            node.Name = $"PSE Data: {BitConverter.ToString(data)}";

        //            // For now, we'll leave the Children collection empty.
        //            // In a real application, you would populate this with the child nodes.
        //            node.Children = new ObservableCollection<TreeNode>();

        //            // Add the node to the tree view
        //            // Note that you'll need to bind the TreeView's ItemsSource property to a collection of TreeNode objects
        //            _treeNodes.Add(node);
        //        }



        private void ribbonTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        //private void GenerateReport_Click(object sender, RoutedEventArgs e)
        //{
        //    // Assuming you have a collection named CardRecords in your ViewModel
        //    var generator = new ReportGenerator("Cards Chip QC Report", usernameLabel.Content.ToString(), Reccounter.Content.ToString(), viewModel.CardRecords);

        //    string filename = generator.GenerateReport();

        //    // Automatically open the generated PDF
        //    //System.Diagnostics.Process.Start(filename);
        //    string fullPath = System.IO.Path.Combine(Environment.CurrentDirectory, "CardsQCReport.pdf");
        //    var psi = new ProcessStartInfo
        //    {
        //        FileName = fullPath,
        //        UseShellExecute = true
        //    };
        //    Process.Start(psi);

        //    //string fullPath = System.IO.Path.Combine(Environment.CurrentDirectory, "CardsQCReport.pdf");
        //    //Process.Start(fullPath);
        //}

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (int.TryParse(Reccounter.Content.ToString(), out int recordCount) && recordCount > 0)
            {
                GenerateReport();
            }
        }

        private void GenerateReport()
        {
            var generator = new ReportGenerator("Cards Chip QC Report", usernameLabel.Content.ToString(), Reccounter.Content.ToString(), viewModel.CardRecords);

            string filename = generator.GenerateReport();

            // Automatically open the generated PDF
            string fullPath = System.IO.Path.Combine(Environment.CurrentDirectory, "CardsQCReport.pdf");
            var psi = new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        //private void GenerateReport_Click(object sender, RoutedEventArgs e)
        //{
        //    GenerateReport();
        //}


        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            // Ensure that Reccounter has a value and that it's positive
            if (int.TryParse(Reccounter.Content.ToString(), out int recordCount) && recordCount > 0)
            {
                // Assuming you have a collection named CardRecords in your ViewModel
                var generator = new ReportGenerator("Cards Chip QC Report", usernameLabel.Content.ToString(), Reccounter.Content.ToString(), viewModel.CardRecords);

                string filename = generator.GenerateReport();

                // Automatically open the generated PDF
                string fullPath = System.IO.Path.Combine(Environment.CurrentDirectory, "CardsQCReport.pdf");
                var psi = new ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show("No data found in table. Please read some cards to generate report.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}