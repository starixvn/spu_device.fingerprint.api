namespace Device.FingerPrint.Model
{
    public class FingerprintAttributesModel
    {
        public string FingerPrintCode { get; set; }

        public string Attribute { get; set; }

        public bool CheckCanvasDefender { get; set; }

        public bool CheckWebGLDefender { get; set; }
    }
}
