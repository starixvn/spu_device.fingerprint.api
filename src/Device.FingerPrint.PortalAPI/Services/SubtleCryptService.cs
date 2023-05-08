using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface ISubtleCryptService
    {
        string DecryptData(string message);
    }

    public class SubtleCryptService : ISubtleCryptService
    {
        private readonly string privateKeyBase64 = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCSAqGk0C7F7lvZe5ozeyTIroJXrVeN/dDxrK82zqR0mBHdoCQoOzmM1s/W/pjBrUYKur++E2y3P/tO/TvFNd3iinsghnuyPqAgYIyi9ajh8/Ah8/Ft/iFqVh9bHZ7IDC9SoaBPWB/6PTeLurf+eEZq0kAz1p//zvs/47fEg11eRfChfCTZFv1Oiq7RQ/Vs7g+rewBmDckLGvTYR8c6nodr6tQc1tjwERzahQNx2E6nAZUXFeU6H/wR3Nq9lcKa/lolWjNK722iTzCJUnkSEMmANcEU2lyY1NgSdtLDKx/u7Ic0ZVir+tbNEvEc/ze5PYB4Uf8/ccaS+KqiKl1DDR0fAgMBAAECggEADFxdAIhqpUawD5csr0C0v6kyZKd1RZKKw/hECbubEDnS2AnXdcZ0cU1ljARtZzFllWvzkUJbY59u2uQ0/AA67Wj0J9qVZzobsTyrO0fZHm6EfO4GGYiNnfnLhBmxAwDbvWmXGjsnfeD9LlRGK6yqqB7Di6qW91TiT/7VubULTFCLudEx9SUaBmMyj2FYFou62vf0FZz4rut4Vn9wqExi+BPg+vouwkXvTLEEzUld2vhxCUvS6xyycbhE6PvKk53WaSeHwfM/WsT3FvN6u0Jksuiy2v88q3ex6iQzkT2KSmnuV6GCt8sO+ynxlBycWbOONkZDtRs/3P0CZP7pZlWgJQKBgQDCKA1tBGFLpL1E+2qAkVFizWLj1tbQyabeSuzDL/M9wXQqFbUpjuYMeJ2i7NLqSFY/KXFeYfWZATPbOkpcChhciT9b1iF0va4yD8EdCwgudUaM8QUdyUenUDQ7Tg9Z1B5EMwVPZpT53bBbTCzCGie48m2LSzw6iBFXk22V5eUC3QKBgQDAhKITfYvESCY4+8JNezVBArlPyeVyhRrii22G1XFoDTaMTdfo1dXeSE+53bB2W2kJsxCiPWzX5COmt6+7dnKun7LrgW6Dxx9COsoPDXredOkT5rTwTqGPQ47qSqVR4HEo4vGqVDJPen1KNeTLcgJhhR3kYCqkyVW9C0cvQ00KKwKBgQCjNa4LjVoIj2Q3P61IhF/I0WQmqKuNeWXXiV47uewcZqmy0uqmE6SYqS/CDJhELyipEbpmH6ynwNYJnJdzhVzzNMBY/8jB7M0o8gtg0aw/+X+j7NtjLZcZM5eknrrSxTfoFoeJNm9XF58vL/qT9A6eLtQc+tKXVVhWqOy+zDFV1QKBgGqfCkRRG78xVjno9MuEmRHQOGGAJ5stLJ6bKaWrQIuoBbXext0k0LWyxrThmoFyTtIYYfX4nvdSWoxS3viruQgNWkIkBNjmxuEfPhjoFkxt40gqVbthUQjPGa+Yd8bIzuMeDUmQJ+zsoccf4f7qvXbVCqEYwYVSHDH+tQXv+BLRAoGAS869HWTv8HSicZx66HqZ/hXBS5qQWJIdQZwQhkawS1/nSnwJEtrzyalzXPs0/rbuXYZZ+m+djlaaS0cLxPAzTUnkNSPhj9WSJEA7EEihfSlzYbfo8Vu9bqjEU03LveuwBiZoQyDhsMVhT1AaQMu2rT/pToK8A2WsoysQcYU4eZQ=";
        private static readonly int chunkSize = 344;

        //public string DecryptData(string message)
        //{
        //    using (RSA rsa = RSA.Create())
        //    {
        //        byte[] privateKey = Convert.FromBase64String(privateKeyBase64);
        //        byte[] encryptedData = Convert.FromBase64String(message);

        //        rsa.ImportPkcs8PrivateKey(privateKey, out int bytesRead);

        //        byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);

        //        return Encoding.UTF8.GetString(decryptedData);
        //    }
        //}

        public string DecryptData(string ciphertext)
        {
            using (var rsa = RSA.Create())
            {
                byte[] privateKey = Convert.FromBase64String(privateKeyBase64);
                rsa.ImportPkcs8PrivateKey(privateKey, out int bytesRead);

                var chunks = ChunkString(ciphertext, chunkSize);
                var decodedChunks = chunks.Select(s => Convert.FromBase64String(s)).ToList();

                var plaintexts = new List<byte[]>();
                foreach (var chunk in decodedChunks)
                {
                    var decryptedChunk = rsa.Decrypt(chunk, RSAEncryptionPadding.OaepSHA256);
                    plaintexts.Add(decryptedChunk);
                }

                var arrayBuffers = ConcatByteArrays(plaintexts.ToArray());
                return Encoding.UTF8.GetString(arrayBuffers);
            }
        }

        private static byte[] ConcatByteArrays(byte[][] byteArrays)
        {
            var totalLength = byteArrays.Sum(arr => arr.Length);
            var result = new byte[totalLength];
            var offset = 0;
            foreach (var arr in byteArrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }
            return result;
        }


        private static List<string> ChunkString(string str, int chunkSize)
        {
            var chunks = new List<string>();
            for (int i = 0; i < str.Length; i += chunkSize)
            {
                chunks.Add(str.Substring(i, Math.Min(chunkSize, str.Length - i)));
            }
            return chunks;
        }
    }
}
