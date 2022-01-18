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
        IEnumerable<SubscriberEntity> GetSubscriber(int subscriberId);
        IEnumerable<SubscriberEntity> GetSubscriberList();
    }
    public class SubscriberDataAccess : ISubscriberDataAccess
    {
        private readonly IDynamicRepository repository;

        public SubscriberDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<SubscriberEntity> GetSubscriber(int subscriberId)
        {
            var criteria = new GetSubscriberInfoCriteria
            {
                SubscriberId = subscriberId
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
