using System;
using DebtCollection.Common;

namespace DebtCollection.Model.Dtos
{
    public class JurisdictionDebtorDto : TrackableTuple
    {
        public long ContractCode { get; set; }
        public int WhenCreated { get; set; }
        public string InitialJurisdictionName { get; set; }
        public string CurrentJurisdictionName { get; set; }
        public string Organisation { get; set; }
        public string OrganisationAddress { get; set; }
        public DateTime? LegalOpinionDate { get; set; }

        //writing columns
        public string ProcessingStatus { get; set; }
        public string ProcessingNotes { get; set; }
    }
}