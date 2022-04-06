using Device.FingerPrint.Features.UserAgent.Criterias;
using Device.FingerPrint.Features.UserAgent.Model;
using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.UserAgent.Services
{
    public interface IUserAgentDataAccess
    {
        UserAgentEntity GetUserAgent(int batchSize, int offsetId);
    }

    public class UserAgentDataAccess : IUserAgentDataAccess
    {
        private readonly IDynamicRepository repository;

        public UserAgentDataAccess()
        {
            repository = new DynamicRepository();
        }

        public UserAgentEntity GetUserAgent(int batchSize, int offsetId)
        {
            var criteria = new GetUserAgentCriteria
            {
                Size = batchSize,
                OffsetId = offsetId
            };

            using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
            return db.QueryEntity<UserAgentEntity>(criteria);
        }
    }
}
