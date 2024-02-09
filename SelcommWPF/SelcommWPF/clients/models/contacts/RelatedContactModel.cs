using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class RelatedContactModel
    {
        public string Id { get; set; }

        public List<UserModel.BusinessUnitModel> BusinessUnits { get; set; }

        public List<Relationship> RelationshipTypes { get; set; }

        public string RelatedContactCode { get; set; }

        public string RelatedContact { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public List<Relationship> Relationships { get; set; }

        public string RelationshipText { get; set; }

        public string AuthenticationLoginId { get; set; }

        public string AuthenticationMobile { get; set; }

        public string AuthenticationEmail { get; set; }

        public string AuthenticationStatus { get; set; }

        public bool Suspendable { get; set; }

        public string LastLogIn { get; set; }

        public string PasswordExpires { get; set; }

        public string FamilyName { get; set; }

        public string FirstName { get; set; }

        public string Name { get; set; }

        public string StatusId { get; set; }

        public string Status { get; set; }

        public string Title { get; set; }

        public string DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string EmployeeReference { get; set; }

        public string HomePhone { get; set; }

        public string WorkPhone { get; set; }

        public string MobilePhone { get; set; }

        public List<UserModel.BusinessUnitModel> Roles { get; set; }

        public List<UserModel.BusinessUnitModel> Teams { get; set; }

        public long? TimezoneId { get; set; }

        public class Relationship
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public int DisplayOrder { get; set; }

            public bool Visible { get; set; }

            public string Note { get; set; }

            public int MinimumOccurrences { get; set; }

            public int MaximumOccurrences { get; set; }

            public bool System { get; set; }

            public string From { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class TimeZone
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public string CountryId { get; set; }

            public string Country { get; set; }
        }

    }
}
