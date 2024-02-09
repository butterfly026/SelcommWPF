using System.Collections.Generic;

namespace SelcommWPF.clients.models
{
    class UserModel
    {
        public string Id { get; set; }

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

        public string Email { get; set; }

        public string EmployeeReference { get; set; }

        public string HomePhone { get; set; }

        public string WorkPhone { get; set; }

        public string MobilePhone { get; set; }

        public List<BusinessUnitModel> BusinessUnits { get; set; }

        public List<BusinessUnitModel> Roles { get; set; }

        public string RolesText { get; set; }

        public List<BusinessUnitModel> Teams { get; set; }

        public class BusinessUnitModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public bool IsChecked { get; set; }

        }

    }
}
