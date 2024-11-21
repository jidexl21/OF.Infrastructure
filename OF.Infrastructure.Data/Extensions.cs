using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OF.Infrastructure.Data
{
    public static class Extensions
    {
        public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
        {
            var ls = new ConcurrentBag<T>(list);
            await Task.WhenAll(ls.Select(x => func(x)));
            list = ls.ToList();
        }

        public static string CreateMD5(this string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static string Btoa(this string toEncode)
        {
            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(toEncode);
            string toReturn = System.Convert.ToBase64String(bytes);
            return toReturn;
        }

        public static bool IsCollection(this object item) => item.GetType().GetInterface("IList") != null;
        public static object GetValueX(this System.Reflection.PropertyInfo item, object parentItem) =>
            (item.PropertyType.IsEnum) ? item.GetValue(parentItem).ToString() : item.GetValue(parentItem);
    }
}
