using Fanex.Data.Repository;
using Fps.Device.FingerPrint.Features.Subscriber.Criterias;
using Fps.Device.FingerPrint.Features.Subscriber.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fps.Device.FingerPrint.Features.Subscriber.Services
{
    public interface ISubscriberDataAccess
    {
        SubscriberEntity GetSubscriber(int subscriberId);
    }
    public class SubscriberDataAccess : ISubscriberDataAccess
    {
        private readonly IDynamicRepository repository;

        public SubscriberDataAccess()
        {
            repository = new DynamicRepository();
        }

        public SubscriberEntity GetSubscriber(int subscriberId)
        {
            var criteria = new GetSubscriberInfoCriteria
            {
                SubscriberId = subscriberId
            };


            return repository.Get<SubscriberEntity>(criteria);
        }
    }
}
