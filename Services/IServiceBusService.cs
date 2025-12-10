using CreditoAPI.DTOs;

namespace CreditoAPI.Services
{
    public interface IServiceBusService
    {
        Task SendMessageAsync(CreditoDto credito);
        Task<List<CreditoDto>> ReceiveMessagesAsync();
        Task SendAuditMessageAsync(string eventType, string key);
    }
}
