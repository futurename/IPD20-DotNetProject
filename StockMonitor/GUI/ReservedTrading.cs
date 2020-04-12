//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GUI
{
    using System;
    using System.Collections.Generic;
    
    public partial class ReservedTrading
    {
        public ReservedTrading() { }
        public ReservedTrading(int companyId, int userId, TradeEnum trade, string quantityStr,
              string targetPriceStr, DateTime pickDate, DateTime pickTime)
        {
            CompanyId = companyId;
            UserId = userId;
            TradeType = trade.ToString();
            SetTargetPrice(targetPriceStr);
            SetDueDateTime(pickDate, pickTime);
            SetValume(quantityStr);
            DateTime = DateTime.Now;
        }

        void SetDueDateTime(DateTime pickDate, DateTime pickTime)
        {
            if (pickDate == null) { throw new ArgumentException("Choose Date"); }
            //if (pickDate.CompareTo(DateTime.Now.Date) < 0) { throw new ArgumentException("Choose Date(today or later)"); }

            if (pickTime.Ticks == 0)
            {
                DueDateTime = pickDate.AddHours(23).AddMinutes(59);
            }
            else
            {
                DueDateTime = pickDate.AddHours(pickTime.Hour).AddMinutes(pickTime.Minute);

            }
        }
        void SetValume(string quantityStr)
        {
            int quantity;
            if (!int.TryParse(quantityStr, out quantity))
            {
                throw new ArgumentException("Quantity is not valid");
            }
            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity is not valid");
            }
            Volume = quantity;
        }
        void SetTargetPrice(string targetPriceStr)
        {
            double targetPrice;
            if (!double.TryParse(targetPriceStr, out targetPrice))
            {
                throw new ArgumentException("Quantity is not valid");
            }
            if (targetPrice <= 0)
            {
                throw new ArgumentException("Quantity is not valid");
            }
            TargetPrice = targetPrice;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public System.DateTime DateTime { get; set; }
        public double TargetPrice { get; set; }
        public long Volume { get; set; }
        public string TradeType { get; set; }
        public System.DateTime DueDateTime { get; set; }
        public int CompanyId { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual User User { get; set; }
    }
}
