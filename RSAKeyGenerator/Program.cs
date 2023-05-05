//// See https://aka.ms/new-console-template for more information
//using RSAKeyGenerator;
//using System.Security.Cryptography;

//var rsa = RSA.Create(2048);

//// Save private key to PEM file
//var privateKey = rsa.ExportRSAPrivateKey();
//var privateKeyPem = ExportPrivateKeyToPem(privateKey);
//File.WriteAllText("privateKey.pem", privateKeyPem);

//// Save public key to PEM file
//var publicKey = rsa.ExportRSAPublicKey();
//var publicKeyPem = ExportPublicKeyToPem(publicKey);
//File.WriteAllText("publicKey.pem", publicKeyPem);

//Console.WriteLine("Private and public key pairs saved to PEM files.");

//static string ExportPrivateKeyToPem(byte[] privateKey)
//{
//    var privateKeyBuilder = new PemBuilder(privateKey, "RSA PRIVATE KEY");
//    return privateKeyBuilder.Build();
//}

//static string ExportPublicKeyToPem(byte[] publicKey)
//{
//    var publicKeyBuilder = new PemBuilder(publicKey, "PUBLIC KEY");
//    return publicKeyBuilder.Build();
//}

using System.Security.Cryptography;
using System.Text;

var publicKeyBase64 = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCSAqGk0C7F7lvZe5ozeyTIroJXrVeN/dDxrK82zqR0mBHdoCQoOzmM1s/W/pjBrUYKur++E2y3P/tO/TvFNd3iinsghnuyPqAgYIyi9ajh8/Ah8/Ft/iFqVh9bHZ7IDC9SoaBPWB/6PTeLurf+eEZq0kAz1p//zvs/47fEg11eRfChfCTZFv1Oiq7RQ/Vs7g+rewBmDckLGvTYR8c6nodr6tQc1tjwERzahQNx2E6nAZUXFeU6H/wR3Nq9lcKa/lolWjNK722iTzCJUnkSEMmANcEU2lyY1NgSdtLDKx/u7Ic0ZVir+tbNEvEc/ze5PYB4Uf8/ccaS+KqiKl1DDR0fAgMBAAECggEADFxdAIhqpUawD5csr0C0v6kyZKd1RZKKw/hECbubEDnS2AnXdcZ0cU1ljARtZzFllWvzkUJbY59u2uQ0/AA67Wj0J9qVZzobsTyrO0fZHm6EfO4GGYiNnfnLhBmxAwDbvWmXGjsnfeD9LlRGK6yqqB7Di6qW91TiT/7VubULTFCLudEx9SUaBmMyj2FYFou62vf0FZz4rut4Vn9wqExi+BPg+vouwkXvTLEEzUld2vhxCUvS6xyycbhE6PvKk53WaSeHwfM/WsT3FvN6u0Jksuiy2v88q3ex6iQzkT2KSmnuV6GCt8sO+ynxlBycWbOONkZDtRs/3P0CZP7pZlWgJQKBgQDCKA1tBGFLpL1E+2qAkVFizWLj1tbQyabeSuzDL/M9wXQqFbUpjuYMeJ2i7NLqSFY/KXFeYfWZATPbOkpcChhciT9b1iF0va4yD8EdCwgudUaM8QUdyUenUDQ7Tg9Z1B5EMwVPZpT53bBbTCzCGie48m2LSzw6iBFXk22V5eUC3QKBgQDAhKITfYvESCY4+8JNezVBArlPyeVyhRrii22G1XFoDTaMTdfo1dXeSE+53bB2W2kJsxCiPWzX5COmt6+7dnKun7LrgW6Dxx9COsoPDXredOkT5rTwTqGPQ47qSqVR4HEo4vGqVDJPen1KNeTLcgJhhR3kYCqkyVW9C0cvQ00KKwKBgQCjNa4LjVoIj2Q3P61IhF/I0WQmqKuNeWXXiV47uewcZqmy0uqmE6SYqS/CDJhELyipEbpmH6ynwNYJnJdzhVzzNMBY/8jB7M0o8gtg0aw/+X+j7NtjLZcZM5eknrrSxTfoFoeJNm9XF58vL/qT9A6eLtQc+tKXVVhWqOy+zDFV1QKBgGqfCkRRG78xVjno9MuEmRHQOGGAJ5stLJ6bKaWrQIuoBbXext0k0LWyxrThmoFyTtIYYfX4nvdSWoxS3viruQgNWkIkBNjmxuEfPhjoFkxt40gqVbthUQjPGa+Yd8bIzuMeDUmQJ+zsoccf4f7qvXbVCqEYwYVSHDH+tQXv+BLRAoGAS869HWTv8HSicZx66HqZ/hXBS5qQWJIdQZwQhkawS1/nSnwJEtrzyalzXPs0/rbuXYZZ+m+djlaaS0cLxPAzTUnkNSPhj9WSJEA7EEihfSlzYbfo8Vu9bqjEU03LveuwBiZoQyDhsMVhT1AaQMu2rT/pToK8A2WsoysQcYU4eZQ=";
var encryptedDataBase64 = "jZFxvVaWet9NrkP+zFa2jiXwzr04xWW3PoHN0cPOoOWXfOHOcm3oZYT54LEHI3kaeZHXGwrpkKaU+pWKc94vbX0lbf3RvDJ8MPiBTFu8vw/X/Q3lhDuc4Z+40Nd0X+D4XCSs7EullWKF2WZaVGgdci1c/hxY9IQMSRvdVgtJL6+65Wn71rZW+85FcJ82kYqWUTf1cG6De7L7ZDc5PVS/oHSieEGqOiLW/wsx9ep106Q2AhZG/Fq7/M94xSW9MKC3l0EVfdxoGNagzmuSN0ClJjaS6GSaKErWm4023sS7UaWidRn6FX1HH+CDWMpGRHQfv0WAbz2n6pd+dSJNkllrDQ==";

static string DecryptData(byte[] encryptedData, byte[] privateKey)
{
    using (RSA rsa = RSA.Create())
    {
        rsa.ImportPkcs8PrivateKey(privateKey, out int bytesRead);

        byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);

        return Encoding.UTF8.GetString(decryptedData);
    }
}

byte[] encryptedData = Convert.FromBase64String(encryptedDataBase64); // your encrypted data here
byte[] privateKey = Convert.FromBase64String(publicKeyBase64); // your private key here

string decryptedData = DecryptData(encryptedData, privateKey);

Console.WriteLine(decryptedData);

