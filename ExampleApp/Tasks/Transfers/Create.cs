﻿using System;
using System.Threading.Tasks;
using Dwolla.Client.Models;

namespace ExampleApp.Tasks.Transfers
{
    [Task("ct", "Create Transfer")]
    internal class Create : BaseTask
    {
        public override async Task Run()
        {
            Write("Funding Source ID from which to transfer: ");
            var sourceFundingSource = ReadLineAsGuid();
            Write("Funding Source ID to which to transfer: ");
            var destinationFundingSource = ReadLineAsGuid();

            Write("Include a fee? (y/n): ");
            var includeFee = ReadLine();

            Write("Include ACH details? (y/n): ");
            var includeAchDetails = ReadLine();

            string sourceAddenda = null;
            string destinationAddenda = null;

            if ("y".Equals(includeAchDetails, StringComparison.CurrentCultureIgnoreCase))
            {
                Write("Enter ACH details for Source bank account: ");
                sourceAddenda = ReadLine();

                Write("Enter ACH details for Destination bank account: ");
                destinationAddenda = ReadLine();
            }

            Uri uri;
            if ("y".Equals(includeFee, StringComparison.CurrentCultureIgnoreCase))
            {
                var fundingSource = await Service.GetFundingSourceAsync(destinationFundingSource);
                uri = await Service.CreateTransferAsync(sourceFundingSource, destinationFundingSource, 50, 1,
                    fundingSource.Links["customer"].Href, sourceAddenda, destinationAddenda);
            }
            else
            {
                uri = await Service.CreateTransferAsync(sourceFundingSource, destinationFundingSource, 50, null, null, sourceAddenda, destinationAddenda);
            }

            if (uri == null) return;

            var transfer = await Service.GetTransferAsync(Link.ParseId(uri).Value);
            WriteLine($"Created {transfer.Id}; Status: {transfer.Status}");
        }
    }
}
