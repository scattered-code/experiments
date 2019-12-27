using System;
using System.Collections.Generic;

namespace AsyncProcessingBenchmarks
{
    public class Order
    {
        public string Id { get; set; }
        public string Company { get; set; }
        public string Employee { get; set; }
        public DateTime OrderedAt { get; set; }
        public DateTime RequireAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public Address ShipTo { get; set; }
        public string ShipVia { get; set; }
        public decimal Freight { get; set; }
        public List<OrderLine> Lines { get; set; }
    }

    public class ProcessedOrder
    {
        public string Id { get; set; }

        public string OrderId { get; set; }

        public DateTime DateProcessed { get; set; } = DateTime.UtcNow;
    }

    public class OrderLine
    {
        public string Product { get; set; }
        public string ProductName { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public Location Location { get; set; }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
