namespace FPS.RawTransaction.Insert.Services
{
    public interface IFileReaderService
    {
        string ReadFileContent(string filePath);
    }

    public class FileReaderService : IFileReaderService
    {
        public string ReadFileContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                return $"Tệp tin {filePath} không tồn tại.";
            }
        }
    }
}
