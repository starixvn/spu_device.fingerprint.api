using Fanex.Data.Repository;
using Device.FingerPrint.Features.Subscriber.Criterias;
using Device.FingerPrint.Features.Subscriber.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Subscriber.Services
{
    public interface ISubscriberDataAccess
    {
        IEnumerable<SubscriberEntity> GetSubscriberById(int subscriberId);
        IEnumerable<SubscriberEntity> GetSubscriberByName(string subscriberName);
        IEnumerable<SubscriberEntity> GetSubscriberList();
    }
    public class SubscriberDataAccess : ISubscriberDataAccess
    {
        private readonly IDynamicRepository repository;

        public SubscriberDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<SubscriberEntity> GetSubscriberById(int subscriberId)
        {
            var criteria = new GetSubscriberInfoByIdCriteria
            {
                SubscriberId = subscriberId
            };

            return repository.Fetch<SubscriberEntity>(criteria);
        }

        public IEnumerable<SubscriberEntity> GetSubscriberByName(string subscriberName)
        {
            var criteria = new GetSubscriberInfoByNameCriteria
            {
                SubscriberName = subscriberName
            };

            return repository.Fetch<SubscriberEntity>(criteria);
        }

        public IEnumerable<SubscriberEntity> GetSubscriberList()
        {
            var criteria = new GetSubscriberListCriteria();

            return repository.Fetch<SubscriberEntity>(criteria);
        }
    }
}
