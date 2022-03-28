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

        public void MonthlyReport();

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
            var mdyDate = DateTime.UtcNow.AddHours(7).ToString("MM/dd/yyyy");
            var moreInfo = new StringBuilder("");
            moreInfo = GetInfoDailyReport(mdyDate, moreInfo);
            Notification("Daily Report", "Daily Report " + mdyDate, moreInfo);
        }

        public void MonthlyReport()
        {
            var currentMonth = DateTime.UtcNow.AddHours(7).ToString("MM/yyyy");
            var moreInfo = new StringBuilder("");
            moreInfo = GetInfoMonthlyReport(currentMonth, moreInfo);
            Notification("Monthly Report", "Monthly Report " + currentMonth, moreInfo);
        }

        public void MonthlySum()
        {
            var currentMonth = DateTime.UtcNow.AddHours(7).ToString("MM/yyyy");
            transactionReportServices.MonthlySum(currentMonth);
        }

        private StringBuilder GetInfoDailyReport(string mdyDate, StringBuilder moreInfo)
        {
            var listDailySum = transactionReportServices.GetTransactionDailySumList(mdyDate);

            // Server Daily report
            moreInfo.Append("\n\r");
            moreInfo.Append("The List data ServerTransactionDailySum:");
            var serverTransDaily = TranformData(listDailySum.Item1);
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
            var clientTransDaily = TranformData(listDailySum.Item2);
            foreach (var item in clientTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - Subscriber: " + item.SubscriberName);
                moreInfo.Append(", TotalTrans: " + item.TotalTrans.ToString());
                moreInfo.Append(", TotalValidTrans: " + item.TotalValidTrans.ToString());
            }

            return moreInfo;
        }

        private StringBuilder GetInfoMonthlyReport(string currentMonth, StringBuilder moreInfo)
        {
            var listMonthlySum = transactionReportServices.GetTransactionMonthlySumList(currentMonth);

            // Server Daily report
            moreInfo.Append("\n\r");
            moreInfo.Append("The List data ServerTransactionMonthlySum:");
            var serverTransDaily = TranformData(listMonthlySum.Item1);
            foreach (var item in serverTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - Subscriber: " + item.SubscriberName);
                moreInfo.Append(", TotalTrans: " + item.TotalTrans.ToString());
                moreInfo.Append(", TotalValidTrans: " + item.TotalValidTrans.ToString());
            }

            // Client Daily report
            moreInfo.Append("\n\r");
            moreInfo.Append("The List data ClientTransactionMonthlySum:");
            var clientTransDaily = TranformData(listMonthlySum.Item2);
            foreach (var item in clientTransDaily)
            {
                moreInfo.Append("\n\r");
                moreInfo.Append(" - Subscriber: " + item.SubscriberName);
                moreInfo.Append(", TotalTrans: " + item.TotalTrans.ToString());
                moreInfo.Append(", TotalValidTrans: " + item.TotalValidTrans.ToString());
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
                      .Config("0A40aOc7CtpzjbBSJu8KNWQRoXyjRvf9", "-506613866", "https://bot.nexdev.net/skynex/api/messages/forwardwithtoken")
                      .Message(new MessageModel(jobName, DateTime.UtcNow.AddHours(7), appSettings.Environment, message, moreInfo.ToString()))
                      .SendWithDelay(300, true);
        }
    }
}
