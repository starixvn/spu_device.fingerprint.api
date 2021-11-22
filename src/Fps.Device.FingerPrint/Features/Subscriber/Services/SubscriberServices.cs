using FPS.Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fps.Device.FingerPrint.Features.Subscriber.Services
{
    public interface ISubscriberServices
    {
        SubscriberModel GetSubscriber(int subscriberId);
    }

    public class SubscriberServices : ISubscriberServices
    {

        private readonly ISubscriberDataAccess subscriberDataAccess;

        public SubscriberServices(ISubscriberDataAccess subscriberDataAccess)
        {
            this.subscriberDataAccess = subscriberDataAccess;
        }

        public SubscriberModel GetSubscriber(int subscriberId)
        {
            var subcriber = subscriberDataAccess.GetSubscriber(subscriberId);

            if(subcriber != null && subcriber.SubscriberId != 0)
            {
                return new SubscriberModel
                {
                    SubscriberId = subcriber.SubscriberId,
                    SubscriberName = subcriber.SubscriberName,
                    SubscriberStatus = subcriber.SubscriberStatus,
                    DCSStatus = subcriber.DCSStatus,
                    CreatedDate = subcriber.CreatedDate,
                    SubscriberType = subcriber.SubscriberType
                };
            }

            return new SubscriberModel();
        }
    }
}
