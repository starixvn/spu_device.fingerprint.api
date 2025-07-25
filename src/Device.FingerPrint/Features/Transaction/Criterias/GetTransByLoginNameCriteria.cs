namespace Device.FingerPrint.Features.Transaction.Criterias
{
    using Fanex.Data;
    using Fanex.Data.Repository;

    public class GetTransByLoginNameCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_SubscriberID")]
        public int SubscriberID { get; set; }

        [SpParam(SqlParamName = "ip_FromDate")]
        public DateTime FromDate { get; set; }

        [SpParam(SqlParamName = "ip_ToDate")]
        public DateTime ToDate { get; set; }

        [SpParam(SqlParamName = "ip_LoginName")]
        public string LoginName { get; set; }

        [SpParam(SqlParamName = "ip_BatchSize")]
        public int BatchSize { get; set; }

        public override string GetSettingKey()
        {
            return "DCS_DC_GetTransactionByLoginName";
        }

        public override bool IsValid()
        {
            return SubscriberID > 0 && !string.IsNullOrEmpty(LoginName) && BatchSize > 0 && FromDate <= ToDate;
        }
    }
}
