using System;
using System.Collections;
using System.Collections.Generic;

namespace Dwolla.Client.Models.Responses
{
    public class DocumentResponse : BaseResponse
    {
        public Guid Id { get; set; }
        public DocumentStatus Status { get; set; }
        public DocumentType Type { get; set; }
        public DateTime Created { get; set; }
        public string FailureReason { get; set; }
        public ICollection<DocumentFailureReason> AllFailureReasons { get; set; }
    }

    public class DocumentFailureReason
    {
        public string Reason { get; set; }
        public string Description { get; set; }
    }
}
