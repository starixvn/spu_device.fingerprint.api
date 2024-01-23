using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;

namespace FPS.RawTransaction.Insert.Services
{
    public class BusinessLogicService
    {
        private readonly IFileReaderService fileReaderService;
        private readonly ITransactionService transactionService;

        public BusinessLogicService(IFileReaderService fileReaderService, ITransactionService transactionService)
        {
            this.fileReaderService = fileReaderService;
            this.transactionService = transactionService;
        }

        public async void RunBusinessLogic(string? filePath)
        {
            try
            {
                if (fileReaderService != null)
                {
                    if (filePath != null)
                    {
                        string content = fileReaderService.ReadFileContent(filePath);

                        string[] lines = content.Split('\n');

                        var transactionModels = new List<RawTransactionModel>();
                        for (int i = 0; i < lines.Length; i++)
                        {
                            try
                            {
                                string line = lines[i];
                                var transactionModel = JsonConvert.DeserializeObject<RawTransactionModel>(line);
                                if (transactionModel != null)
                                {
                                    transactionModels.Add(transactionModel);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"\nError Line {i + 1}, message: {ex.Message}\n\n");
                            }
                        }

                        if (transactionModels.Count > 0)
                        {
                            await transactionService.InsertRawTransaction(transactionModels, false);
                        }
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error: ${ex.Message}");
            }
        }
    }
}