using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TestingSomething
{
    class Program
    {
        static void Main(string[] args)
        {

            //Console.WriteLine(ConvertStringToHex("diet", Encoding.UTF8));

            //Console.WriteLine(ConvertStringToHex("tide", Encoding.UTF8));

            var privateKey = "-----BEGIN PRIVATE KEY-----\r\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCQ55nWPMRXfmtbW+XcoEdANz0c7YsyeNuDBfoa61oJ8j2x6HphsuP/jVI1WjnEgQyYuK1cCh2Iu7dCFf8lh64+GVlv1R9gaOgK5dCyK0Ul5C8qr5fA30j5oy9/twTQQzuYgQVLDfPzA6Z/I033NLN9UBxkLb4sWAZRViZZMBEBRRQ4LAxsABSp+dW96nwlO+seU6JziImEWR6oDk3rSh0HhVb7+UNH1oQRl3OKCX1foUtcNlxxWpD1tbFhm3lRoK49AhlBltyf1ouDAWpEHkG6LSNRAYM9yWyvYKsd+0EJjYYOlQ2l8jAXDZNjpEtVl+6lPjytsNgfX/8uIlz14+OlAgMBAAECggEAHuy2iaDFXFDYE5LMvyWbnypKyhouC1L8sf08D7OrrxS6MvZE7AXUH81mvokna/cIm61KHirwe6ZoKVvtbHKE7q7VRgcrX5Vz9CNwwx117+y/XjJERj7A4boAYJbMGbBHjzf1y7tKFVJxnY+Iy5m9NqC6tmyGugr9Ki2jGISoFGd55OaLOipBEyKioNPKVpKaxHf78QQ+5p92uJHzrNx5TtyhbDAnsDiB0LRk/aKYPuFL99z4JAwyKl3MNVhWjwU8vhtcSehbSrczPJE64fMdPRkVS6L9S9j68cTDyZlmcoHAaeWNMHluAHn1PpS0DaZ5W7eAM3Q5i0mQm3YILtZcrwKBgQDKqDs26Ljh1yXHJvIfFu+UX/gmlaH7k0TQnx7gM/eXSSeEV/gRV3pvhy6kxipGg0PTgmwsgOK6HhXVPSED+gC6v9nPjnTc06FbQSWjVK/Q/4LLELcyf9CRqDlfAu4/DOXgmE2+U4p9Ln7faRRH/Cb4bIwxapp7xLkRFF8FDg/jBwKBgQC3C8z+Y7k0rvDVjqAVVi0y1xb526VXUVbUzUy8ESu8CM6xx7/tczKdAbT24rPPX8FJ0JpHM901YMWxDcNVwNYWcShalr3bnFExivHHVayO+xGtpo4cvsW6m651G5GrcPEQBYDFjU/xCCGoHhJwvfsrm9/1oNkCBtt2cSGx0Gh88wKBgQC8rumC9Ca+zqvzwkUHwTkK8ld+xS5tpu4ZNy1uHbRV/Nx4Edd7ffEWycwZyg8uW6TY1sOUo+N02fIF+cY1h50KQvmuj7oqqDfBWOSML5sKgfrqygNnVy1/kBrqZTYhKgcVl3g6zllQt+uUYqdpZlF6VsYodFKlDcIpLshRUohkHQKBgQCRuoNpxFse5mg2nWY1lnZFk5hnM8dezRMkOI5u6/y1pw1n3G+gy6Xv+fvo0nP1FXb9HWa2aUQjyveZDiwFiZDoLQ7DkF93B+xcVRJIcJ0A+EIKzh82I2uoNWmfMmO26iQBbWAn52lNq/ufrNFhkN72yRJ0q6oZQvxWP5tjxVhqoQKBgEzeJgXSvvRmJbAHj5pTNPMGQuR0zTCZMIcZFJlJ/vpYz7BDAzHN8pPU/79cBovb9ckEeY3RWMc7dq0g8I4EAuuRyTT6hpz+5odAwuKhO5KbW0oQkJrNeWldeSOhmDFdhwXqTjrqyaguBr+IccrpzHRagxdc7yiwK/CPK/sdZB82\r\n-----END PRIVATE KEY-----";

            try
            {
                var key = ImportPrivateKey(privateKey);

                var dec = DecryptData("TKuJTNkShFlUg6zgvHewHGY6iy5BjUjW8BsWZE2v+sLr0o8tDuItJ2P4Bnqmk1s0FCjE8WEk9mG0bXuNjqouDQw9+NiWCmx/7XSIZ4p97naSrF1Lzdv0hYnGUNylv86Lv+ed8+j8fijxB8t1LynrBqeQy2M6CWCZJE7DN0ySVN+9ndmlyFdzhTQWVraGQ9ZDdqSRlAG2A5yxnOOo3WmFnqcLr1qRs2hzWCmfxwyAYNo2XtK18u6CozrgNNyFRliEaQVnAgdrrwjj1gJQca99Sny335igANDLAroqlNeKorr3TtJ9dVQ1pHwnfKSqOTddU9Tsmu8ezr9P/oX3C3JAeA==", key);

                Console.WriteLine(dec.ToString());
            }
            catch (Exception ex)
            {

                throw ex;
            }

            Console.ReadLine();
        }



        public static byte[] Base64ToArrayBuffer(string base64)
        {
            return Convert.FromBase64String(base64);
        }

        public static RSA ImportPrivateKey(string privateKeyPem)
        {
            // Remove the PEM header and footer
            var pemBody = privateKeyPem
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("\r\n", "")
                .Replace("\n", "");

            // Convert the base64-encoded PEM body to a byte array
            var pemBytes = Base64ToArrayBuffer(pemBody);

            // Parse the DER-encoded private key
            return RSA.Create();
        }

        public static string DecryptData(string message, RSA privateKey)
        {
            // Convert the encrypted data from a base64-encoded string to a byte array
            var encryptedDataBytes = Base64ToArrayBuffer(message);

            // Decrypt the data with the private key
            var decryptedDataBytes = privateKey.Decrypt(encryptedDataBytes, RSAEncryptionPadding.OaepSHA256);

            // Convert the decrypted data to a string
            var decryptedData = Encoding.UTF8.GetString(decryptedDataBytes);

            return decryptedData;
        }


        public static string ConvertStringToHex(String input, System.Text.Encoding encoding)
        {
            Byte[] stringBytes = encoding.GetBytes(input);
            var sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }

            return sbBytes.ToString();
        }
        static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        static int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
    }
}
