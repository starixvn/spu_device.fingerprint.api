using FPS.RawTransaction.Insert.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    private readonly IFileReaderService _fileReaderService;

    public Program(IFileReaderService fileReaderService)
    {
        _fileReaderService = fileReaderService;
    }

    public void Run(string? filePath)
    {
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

        //var subscribers = subscriberServices.GetSubscriberList();

        Console.ReadLine();
    }

    static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IFileReaderService, FileReaderService>();
        services.AddSingleton<Program>();

        var serviceProvider = services.BuildServiceProvider();
        var program = serviceProvider.GetService<Program>();

        program?.Run(configuration["FilePath"]);
    }
}