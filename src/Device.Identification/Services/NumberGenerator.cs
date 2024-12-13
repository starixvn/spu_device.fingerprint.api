namespace Device.Identification.Services
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    public class NumberGenerator
    {
        private readonly int[] _arrRedisDbIndex;
        private readonly int _arrRedisDbLength;

        private int _currentIndex = 0;
        private readonly object _lock = new();

        public NumberGenerator(IAppSettings appSettings)
        {
            var redisDbIndexSetting = appSettings.RedisDbIndex;
            if (string.IsNullOrEmpty(redisDbIndexSetting))
            {
                throw new InvalidOperationException("RedisDbIndex configuration is not set.");
            }

            _arrRedisDbIndex = redisDbIndexSetting.Split(',')
                                                   .Select(index =>
                                                   {
                                                       if (int.TryParse(index, out var parsed))
                                                       {
                                                           return parsed;
                                                       }
                                                       throw new FormatException($"Invalid Redis DB index: {index}");
                                                   })
                                                   .ToArray();

            _arrRedisDbLength = _arrRedisDbIndex.Length;

            if (_arrRedisDbLength == 0)
            {
                throw new InvalidOperationException("RedisDbIndex cannot be empty.");
            }
        }

        public int GetNextNumber()
        {
            lock (_lock)
            {
                int number = _arrRedisDbIndex[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _arrRedisDbLength;
                return number;
            }
        }
    }

}
