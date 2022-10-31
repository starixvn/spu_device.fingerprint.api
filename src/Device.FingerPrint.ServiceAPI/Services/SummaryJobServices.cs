using Device.FingerPrint.Features.SummaryTransaction.Services;
using Sentry;
using System;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ISummaryJobServices
    {
        Task<bool> SummaryServerTransactionDaily(int batchSize);

        Task<bool> SummaryClientTransactionDaily(int batchSize);

        Task<bool> SummaryTransactionDaily(int batchSize);

        Task<bool> SummaryRecoverTypeDaily(int batchSize);
    }

    public class SummaryJobServices : ISummaryJobServices
    {
        private readonly ISummaryTransactionService summaryTransactionService;
        private readonly ISentryClient sentryClient;

        public SummaryJobServices(ISummaryTransactionService summaryTransactionService, ISentryClient sentryClient)
        {
            this.summaryTransactionService = summaryTransactionService;
            this.sentryClient = sentryClient;
        }

        public Task<bool> SummaryServerTransactionDaily(int batchSize)
        {
            try
            {
                summaryTransactionService.SummaryServerTransactionDaily(batchSize);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> SummaryClientTransactionDaily(int batchSize)
        {
            try
            {
                summaryTransactionService.SummaryClientTransactionDaily(batchSize);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> SummaryTransactionDaily(int batchSize)
        {
            try
            {
                summaryTransactionService.SummaryTransactionDaily(batchSize);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> SummaryRecoverTypeDaily(int batchSize)
        {
            try
            {
                summaryTransactionService.SummaryRecoverTypeDaily(batchSize);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
