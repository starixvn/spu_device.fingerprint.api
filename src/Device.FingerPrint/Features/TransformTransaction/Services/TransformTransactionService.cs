namespace Device.FingerPrint.Features.TransformTransaction.Services
{

    public interface ITransformTransactionService
    {
        void Execute(int key, int delay);
    }

    public class TransformTransactionService : ITransformTransactionService
    {
        private readonly ITransformTransactionDataAccess transformTransactionDataAccess;

        public TransformTransactionService(ITransformTransactionDataAccess transformTransactionDataAccess)
        {
            this.transformTransactionDataAccess = transformTransactionDataAccess;
        }

        public void Execute(int key, int delay)
        {
            transformTransactionDataAccess.Execute(key, delay);
        }
    }
}
