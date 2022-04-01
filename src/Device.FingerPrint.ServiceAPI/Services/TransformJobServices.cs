using Device.FingerPrint.Features.TransformTransaction.Services;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ITransformJobServices
    {
        public void Execute(int key, int delay);
    }

    public class TransformJobServices : ITransformJobServices
    {
        private readonly ITransformTransactionService transformTransactionService;

        public TransformJobServices(ITransformTransactionService transformTransactionService)
        {
            this.transformTransactionService = transformTransactionService;
        }

        public void Execute(int key, int delay)
        {
            transformTransactionService.Execute(key, delay);
        }
    }
}
