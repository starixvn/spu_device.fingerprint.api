using Device.FingerPrint.Features.Monitor.Model;
using Device.FingerPrint.Features.Monitor.Services;
using Device.FingerPrint.Model;
using Fanex.NAP.SPU.Notification.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var moreInfo = new StringBuilder("");
            GetInfoDailyReport(currentDate, ref moreInfo);
            Notification("Daily Report", "Daily Report Date " + currentDate, moreInfo);
        }

        public void MonthlySum()
        {
            var currentMonth = DateTime.UtcNow.AddHours(7).ToString("MM/yyyy");
            transactionReportServices.MonthlySum(currentMonth);
        }

        private void GetInfoDailyReport(string currentDate, ref StringBuilder moreInfo)
        {
            var listDailySum = transactionReportServices.GetServerTransDailySumList(currentDate);

            // Server Daily report
            moreInfo.Append("\n\r");
            moreInfo.Append("The List data ServerTransactionDailySum:");
            var serverTransDaily = ConvertEntityToModel_ServerTrans(listDailySum.Item1);
            foreach (var item in serverTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - Subscriber: " + item.SubscriberName);
                moreInfo.Append(", TotalTrans: " + item.TotalTrans.ToString());
                moreInfo.Append(", TotalValidTrans: " + item.TotalValidTrans.ToString());
            }

            // Client Daily report
            moreInfo.Append("\n\r");
            moreInfo.Append("The List data ClientTransactionDailySum:");
            var clientTransDaily = ConvertEntityToModel_ClientTrans(listDailySum.Item2);
            foreach (var item in clientTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - Subscriber: " + item.SubscriberName);
                moreInfo.Append(", TotalTrans: " + item.TotalTrans.ToString());
                moreInfo.Append(", TotalValidTrans: " + item.TotalValidTrans.ToString());
            }
        }

        private static IEnumerable<ServerTransDailySumModel> ConvertEntityToModel_ServerTrans(IEnumerable<ServerTransDailySumEntity> entity)
        {
            if (entity != null && entity.Any())
            {
                return entity.Select(x => new ServerTransDailySumModel
                {
                    TransDate = x.TransDate,
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName,
                    TotalTrans = x.TotalTrans,
                    TotalValidTrans = x.TotalValidTrans
                });
            }
            return new List<ServerTransDailySumModel>();
        }

        private static IEnumerable<ClientTransDailySumModel> ConvertEntityToModel_ClientTrans(IEnumerable<ClientTransDailySumEntity> entity)
        {
            if (entity != null && entity.Any())
            {
                return entity.Select(x => new ClientTransDailySumModel
                {
                    TransDate = x.TransDate,
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName,
                    TotalTrans = x.TotalTrans,
                    TotalValidTrans = x.TotalValidTrans
                });
            }
            return new List<ClientTransDailySumModel>();
        }

        private void Notification(string jobName, string message, StringBuilder moreInfo)
        {
            var notification = new NotificationService();

            notification
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken")
                      .Message(new MessageModel(jobName, DateTime.UtcNow.AddHours(7), appSettings.Environment, message, moreInfo.ToString()))
                      .SendWithDelay(300, true);
        }
    }
}
