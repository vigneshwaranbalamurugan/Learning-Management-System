using LMSApi.BALLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IEnumerable<IPaymentProvider> _providers;

        public PaymentService(IEnumerable<IPaymentProvider> providers)
        {
            _providers = providers;
        }

        public Task<string> CreateOrderAsync(string providerName, decimal amount, string currency, string receiptId)
        {
            var provider = GetProvider(providerName);
            return provider.CreateOrderAsync(amount, currency, receiptId);
        }

        public bool VerifySignature(string providerName, string orderId, string paymentId, string signature)
        {
            var provider = GetProvider(providerName);
            return provider.VerifySignature(orderId, paymentId, signature);
        }

        private IPaymentProvider GetProvider(string providerName)
        {
            var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
            {
                throw new NotSupportedException($"Payment provider '{providerName}' is not supported.");
            }
            return provider;
        }
    }
}
