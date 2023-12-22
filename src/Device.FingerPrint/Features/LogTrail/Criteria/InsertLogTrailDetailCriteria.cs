namespace Device.FingerPrint.Features.LogTrail.Criteria
{
    using Fanex.Data.Repository;
    using Fanex.Data;

    public class InsertLogTrailDetailCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_LogTrailJson")]
        public string RawLogEvent { get; set; }

        public override string GetSettingKey()
        {
            return "DCS_DT_LogTrail_Detail_Insert";
        }

        public override bool IsValid() => !string.IsNullOrEmpty(RawLogEvent);
    }
}
