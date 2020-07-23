using System;

namespace Dwolla.Client.Models.Responses
{
    public class Customer : BaseResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public CustomerType Type { get; set; }
        public CustomerStatus Status { get; set; }
        public DateTime Created { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string DoingBusinessAs { get; set; }
        public string Website { get; set; }
        public Controller Controller { get; set; }
        public Guid BusinessClassification { get; set; }
        public string BusinessType { get; set; }
    }
}
