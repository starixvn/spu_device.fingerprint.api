using Fps.Device.FingerPrint.Features.Transaction.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IRecoverDeviceServices
    {
        public void RecoverDevice(int batchSize);

    }

    public class RecoverDeviceServices: IRecoverDeviceServices
    {
        private readonly ITransactionService transactionService;

        public RecoverDeviceServices(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        public void RecoverDevice(int batchSize)
        {
            transactionService.RecoverDevice(batchSize);
        }
    }
}
