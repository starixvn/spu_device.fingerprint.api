using Device.FingerPrint.Features.Monitor.Model;
using Device.FingerPrint.Features.Monitor.Services;
using Device.FingerPrint.Model;
using Fanex.NAP.SPU.Notification.Services;
using Sentry;
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

        void TestSentryCaptureMessage(string message);

        void TestSentryCaptureException();

        void TestNotificationTelegram(string message);
    }

    public class MonitorJobServices : IMonitorJobServices
    {
        private readonly IAppSettings appSettings;
        private readonly ITransactionReportServices transactionReportServices;
        private readonly ISentryClient sentryClient;

        public MonitorJobServices(IAppSettings appSettings
                                , ITransactionReportServices transactionReportServices
                                , ISentryClient sentryClient)
        {
            this.appSettings = appSettings;
            this.transactionReportServices = transactionReportServices;
            this.sentryClient = sentryClient;
        }

        public Task<bool> DailyReport()
        {
            try
            {
                var currentDate = DateTime.Now;

                // Get data of last day
                var lastDay = currentDate.AddDays(-1);
                var strDate = lastDay.ToString("MM/dd/yyyy");
                var moreInfo = GetInfoReport(lastDay, lastDay);

                Notification("Daily Summary Report", "Daily Summary Report " + strDate, moreInfo);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> MonthlyReport()
        {
            try
            {
                var currentDate = DateTime.Now;

                // Get data of last month
                var lastDayOfMonth = currentDate.AddDays(-1);
                var firstDayOfMonth = new DateTime(lastDayOfMonth.Year, lastDayOfMonth.Month, 1);

                var currentMonth = lastDayOfMonth.ToString("MM/yyyy");
                var moreInfo = GetInfoReport(firstDayOfMonth, lastDayOfMonth);

                Notification("Monthly Summary Report", "Monthly Summary Report " + currentMonth, moreInfo);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private StringBuilder GetInfoReport(DateTime fromDate, DateTime toDate)
        {
            var moreInfo = new StringBuilder("");
            var transactionSumList = transactionReportServices.GetTransactionSumList(fromDate, toDate);

            // Server summary
            moreInfo.Append("\n\r");
            moreInfo.Append("ServerTransaction:");
            var serverTransDaily = TranformData(transactionSumList.Item1);
            foreach (var item in serverTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - " + item.SubscriberName);
                moreInfo.Append(": " + item.TotalTrans.ToString());
                moreInfo.Append(" (" + item.TotalValidTrans.ToString() + ")");
            }

            // Client summary
            moreInfo.Append("\n\r");
            moreInfo.Append("ClientTransaction:");
            var clientTransDaily = TranformData(transactionSumList.Item2);
            foreach (var item in clientTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - " + item.SubscriberName);
                moreInfo.Append(": " + item.TotalTrans.ToString());
                moreInfo.Append(" (" + item.TotalValidTrans.ToString() + ")");
            }

            // Transaction summary
            moreInfo.Append("\n\r");
            moreInfo.Append("Transaction:");
            var transaction = TranformData(transactionSumList.Item3);
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

        public void TestSentryCaptureMessage(string message)
        {
            sentryClient.CaptureMessage(message);
        }

        public void TestSentryCaptureException()
        {
            try
            {
                ThrowExeption();
            } catch(Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
            ThrowExeption();
        }

        private void ThrowExeption()
        {
            throw new NotImplementedException();
        }

        public void TestNotificationTelegram(string message)
        {
            var notification = new NotificationService();
            var moreInfo = new StringBuilder("TestNotificationTelegram! \nMessage: " + message);
            sentryClient.CaptureMessage(moreInfo.ToString());

            notification
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken", appSettings.TelegramProxy)
                      .Message(new MessageModel("TestNotificationTelegram", DateTime.Now, appSettings.Environment, message, moreInfo.ToString()))
                      .SendWithDelay(1, true);

        }
    }
}
