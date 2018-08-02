using System.Collections.Generic;

namespace StoreScraper.Models
{
    class Info
    {
        public Dictionary<string, string> Data;

        public Info(InfoData dataObj)
        {
            Data = new Dictionary<string, string>()
            {
                {"order[state_lock_version]", dataObj.StateLockVersion},
                {"order[bill_address_attributes][firstname]", dataObj.FirstName},
                {"order[bill_address_attributes][lastname]", dataObj.LastName},
                {"order[bill_address_attributes][address1]", dataObj.Address1},
                {"order[bill_address_attributes][address2]", dataObj.Address2},
                {"order[bill_address_attributes][city]", dataObj.City},
                {"order[bill_address_attributes][country_id]", dataObj.CountryId},
                {"order[bill_address_attributes][state_id]", dataObj.StateId},
                {"order[bill_address_attributes][zipcode]", dataObj.ZipCode},
                {"order[bill_address_attributes][phone]", dataObj.Phone},
                {"order[bill_address_attributes][id]", dataObj.Id}
            };
        }

        public class InfoData
        {
            public string Email { get; set; }
            public string StateLockVersion { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string CountryId { get; set; }
            public string StateId { get; set; }
            public string ZipCode { get; set; }
            public string Phone { get; set; }
            public string Id { get; set; }
        }
    }
}
