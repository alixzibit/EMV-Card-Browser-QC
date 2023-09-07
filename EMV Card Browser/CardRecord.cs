﻿using System;

namespace EMV_Card_Browser
{
    public class CardRecord
    {
        public string Srno { get; set; }
        public string CardType { get; set; }
        public string CardholderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public DateTime Timestamp { get; set; }

    }
}

