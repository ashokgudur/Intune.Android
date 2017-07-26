using System;

namespace Intune.ApiGateway.Model
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
        public bool IsNew { get { return Id == 0; } }
        public bool IsVoid { get { return VoidId != 0; } }

        public Entry()
        {
            TxnDate = DateTime.Today;
        }

        public bool IsValid()
        {
            if (TxnDate == DateTime.MinValue) return false;
            if (Quantity == 0 && Amount == 0) return false;

            return true;
        }
    }
}