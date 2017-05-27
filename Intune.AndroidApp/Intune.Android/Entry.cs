using System;

namespace Intune.Android
{
    public class Entry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public TxnType TxnType { get; set; }
        public string Notes { get; set; }
        public DateTime TxnDate { get; set; }
        public double Quantity { get; set; }
        public decimal Amount { get; set; }
        public int VoidId { get; set; }

        //public bool IsValid();
    }

    public enum TxnType
    {
        Paid = 0,
        Issued = 1,
        Received = 2
    }
}