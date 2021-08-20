using Microsoft.IdentityModel.Tokens;
using OF.Auth.Core;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace OF.Infrastructure.Auth
{

    public static class Extensions
    {
        public static string GenerateJWT(this IAppUser user, string SecretKey, int tokenMinutes)
        {
            return GenerateJWT(user.ID.ToString(), SecretKey, tokenMinutes);
        }
        public static string GenerateJWT(this IAppUser user, string AppID, string SecretKey, int tokenMinutes)
        {
            var session = new Dictionary<string, object>();
            session.Add("id", user.ID);
            session.Add("appId", AppID);
            var rlist = (user.UserRoles == null) ? new List<Role>() : user.UserRoles;
            session.Add("roles", string.Join(",", rlist.Select(x=>x.Code)));
            return GenerateJWT(session, SecretKey, tokenMinutes);
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.GetEncoding(28591).GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static bool IsBase64(this string base64String)
        {
            // Credit: oybek https://stackoverflow.com/users/794764/oybek
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception exception)
            {
                // Handle the exception
                System.Diagnostics.Trace.TraceError(exception.Message);
                System.Diagnostics.Trace.TraceError(exception.StackTrace);
            }
            return false;
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.GetEncoding(28591).GetString(base64EncodedBytes);
        }

        public static string CreateMD5(this string input)
        {
            // Use input string to calculate MD5 hash
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                hashBytes = md5.ComputeHash(inputBytes);
            } ;
            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private static string GenerateJWT(string Id, string SecretKey, int tokenMinutes)
        {
            // generate token that is valid for 45 minutes
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", Id) }),
                Expires = DateTime.UtcNow.AddMinutes(tokenMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string GenerateJWT(Dictionary<string, object> valuePairs, string SecretKey, int tokenMinutes)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);
            var identity = valuePairs.Select(x => new Claim(x.Key, $"{x.Value}"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(identity),
                Expires = DateTime.UtcNow.AddMinutes(tokenMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}
