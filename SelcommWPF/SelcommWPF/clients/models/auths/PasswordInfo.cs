using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.auths
{
    class PasswordInfo
    {
        public bool MustChange { get; set; }

        public string ExpiryDate { get; set; }

        public string ExpiryWarningDate { get; set; }

        public bool EmailRegistered { get; set; }

        public bool EmailVerified { get; set; }

        public string Email { get; set; }

        public bool MobileRegistered { get; set; }

        public bool MobileVerified { get; set; }

        public string Mobile { get; set; }

        public PasswordConfig PasswordConfiguration { get; set; }

        public class PasswordConfig
        {
            public int MinimumLength { get; set; }

            public int MaximumLength { get; set; }

            public int MinimumStrongLength { get; set; }

            public int MinimumNumberSpecialCharacters { get; set; }

            public int MinimumNumberUpperCaseCharacters { get; set; }

            public int MinimumNumberNumberIntegers { get; set; }

            public int MaximumRepeatingCharacters { get; set; }

            public int MaximumLoginAttempts { get; set; }

            public int LoginLockoutPeriod { get; set; }

            public string LoginLockoutMessage { get; set; }

            public bool UseUserDetails { get; set; }

            public int ExpiryPeriod { get; set; }

            public int TemporaryPasswordExpiryPeriod { get; set; }

            public int MaximumPasswordChangesPerDay { get; set; }

            public string MobileRegistrationQuestion { get; set; }

            public string MobileRegistrationLabel { get; set; }

            public string MobileRegistrationMandatoryLabel { get; set; }

            public string EmailRegistrationQuestion { get; set; }

            public string EmailRegistrationLabel { get; set; }

            public string EmailRegistrationMandatoryLabel { get; set; }

            public string PasswordComplexityMessage { get; set; }
        }
    }
}
