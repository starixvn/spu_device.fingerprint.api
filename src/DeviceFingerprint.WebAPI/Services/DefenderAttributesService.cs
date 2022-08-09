using Device.FingerPrint.Model;

namespace Device.Fingerprint.WebAPI.Services
{
    public class DefenderAttributesService
    {
        public string GenerateAddonCode(Models.DefenderAttributesModel model)
        {
            if(model != null)
            {
                return (model.CheckCanvasDefender ? "1" : "0")
                    + (model.CheckWebGLDefender ? "1" : "0")
                    + (model.CheckFontDefender ? "1" : "0")
                    + (model.CheckAudioContextDefender ? "1" : "0");
            }
            return "";
        }

        public DefenderAttributesModel TransformDefenderAttributes(Models.DefenderAttributesModel model)
        {
            if(model != null)
            {
                return new DefenderAttributesModel()
                {
                    CheckCanvasDefender = model.CheckCanvasDefender,
                    CheckWebGLDefender = model.CheckWebGLDefender,
                    CheckFontDefender = model.CheckFontDefender,
                    CheckAudioContextDefender = model.CheckAudioContextDefender
                };
            }
            return new DefenderAttributesModel();
        }
    }
}
