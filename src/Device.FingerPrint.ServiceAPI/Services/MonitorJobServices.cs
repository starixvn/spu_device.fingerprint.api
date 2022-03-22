using Device.FingerPrint.Features.Monitor.Services;
using Fanex.NAP.SPU.Notification.Services;
using System;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface IMonitorJobServices
    {
        public void DailyReport();
    }
    public class MonitorJobServices : IMonitorJobServices
    {
        private readonly IAppSettings appSettings;
        private readonly IServerTransDailySumServices serverTransDailySumServices;
        private readonly IClientTransDailySumServices clientTransDailySumServices;

        public MonitorJobServices(IAppSettings appSettings
                                , IServerTransDailySumServices serverTransDailySumServices
                                , IClientTransDailySumServices clientTransDailySumServices)
        {
            this.appSettings = appSettings;
            this.serverTransDailySumServices = serverTransDailySumServices;
            this.clientTransDailySumServices = clientTransDailySumServices;
        }
        public void DailyReport()
        {
            var currentDate = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy");
            var moreInfo = "";
            ServerTransDailySum(currentDate, ref moreInfo);
            ClientTransDailySum(currentDate, ref moreInfo);
            Notification("Daily Report", "Daily Report Date " + currentDate, moreInfo);
        }

        private void ServerTransDailySum(string currentDate, ref string moreInfo)
        {
            var listDailySum = serverTransDailySumServices.GetServerTransDailySumList(currentDate);
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
            var listDailySum = clientTransDailySumServices.GetClientTransDailySumList(currentDate);
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
