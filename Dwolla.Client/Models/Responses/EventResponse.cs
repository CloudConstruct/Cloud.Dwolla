using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models.Responses
{
    public class EventResponse : BaseResponse
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public EventType? Topic { get; set; }
        public Guid ResourceId { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        [EnumMember(Value = "customer_created")]
        CustomerCreated,
        [EnumMember(Value = "customer_reverification_needed")]
        CustomerReverificationNeeded,
        [EnumMember(Value = "customer_verification_document_needed")]
        CustomerVerificationDocumentNeeded,
        [EnumMember(Value = "customer_verification_document_uploaded")]
        CustomerVerificationDocumentUploaded,
        [EnumMember(Value = "customer_verification_document_failed")]
        CustomerVerificationDocumentFailed,
        [EnumMember(Value = "customer_verification_document_approved")]
        CustomerVerificationDocumentApproved,
        [EnumMember(Value = "customer_verified")]
        CustomerVerified,
        [EnumMember(Value = "customer_suspended")]
        CustomerSuspended,
        [EnumMember(Value = "customer_activated")]
        CustomerActivated,
        [EnumMember(Value = "customer_deactivated")]
        CustomerDeactivated,
        [EnumMember(Value = "customer_beneficial_owner_created")]
        CustomerBeneficialOwnerCreated,
        [EnumMember(Value = "customer_beneficial_owner_removed")]
        CustomerBeneficialOwnerRemoved,
        [EnumMember(Value = "customer_beneficial_owner_verification_document_needed")]
        CustomerBeneficialOwnerVerificationDocumentNeeded,
        [EnumMember(Value = "customer_beneficial_owner_verification_document_uploaded")]
        CustomerBeneficialOwnerVerificationDocumentUploaded,
        [EnumMember(Value = "customer_beneficial_owner_verification_document_failed")]
        CustomerBeneficialOwnerVerificationDocumentFailed,
        [EnumMember(Value = "customer_beneficial_owner_verification_document_approved")]
        CustomerBeneficialOwnerVerificationDocumentApproved,
        [EnumMember(Value = "customer_beneficial_owner_reverification_needed")]
        CustomerBeneficialOwnerReverificationNeeded,
        [EnumMember(Value = "customer_funding_source_added")]
        CustomerFundingSourceAdded,
        [EnumMember(Value = "customer_funding_source_removed")]
        CustomerFundingSourceRemoved,
        [EnumMember(Value = "customer_funding_source_verified")]
        CustomerFundingSourceVerified,
        [EnumMember(Value = "customer_funding_source_unverified")]
        CustomerFundingSourceUnverified,
        [EnumMember(Value = "customer_funding_source_negative")]
        CustomerFundingSourceNegative,
        [EnumMember(Value = "customer_funding_source_updated")]
        CustomerFundingSourceUpdated,
        [EnumMember(Value = "customer_microdeposits_added")]
        CustomerMicrodepositsAdded,
        [EnumMember(Value = "customer_microdeposits_failed")]
        CustomerMicrodepositsFailed,
        [EnumMember(Value = "customer_microdeposits_completed")]
        CustomerMicrodepositsCompleted,
        [EnumMember(Value = "customer_microdeposits_maxattempt")]
        CustomerMicrodepositsMaxattempt,
        [EnumMember(Value = "customer_bank_transfer_created")]
        CustomerBankTransferCreated,
        [EnumMember(Value = "customer_bank_transfer_cancelled")]
        CustomerBankTransferCancelled,
        [EnumMember(Value = "customer_bank_transfer_failed")]
        CustomerBankTransferFailed,
        [EnumMember(Value = "customer_bank_transfer_creation_failed")]
        CustomerBankTransferCreationFailed,
        [EnumMember(Value = "customer_bank_transfer_completed")]
        CustomerBankTransferCompleted,
        [EnumMember(Value = "customer_transfer_created")]
        CustomerTransferCreated,
        [EnumMember(Value = "customer_transfer_cancelled")]
        CustomerTransferCancelled,
        [EnumMember(Value = "customer_transfer_failed")]
        CustomerTransferFailed,
        [EnumMember(Value = "customer_transfer_completed")]
        CustomerTransferCompleted,
        [EnumMember(Value = "customer_mass_payment_created")]
        CustomerMassPaymentCreated,
        [EnumMember(Value = "customer_mass_payment_completed")]
        CustomerMassPaymentCompleted,
        [EnumMember(Value = "customer_mass_payment_cancelled")]
        CustomerMassPaymentCancelled,
        [EnumMember(Value = "customer_label_created")]
        CustomerLabelCreated,
        [EnumMember(Value = "customer_label_ledger_entry_created")]
        CustomerLabelLedgerEntryCreated,
    }
}
