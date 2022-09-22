using Device.FingerPrint.Features.Transaction.Model;

namespace Device.FingerPrint.ServiceAPI.Models
{
    public class TransactionEvolutionAttributesModel
    {
        public TransactionEvolutionAttributesEntity _old { get; set; }

        public TransactionEvolutionAttributesEntity _new { get; set; }
    }
}
