using System;
using System.Text;

namespace EMV_Card_Browser
{
    public class AIDSelection
    {
        private PCSCReader _cardReader;

        private static readonly byte[][] knownAIDs =
        {
            // Visa
            new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x03, 0x10, 0x10 },
            // MasterCard
            new byte[] { 0xA0, 0x00, 0x00, 0x00, 0x04, 0x10, 0x10 }
        };

        public AIDSelection(PCSCReader cardReader)
        {
            _cardReader = cardReader ?? throw new ArgumentNullException(nameof(cardReader));
        }

        // This method attempts to select from the hardcoded list of AIDs.
        public (byte[] AID, APDUResponse Response) SelectKnownAID()
        {
            const int maxRetries = 3;
            int currentRetry = 0;

            while (currentRetry < maxRetries)
            {
                foreach (var aid in knownAIDs)
                {
                    APDUResponse response = SelectAID(aid);
                    if (IsSelectionSuccessful(response))
                    {
                        return (aid, response);
                    }
                }

                currentRetry++;

                // If you reached this point, it means the selection was unsuccessful.
                // Wait a bit before the next retry. You can adjust the waiting time if needed.
                System.Threading.Thread.Sleep(1000);  // waits for 1 second
            }

            throw new InvalidOperationException("No known AID was selected successfully after " + maxRetries + " tries.");
        }


        //// This method attempts to select from the hardcoded list of AIDs.
        //public (byte[] AID, APDUResponse Response) SelectKnownAID()
        //{
        //    foreach (var aid in knownAIDs)
        //    {
        //        APDUResponse response = SelectAID(aid);
        //        if (IsSelectionSuccessful(response))
        //        {
        //            return (aid, response);
        //        }
        //    }

        //    throw new InvalidOperationException("No known AID was selected successfully.");
        //}

        private APDUResponse SelectAID(byte[] aid)
        {
            // APDU for SELECT command (CLA=00, INS=A4)
            APDUCommand selectCommand = new APDUCommand(0x00, 0xA4, 0x04, 0x00, aid, (byte)aid.Length);
            APDUResponse response = _cardReader.Transmit(selectCommand);

            // Check if the response indicates more data is available.
            if (response.SW1 == 0x61)
            {
                byte le = response.SW2;
                APDUCommand getResponseCommand = new APDUCommand(0x00, 0xC0, 0x00, 0x00, null, le);
                response = _cardReader.Transmit(getResponseCommand);
            }

            return response;
        }

        private bool IsSelectionSuccessful(APDUResponse response)
        {
            // SW1 of 0x90 and SW2 of 0x00 indicates success
            return response.SW1 == 0x90 && response.SW2 == 0x00;
        }
    }
}
