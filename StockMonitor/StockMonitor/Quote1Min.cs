//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockMonitor
{
    using System;
    using System.Collections.Generic;
    
    public partial class Quote1Min
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public System.DateTime Date { get; set; }
        public double Open { get; set; }
        public double Low { get; set; }
        public double High { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }
        public override string ToString()
        {
            return $"{Id}:{Symbol}, {Date}, {Open}, {Volume}";
        }
    }
}
