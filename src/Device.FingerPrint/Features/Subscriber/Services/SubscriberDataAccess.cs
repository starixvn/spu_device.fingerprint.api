using Fanex.Data.Repository;
using Device.FingerPrint.Features.Subscriber.Criterias;
using Device.FingerPrint.Features.Subscriber.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Fanex.Data;

namespace Device.FingerPrint.Features.Subscriber.Services
{
    public interface ISubscriberDataAccess
    {
        SubscriberEntity GetSubscriberById(int subscriberId);

        SubscriberEntity GetSubscriberByName(string subscriberName);

        IEnumerable<SubscriberEntity> GetSubscriberList();
    }
    public class SubscriberDataAccess : ISubscriberDataAccess
    {
        private readonly IDynamicRepository repository;

        public SubscriberDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<SubscriberEntity> GetSubscriberList()
        {
            var criteria = new GetSubscriberListCriteria();

            return repository.Fetch<SubscriberEntity>(criteria);
        }

        public SubscriberEntity GetSubscriberById(int subscriberId)
        {
            var criteria = new GetSubscriberInfoByIdCriteria
            {
                SubscriberId = subscriberId
            };

            using (var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey()))
            {
                return db.QueryEntity<SubscriberEntity>(criteria);
            }
        }

        public SubscriberEntity GetSubscriberByName(string subscriberName)
        {
            var criteria = new GetSubscriberInfoByNameCriteria
            {
                SubscriberName = subscriberName
            };

            using (var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey()))
            {
                return db.QueryEntity<SubscriberEntity>(criteria);
            }
        }
    }
}
