using Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Device.FingerPrint.Features.Subscriber.Services
{
    public interface ISubscriberServices
    {
       IEnumerable<SubscriberModel> GetSubscriberById(int subscriberId);
       IEnumerable<SubscriberModel> GetSubscriberByName(string subscriberName);
       IEnumerable<SubscriberModel> GetSubscriberList();
    }

    public class SubscriberServices : ISubscriberServices
    {

        private readonly ISubscriberDataAccess subscriberDataAccess;

        public SubscriberServices(ISubscriberDataAccess subscriberDataAccess)
        {
            this.subscriberDataAccess = subscriberDataAccess;
        }

        public IEnumerable<SubscriberModel> GetSubscriberById(int subscriberId)
        {
            var subcribers = subscriberDataAccess.GetSubscriberById(subscriberId);

            if(subcribers != null && subcribers.Any())
            {
                return subcribers.Select(x => new SubscriberModel {
                    SubscriberId  = x.SubscriberId,
                    SubscriberName =x.SubscriberName
                });
            }

            return new List<SubscriberModel>();
        }

        public IEnumerable<SubscriberModel> GetSubscriberByName(string subscriberName)
        {
            var subcribers = subscriberDataAccess.GetSubscriberByName(subscriberName);

            if (subcribers != null && subcribers.Any())
            {
                return subcribers.Select(x => new SubscriberModel
                {
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName
                });
            }

            return new List<SubscriberModel>();
        }

        public IEnumerable<SubscriberModel> GetSubscriberList()
        {
            var subcribers = subscriberDataAccess.GetSubscriberList();

            if (subcribers != null && subcribers.Any())
            {
                return subcribers.Select(x => new SubscriberModel
                {
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName
                });
            }

            return new List<SubscriberModel>();
        }
    }
}
