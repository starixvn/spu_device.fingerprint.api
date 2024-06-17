namespace FPS.Client.NetStandard.Validator
{
    /// <summary>
    /// Class InputValidator.
    /// </summary>
    internal class InputValidator
    {
        public static bool IsValidAccount(string account)
        {
            return string.IsNullOrEmpty(account) ? false : true;
        }

        public static bool IsValidDevice(string device)
        {
            return string.IsNullOrEmpty(device) ? false : true;
        }

        public static bool IsValidAction(string action)
        {
            return string.IsNullOrEmpty(action) ? false : true;
        }

        public static bool IsValidSubscriber(string subscriber)
        {
            return string.IsNullOrEmpty(subscriber) ? false : true;
        }
    }
}
