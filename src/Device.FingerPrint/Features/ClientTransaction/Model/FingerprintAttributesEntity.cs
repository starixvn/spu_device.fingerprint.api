namespace Device.FingerPrint.Features.ClientTransaction.Model
{
    public class FingerprintAttributesEntity
    {
        public string FingerPrintCode { get; set; }

        public string Attribute { get; set; }

        public bool CheckCanvasDefender { get; set; }

        public bool CheckWebGLDefender { get; set; }
    }
}
