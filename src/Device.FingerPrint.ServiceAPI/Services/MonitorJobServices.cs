using Device.FingerPrint.Features.Monitor.Model;
using Device.FingerPrint.Features.Monitor.Services;
using Device.FingerPrint.Model;
using Fanex.Logging;
using Fanex.NAP.SPU.Notification.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IMonitorJobServices
    {
        Task<bool> DailyReport();

        Task<bool> MonthlyReport();

        Task<bool> TestSentryCaptureMessage(string message);

        void TestNotificationTelegram(string message);
    }

    public class MonitorJobServices : IMonitorJobServices
    {
        private readonly IAppSettings appSettings;
        private readonly ITransactionReportServices transactionReportServices;
        private readonly ILogger _logger;

        public MonitorJobServices(IAppSettings appSettings
                                , ITransactionReportServices transactionReportServices
                                , ILogger logger)
        {
            this.appSettings = appSettings;
            this.transactionReportServices = transactionReportServices;
            _logger = logger;
        }

        public Task<bool> DailyReport()
        {
            var currentDate = DateTime.Now;

            // Get data of last day
            var lastDay = currentDate.AddDays(-1);
            var strDate = lastDay.ToString("MM/dd/yyyy");

            var moreInfo = GetInfoReport(lastDay, lastDay);
            Notification("Daily Summary Report", "Daily Summary Report " + strDate, moreInfo);

            return Task.FromResult(true);
        }

        public Task<bool> MonthlyReport()
        {
            var currentDate = DateTime.Now;

            // Get data of last month
            var lastDayOfMonth = currentDate.AddDays(-1);
            var firstDayOfMonth = new DateTime(lastDayOfMonth.Year, lastDayOfMonth.Month, 1);

            var currentMonth = lastDayOfMonth.ToString("MM/yyyy");
            var moreInfo = GetInfoReport(firstDayOfMonth, lastDayOfMonth);

            Notification("Monthly Summary Report", "Monthly Summary Report " + currentMonth, moreInfo);

            return Task.FromResult(true);
        }

        private StringBuilder GetInfoReport(DateTime fromDate, DateTime toDate)
        {
            var moreInfo = new StringBuilder("");
            var transactionSumList = transactionReportServices.GetTransactionSumList(fromDate, toDate);

            // Transaction summary
            moreInfo.Append("\n\r");
            moreInfo.Append("Transaction:");
            var transaction = TranformData(transactionSumList);
            foreach (var item in transaction)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - " + item.SubscriberName);
                moreInfo.Append(": " + item.TotalTrans.ToString());
            }

            return moreInfo;
        }

        private static IEnumerable<TransactionReportModel> TranformData(IEnumerable<TransactionReportEntity> entity)
        {
            if (entity != null && entity.Any())
            {
                return entity.Select(x => new TransactionReportModel
                {
                    TransDate = x.TransDate,
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName,
                    TotalTrans = x.TotalTrans,
                    TotalValidTrans = x.TotalValidTrans
                });
            }
            return new List<TransactionReportModel>();
        }

        private void Notification(string jobName, string message, StringBuilder moreInfo)
        {
            var notification = new NotificationService();

            notification
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken", appSettings.TelegramProxy)
                      .Message(new MessageModel(jobName, DateTime.Now, appSettings.Environment, message, moreInfo.ToString()))
                      .SendWithDelay(1, true);
        }

        public Task<bool> TestSentryCaptureMessage(string message)
        {
            _logger.Info(message);
            return Task.FromResult(true);
        }

        public void TestNotificationTelegram(string message)
        {
            var notification = new NotificationService();
            var moreInfo = new StringBuilder("TestNotificationTelegram! \nMessage: " + message);

            notification
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken", appSettings.TelegramProxy)
                      .Message(new MessageModel("TestNotificationTelegram", DateTime.Now, appSettings.Environment, message, moreInfo.ToString()))
                      .SendWithDelay(1, true);

        }
    }
}
