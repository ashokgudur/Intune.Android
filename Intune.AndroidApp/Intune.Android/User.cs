using System;
using System.Text.RegularExpressions;

namespace Intune.Android
{
    public class Country
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string IsdCode { get; set; }
    }

    public class MobileNumberValidator
    {
        const string defaultCountryIsdCode = "+91";
        string _inputNumber;

        public MobileNumberValidator(string inputNumber)
        {
            _inputNumber = $"{inputNumber}".Trim();
        }

        public bool IsValid()
        {
            var mobile = _inputNumber;

            if (mobile.StartsWith("0"))
                return false;

            if (mobile.StartsWith("+"))
                mobile = mobile.Replace("+", " ").Trim();

            if (mobile.Length < 10)
                return false;

            if (mobile.Length == 10)
                return Regex.IsMatch(mobile, @"^[0-9]{10}$");

            if (!Regex.IsMatch(mobile, @"^[0-9]$"))
                return false;

            var isdCode = mobile.Substring(0, mobile.Length - 10);
            return IntuneService.IsCountryIsdCodeValid(isdCode);
        }

        public string GetIsdCodeWithPlus()
        {
            if (string.IsNullOrWhiteSpace(GetIsdCodeWithoutPlus()))
                return defaultCountryIsdCode;

            return $"+{GetIsdCodeWithoutPlus()}";
        }

        public string GetIsdCodeWithoutPlus()
        {
            var mobile = _inputNumber;

            if (mobile.StartsWith("+"))
                mobile = mobile.Replace("+", " ").Trim();

            var isdCode = mobile.Substring(0, mobile.Length - 10);

            if (string.IsNullOrWhiteSpace(isdCode))
                return defaultCountryIsdCode.Replace("+", "");

            return isdCode;
        }

        public string GetFullMobileNumber()
        {
            return $"{GetIsdCodeWithPlus()}{GetMobileNumberWithoutIsdCode()}";
        }

        public string GetMobileNumberWithoutIsdCode()
        {
            var mobile = _inputNumber;

            if (mobile.StartsWith("+"))
                mobile = mobile.Replace("+", " ").Trim();

            return mobile.Substring(mobile.Length - 10);
        }
    }


    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string AtUserName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedOn { get; set; }
        public string SessionToken { get; set; }
        public bool IsNew { get { return Id == 0; } }

        public bool IsValid()
        {
            //TODO: Additional validations for max/min length should be cheecked.
            //TODO: Validate Email for email format
            //TODO: Validate Mobile Number for its length, with country code included.
            //TODO: Validate AtUserName to prefix with '@' symobol. Must be included by user.

            if (string.IsNullOrWhiteSpace(Name)) return false;
            if (string.IsNullOrWhiteSpace(Mobile)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;
            if (string.IsNullOrWhiteSpace(Password)) return false;
            if (string.IsNullOrWhiteSpace(AtUserName)) return false;

            return true;
        }
    }
}