using System;
using System.Text.RegularExpressions;
using Intune.ApiGateway;

namespace Intune.Android
{
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
}
