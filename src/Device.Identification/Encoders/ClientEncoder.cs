namespace Device.Identification.Encoders
{
    public class ClientEncoder
    {
        public const string _clientEncryptKey = "PI";

        public static string Decrypt(string encryptedText)
        {
            return Decrypt(encryptedText, _clientEncryptKey);
        }

        private static string Decrypt(string encryptedText, string key)
        {
            var decryptedText = Base64.Decode(encryptedText);
            decryptedText = Rc4.Decrypt(key, decryptedText);
            return decryptedText;
        }
    }
}