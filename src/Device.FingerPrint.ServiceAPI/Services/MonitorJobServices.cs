using Device.FingerPrint.Features.Monitor.Services;
using Fanex.NAP.SPU.Notification.Services;
using System;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IMonitorJobServices
    {
        public void DailyReport();
        public void MonthlySum();
    }
    public class MonitorJobServices : IMonitorJobServices
    {
        private readonly IAppSettings appSettings;
        private readonly ITransactionReportServices transactionReportServices;

        public MonitorJobServices(IAppSettings appSettings
                                , ITransactionReportServices transactionReportServices)
        {
            this.appSettings = appSettings;
            this.transactionReportServices = transactionReportServices;
        }
        public void DailyReport()
        {
            var currentDate = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy");
            var moreInfo = "";
            ServerTransDailySum(currentDate, ref moreInfo);
            ClientTransDailySum(currentDate, ref moreInfo);
            Notification("Daily Report", "Daily Report Date " + currentDate, moreInfo);
        }

        public void MonthlySum()
        {
            var currentMonth = DateTime.UtcNow.AddHours(7).ToString("MM/yyyy");
            transactionReportServices.MonthlySum(currentMonth);
        }

        private void ServerTransDailySum(string currentDate, ref string moreInfo)
        {
            var listDailySum = transactionReportServices.GetServerTransDailySumList(currentDate);
            moreInfo += "\n\rThe List data ServerTransactionDailySum:";

            foreach (var item in listDailySum)
            {
                moreInfo += "\n\r\tSubscriber: " + item.SubscriberName
                        + ", TotalTrans: " + item.TotalTrans.ToString()
                        + ", TotalValidTrans: " + item.TotalValidTrans.ToString();
            }
        }

        private void ClientTransDailySum(string currentDate, ref string moreInfo)
        {
            var listDailySum = transactionReportServices.GetClientTransDailySumList(currentDate);
            moreInfo += "\n\rThe List data ClientTransactionDailySum:";

            foreach (var item in listDailySum)
            {
                moreInfo += "\n\r\tSubscriber: " + item.SubscriberName
                        + ", TotalTrans: " + item.TotalTrans.ToString()
                        + ", TotalValidTrans: " + item.TotalValidTrans.ToString();
            }
        }

        private void Notification(string jobName, string message, string moreInfo)
        {
            var notification = new NotificationService();

            notification
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken")
                      .Message(new MessageModel(jobName, DateTime.UtcNow.AddHours(7), appSettings.Environment, message, moreInfo))
                      .SendWithDelay(300, true);
        }
    }
}
