using Device.Identification.JsonConverter;
using Fanex.Data.Repository;
using Newtonsoft.Json;

namespace Device.Identification.Criterias
{
    public class InsertDeviceCriteria : NonQueryCommand
    {
        #region Device

        /// <summary>
        /// Gets or sets device identifier.
        /// </summary>               
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets device hash code, decide 2 devices are equal all information.
        /// </summary>        
        public string DeviceCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets navigator information, hashed.
        /// </summary>        
        public string Navigator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets plugins information, hashed.
        /// </summary>        
        public string Plugins
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets mimetypes information, hashed.
        /// </summary>        
        public string MimeTypes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets userAgent information, hashed.
        /// </summary>        
        public string UserAgent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets screen size Width-Height.
        /// </summary>        
        public string Screen
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets monitor.
        /// </summary>
        [JsonConverter(typeof(MonitorConverter))]
        public long? Monitor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets language information, hashed.
        /// </summary>        
        public string Language
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Cookie enabled information, hashed.
        /// </summary>        
        public bool CookieEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Do Not Track information, hashed.
        /// </summary>        
        public bool DoNotTrack
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Super Cookies are available such  as {LocalStorage, SessionStorage, etc... } information, hashed.
        /// </summary>        
        public string SuperCookies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets GPU Code, hashed.
        /// </summary>                
        public string GpuCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets WebGL Code, hashed.
        /// </summary>                
        public string WebGL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets installed fonts, hashed.
        /// </summary>                
        public string Fonts
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets time-zone of device.
        /// </summary>        
        public int? TimeZone
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets system time-offset with server in minutes of device.
        /// </summary>        
        public int? SystemTimeOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets browser name
        /// </summary>
        public string Browser
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Device Type
        /// </summary>
        public short DeviceType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Accept
        /// </summary>
        public string Accept
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets Headers
        /// </summary>
        public string Headers
        {
            get;
            set;
        }

        ///// <summary>
        ///// Gets or sets created date.
        ///// </summary>
        //[JsonIgnore]
        //public DateTime? CreatedDate
        //{
        //    get;
        //    set;
        //}

        #endregion

        #region IP log
        /// <summary>
        /// Gets or sets most reliable IP.
        /// </summary>
        /// <value>
        /// The client's IP.
        /// </value>
        [JsonConverter(typeof(IpConverter))]
        public string Ip
        {
            get;
            set;
        }

        public string LocalIPs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets IPs recorded from header request.
        /// </summary>
        /// <value>
        /// The remote IPs.
        /// </value>        
        public string RemoteIPs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets IP that request to get device detector script.
        /// </summary>
        /// <value>
        /// The script requested IP.
        /// </value>
        [JsonConverter(typeof(IpConverter))]
        public string ScriptRequestedIp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets IP that subscriber server seen via user action request.
        /// </summary>
        /// <value>
        /// The subscriber recorded IP.
        /// </value>
        [JsonConverter(typeof(IpConverter))]
        public string SubscriberRecordedIp
        {
            get;
            set;
        }

        #endregion

        #region Domain log
        public string DomainName
        {
            get;
            set;
        }

        public bool? DomainSameReferrer
        {
            get;
            set;
        }
        #endregion

        public override string GetSettingKey()
        {
            return "FPS_DI_DeviceIns_FOR_FP";
        }

        /// <summary>
        /// Check data's constrains.
        /// </summary>
        /// <returns>True: valid; False: invalid.</returns>
        public override bool IsValid()
        {
            return true;
        }
    }
}