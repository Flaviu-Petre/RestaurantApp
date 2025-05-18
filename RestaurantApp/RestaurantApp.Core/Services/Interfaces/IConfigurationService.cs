using RestaurantApp.Core.Models;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task<AppSettings> GetAppSettingsAsync();
        Task UpdateAppSettingsAsync(AppSettings settings);

        // Individual settings
        Task<decimal> GetMenuDiscountPercentageAsync();
        Task<decimal> GetOrderValueForFreeShippingAsync();
        Task<decimal> GetShippingCostAsync();
        Task<int> GetLoyaltyOrderCountAsync();
        Task<int> GetLoyaltyTimePeriodDaysAsync();
        Task<decimal> GetLoyaltyDiscountPercentageAsync();
        Task<decimal> GetLowStockThresholdAsync();
    }
}