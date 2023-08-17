using System.Collections.Generic;
namespace EMV_Card_Browser
{
    public class CardData
    {
        public string Name { get; set; }
        public List<CardData> Children { get; set; } = new List<CardData>();
    }
}