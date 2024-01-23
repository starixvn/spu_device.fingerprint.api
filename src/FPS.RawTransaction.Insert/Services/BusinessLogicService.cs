using Device.FingerPrint.Features.Subscriber.Services;

namespace FPS.RawTransaction.Insert.Services
{
    public class BusinessLogicService
    {
        private readonly IFileReaderService _fileReaderService;
        private readonly ISubscriberServices _subscriberServices;

        public BusinessLogicService(IFileReaderService fileReaderService, ISubscriberServices subscriberServices)
        {
            _fileReaderService = fileReaderService;
            _subscriberServices = subscriberServices;
        }

        public void RunBusinessLogic(string? filePath)
        {
            var subscribers = _subscriberServices.GetSubscriberByName("Alpha");
            if (_fileReaderService != null)
            {
                if (filePath != null)
                {
                    string content = _fileReaderService.ReadFileContent(filePath);

                    Console.WriteLine("\nNội dung của tệp tin:\n");
                    Console.WriteLine(content);
                }
                else
                {
                    Console.WriteLine("FilePath is null.");
                }
            }
            else
            {
                Console.WriteLine("Could not retrieve IFileReaderService.");
            }

            Console.ReadLine();
        }
    }
}