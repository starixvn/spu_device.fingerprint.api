using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Linq;
using System.Numerics;

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

        public void RunBusinessLogic(string? filePath)
        {
            try
            {
                if (fileReaderService == null)
                {
                    Console.WriteLine("Could not retrieve IFileReaderService.");
                    return;
                }
                if (filePath == null)
                {
                    Console.WriteLine("FilePath is null.");
                    return;
                }

                try
                {
                    string content = fileReaderService.ReadFileContent(filePath);

                    string[] lines = content.Split('\n');

                    var transactionModels = new List<RawTransactionModel>();
                    var listErrorLine = new List<int>();
                    var listErrorMessage = new List<string>();

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
                        catch (JsonException ex)
                        {
                            listErrorLine.Add(i+1);
                            listErrorMessage.Add($"\nError deserializing JSON on Line {i + 1}, message: {ex.Message}\n");
                        }
                    }

                    if (listErrorLine.Count > 0)
                    {
                        Console.WriteLine($"Error list: {string.Join(", ", listErrorLine)}");
                        Console.WriteLine($"\n==== Detail ====");
                        Console.WriteLine(string.Join("\n", listErrorMessage));
                        Console.WriteLine($"\nPlease adjust the data before proceeding!");
                        
                        //do
                        //{
                        //    Console.Write("Do you want to continue insert? (Y/N): ");
                        //} while (Console.ReadKey().Key == ConsoleKey.Y);
                    }
                    else if (transactionModels.Count > 0)
                    {
                        transactionService.InsertRawTransaction(transactionModels, false).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}