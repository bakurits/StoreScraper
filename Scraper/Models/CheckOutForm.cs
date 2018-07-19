using System.Collections.Generic;

namespace StoreScraper.Models
{
    class CheckOutForm
    {
        public Dictionary<string, string> RequestData { get; set; }

        public CheckOutForm(Data dataObj)
        {
            this.RequestData = new Dictionary<string, string>()
            {

                {"_method", dataObj.PMethod},
                {"authenticity_token", dataObj.AuthenticityToken},
                {"order[email]", dataObj.Email},
                {"order[state_lock_version]", dataObj.StateLockVersion},
                {"order[bill_address_attributes][firstname]", dataObj.FirstName},
                {"order[bill_address_attributes][lastname]", dataObj.LastName},
                {"order[bill_address_attributes][address1]", dataObj.Address1},
                {"order[bill_address_attributes][address2]", dataObj.Address2},
                {"order[bill_address_attributes][city]", dataObj.City},
                {"order[bill_address_attributes][country_id]", dataObj.CountryID},
                {"order[bill_address_attributes][state_id]", dataObj.BillingStateID},
                {"order[bill_address_attributes][zipcode]", dataObj.ZipCode},
                {"order[bill_address_attributes][phone]", dataObj.Phone},
                {"order[bill_address_attributes][hs_fiscal_code]", dataObj.FiscalCode},
                {"order[bill_address_attributes][id]", dataObj.BillingID},
                {"order[use_billing]", dataObj.UseBilling},
                {"order[ship_address_attributes][state_id]", dataObj.ShippingStateID},
                {"order[ship_address_attributes][id]", dataObj.ShippingID},
                {"order[terms_and_conditions]", dataObj.TermsAndConditions},
                {"commit", dataObj.Commit}
            };
        }

        public class Data
        {
            public string PMethod { get; set; }
            public string AuthenticityToken { get; set; }
            public string Email { get; set; }
            public string StateLockVersion { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string CountryID { get; set; }
            public string BillingStateID { get; set; }
            public string ZipCode { get; set; }
            public string Phone { get; set; }
            public string FiscalCode { get; set; }
            public string BillingID { get; set; }
            public string UseBilling { get; set; }
            public string ShippingStateID{ get; set; }
            public string ShippingID{ get; set; }
            public string TermsAndConditions{ get; set; }
            public string Commit{ get; set; }
        }
    }
}
