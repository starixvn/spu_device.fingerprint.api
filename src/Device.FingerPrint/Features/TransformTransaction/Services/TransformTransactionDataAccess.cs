using Device.FingerPrint.Features.TransformTransaction.Criterias;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.TransformTransaction.Services
{
    public interface ITransformTransactionDataAccess
    {
        void Execute(int key, int delay);
    }
    public class TransformTransactionDataAccess : ITransformTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransformTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void Execute(int key, int delay)
        {
            var criteria = new TransformTransactionCriteria
            {
                Key = key,
                Delay = delay
            };

            repository.Execute(criteria);
        }
    }
}
