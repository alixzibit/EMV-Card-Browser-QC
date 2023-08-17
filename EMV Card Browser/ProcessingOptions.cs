using EMV_Card_Browser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EMV_Card_Browser
{
    public class ProcessingOptions
    {
        private PCSCReader _cardReader;
        private RecordReader _recordReader;

        public ProcessingOptions(PCSCReader cardReader)
        {
            _cardReader = cardReader ?? throw new ArgumentNullException(nameof(cardReader));
            _recordReader = new RecordReader(_cardReader);
        }
        public List<APDUResponse> ReadRecords { get; private set; } = new List<APDUResponse>();

        public APDUResponse GetProcessingOptions()
        {
            string logPath = @"C:\EMV_CB_log\emvcard_log.txt";
            void LogToFile(string message)
            {
                using (StreamWriter writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine($"{DateTime.Now:G}: {message}");
                }
            }
            byte[] commandData = new byte[] { 0x83, 0x00 };  // Empty PDOL
            LogToFile("Preparing APDU command with Empty PDOL.");

            // Build the APDU command for GPO
            APDUCommand apdu = new APDUCommand(0x80, 0xA8, 0x00, 0x00, commandData, 0x00);
            LogToFile($"APDU Command prepared: {BitConverter.ToString(apdu.CommandData)}.");

            APDUResponse response = _cardReader.Transmit(apdu);
            LogToFile($"Received APDU Response with SW1 = {response.SW1}, SW2 = {response.SW2}.");

            // Create a container for the entire response data
            List<byte> fullData = new List<byte>();

            // If response data is available, append it to fullData
            if (response.Data != null)
            {
                fullData.AddRange(response.Data);
                LogToFile("Added received data to fullData container.");
            }

            // Retry with correct length if status word is 0x6C
            if (response.SW1 == 0x6C)
            {
                LogToFile("Response indicates wrong length. Retrying with correct length...");
                apdu = new APDUCommand(0x80, 0xA8, 0x00, 0x00, commandData, response.SW2);
                response = _cardReader.Transmit(apdu);
                fullData.Clear();
                fullData.AddRange(response.Data);
                LogToFile($"Retried APDU Command and updated fullData with new data: {BitConverter.ToString(fullData.ToArray())}.");
            }

            // Keep fetching more data while status indicates more data
            while (response.SW1 == 0x61)
            {
                LogToFile("Status indicates more data is available. Fetching...");
                apdu = new APDUCommand(0x00, 0xC0, 0x00, 0x00, null, response.SW2);
                response = _cardReader.Transmit(apdu);
                // Append response data to fullData
                if (response.Data != null)
                {
                    fullData.AddRange(response.Data);
                    LogToFile($"Appended new data to fullData: {BitConverter.ToString(fullData.ToArray())}.");
                }
                else
                {
                    LogToFile("No additional data received to append to fullData.");
                }
            }

            // If needed, replace the original response data with the combined fullData
            // Assuming fullData contains: 80-0A-7C-00-18-01-06-01-08-01-01-00
            if (fullData.Count > 0)
            {
                LogToFile($"Appended new data to fullData: {BitConverter.ToString(fullData.ToArray())}");

                byte[] aip = fullData.Skip(2).Take(2).ToArray(); // Skip the template and length, then take AIP
                byte[] aflData = fullData.Skip(4).ToArray();     // Skip template, length, and AIP to get AFL data

                LogToFile($"AIP: {BitConverter.ToString(aip)}");
                LogToFile($"AFL Data: {BitConverter.ToString(aflData)}");

                List<ApplicationFileLocator> afls = new List<ApplicationFileLocator>();
                for (int i = 0; i < aflData.Length; i += 4)
                {
                    if (i + 4 <= aflData.Length)
                    {
                        byte[] aflEntry = aflData.Skip(i).Take(4).ToArray();
                        afls.Add(new ApplicationFileLocator(aflEntry));
                        LogToFile($"Added AFL Entry: {BitConverter.ToString(aflEntry)}");
                    }
                }

                foreach (var locator in afls)
                {
                    for (byte recordNum = locator.FirstRecord; recordNum <= locator.LastRecord; recordNum++)
                    {
                        var recordResponse = _recordReader.ReadRecord(locator.SFI, recordNum);
                        ReadRecords.Add(recordResponse); // Now process the recordResponse as needed
                    }
                }
            }

            else
            {
                LogToFile("No fullData available to process.");
            }


            return response;
        }

        // A helper function to determine if the GPO was successful.
        public bool IsGPOResponseSuccessful(APDUResponse response)
        {
            // Based on ISO/IEC 7816 standards, '90 00' indicates success
            return response.SW1 == 0x90 && response.SW2 == 0x00;
        }
    }
}
    