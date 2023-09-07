using System;
using System.Text;  // for Encoding

namespace EMV_Card_Browser
{
    public class CardholderDetails
    {
        public string AppLabel { get; private set; }
        public string CardholderName { get; private set; }
        public string PAN { get; private set; }
        public string Expiry { get; private set; }

        public CardholderDetails(Asn1NodeViewModel asnData)
        {
            ParseData(asnData);
        }

        private void ParseData(Asn1NodeViewModel asn)
        {
            // Convert Tag byte[] to string for comparison
            string tag = BitConverter.ToString(asn.Tag).Replace("-", "");

            switch (tag)
            {
                case "50":
                    AppLabel = Encoding.Default.GetString(asn.Value);
                    break;
                case "5F20":
                    CardholderName = Encoding.Default.GetString(asn.Value);
                    break;
                case "5F34":
                    PAN = BitConverter.ToString(asn.Value).Replace("-", "");
                    break;
                case "5F24":
                    Expiry = BitConverter.ToString(asn.Value).Replace("-", "").Substring(0, 4); // Extracting YYYYMM
                    break;
            }
        }
    }
}
