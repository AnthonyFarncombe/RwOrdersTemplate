using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RwOrders.Functions
{
    [Flags]
    public enum PasswordType
    {
        Upper = 0x1,
        Lower = 0x2,
        Numeric = 0x4,
        Special = 0x8
    }

    public static class PasswordManager
    {
        private const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string lower = "abcdefghijklmnopqrstuvwxyz";
        private const string numeric = "0123456789";
        private const string special = @"!£$%^&*()@\/|?";

        public static string Generate()
        {
            return Generate(10);
        }

        public static string Generate(short length)
        {
            return Generate(length, true, true, true, true);
        }

        public static string Generate(short length, PasswordType require)
        {
            bool requireUpper = (require & PasswordType.Upper) > 0;
            bool requireLower = (require & PasswordType.Lower) > 0;
            bool requireNumeric = (require & PasswordType.Numeric) > 0;
            bool requireSpecial = (require & PasswordType.Special) > 0;
            return Generate(length, requireUpper, requireLower, requireNumeric, requireSpecial);
        }

        public static string Generate(short length, PasswordType require, PasswordType allow)
        {
            bool requireUpper = (require & PasswordType.Upper) > 0;
            bool requireLower = (require & PasswordType.Lower) > 0;
            bool requireNumeric = (require & PasswordType.Numeric) > 0;
            bool requireSpecial = (require & PasswordType.Special) > 0;
            bool allowUpper = (allow & PasswordType.Upper) > 0;
            bool allowLower = (allow & PasswordType.Lower) > 0;
            bool allowNumeric = (allow & PasswordType.Numeric) > 0;
            bool allowSpecial = (allow & PasswordType.Special) > 0;
            return Generate(length, requireUpper, requireLower, requireNumeric, requireSpecial, allowUpper, allowLower, allowNumeric, allowSpecial);
        }

        public static string Generate(short length, bool requireUpper, bool requireLower, bool requireNumeric, bool requireSpecial)
        {
            return Generate(length, requireUpper, requireLower, requireNumeric, requireSpecial, true, true, true, true);
        }

        public static string Generate(short length, bool requireUpper, bool requireLower, bool requireNumeric, bool requireSpecial,
            bool allowUpper, bool allowLower, bool allowNumeric, bool allowSpecial)
        {
            if (length < 1)
                length = 1;
            if (requireUpper && !allowUpper)
                allowUpper = true;
            if (requireLower && !allowLower)
                allowLower = true;
            if (requireNumeric && !allowNumeric)
                allowNumeric = true;
            if (requireSpecial && !allowSpecial)
                allowSpecial = true;
            List<char> password = new List<char>();
            Random r = new Random();
            if (requireUpper)
                password.Add(upper[r.Next(upper.Length - 1)]);
            if (requireLower)
                password.Add(lower[r.Next(lower.Length - 1)]);
            if (requireNumeric)
                password.Add(numeric[r.Next(numeric.Length - 1)]);
            if (requireSpecial)
                password.Add(special[r.Next(special.Length - 1)]);
            StringBuilder sb = new StringBuilder();
            if (allowUpper)
                sb.Append(upper);
            if (allowLower)
                sb.Append(lower);
            if (allowNumeric)
                sb.Append(numeric);
            if (allowSpecial)
                sb.Append(special);
            string allchars = sb.ToString();
            while (password.Count < length)
                password.Add(allchars[r.Next(allchars.Length - 1)]);
            return new string(password.OrderBy(c => r.Next(password.Count - 1)).ToArray());
        }
    }
}