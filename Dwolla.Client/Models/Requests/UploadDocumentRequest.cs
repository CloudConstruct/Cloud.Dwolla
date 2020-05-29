namespace Dwolla.Client.Models.Requests
{
    public class UploadDocumentRequest
    {
        public DocumentType DocumentType { get; set; }
        public File Document { get; set; }
    }
}
