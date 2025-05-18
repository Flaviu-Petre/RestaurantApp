using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestaurantApp.Core.Models;
using RestaurantApp.Core.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RestaurantApp.Core.Services.Implementations
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _configFilePath;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        }

        public async Task<AppSettings> GetAppSettingsAsync()
        {
            return await Task.FromResult(new AppSettings
            {
                MenuDiscountPercentage = GetMenuDiscountPercentageAsync().Result,
                OrderValueForFreeShipping = GetOrderValueForFreeShippingAsync().Result,
                ShippingCost = GetShippingCostAsync().Result,
                LoyaltyOrderCount = GetLoyaltyOrderCountAsync().Result,
                LoyaltyTimePeriodDays = GetLoyaltyTimePeriodDaysAsync().Result,
                LoyaltyDiscountPercentage = GetLoyaltyDiscountPercentageAsync().Result,
                LowStockThreshold = GetLowStockThresholdAsync().Result
            });
        }

        public async Task UpdateAppSettingsAsync(AppSettings settings)
        {
            // For simplicity, we're not actually updating the appsettings.json file
            // This would require more complex code to modify a JSON file
            // In a real-world scenario, you might store settings in the database

            // For now, just validate the settings
            if (settings.MenuDiscountPercentage < 0 || settings.MenuDiscountPercentage > 100)
                throw new ArgumentException("Menu discount percentage must be between 0 and 100");

            if (settings.OrderValueForFreeShipping < 0)
                throw new ArgumentException("Order value for free shipping must be positive");

            if (settings.ShippingCost < 0)
                throw new ArgumentException("Shipping cost must be positive");

            if (settings.LoyaltyOrderCount <= 0)
                throw new ArgumentException("Loyalty order count must be positive");

            if (settings.LoyaltyTimePeriodDays <= 0)
                throw new ArgumentException("Loyalty time period must be positive");

            if (settings.LoyaltyDiscountPercentage < 0 || settings.LoyaltyDiscountPercentage > 100)
                throw new ArgumentException("Loyalty discount percentage must be between 0 and 100");

            if (settings.LowStockThreshold < 0)
                throw new ArgumentException("Low stock threshold must be positive");

            await Task.CompletedTask;
        }

        public async Task<decimal> GetMenuDiscountPercentageAsync()
        {
            string value = _configuration["AppSettings:MenuDiscountPercentage"];
            if (decimal.TryParse(value, out decimal result))
                return await Task.FromResult(result);

            return await Task.FromResult(10m); // Default: 10%
        }

        public async Task<decimal> GetOrderValueForFreeShippingAsync()
        {
            string value = _configuration["AppSettings:OrderValueForFreeShipping"];
            if (decimal.TryParse(value, out decimal result))
                return await Task.FromResult(result);

            return await Task.FromResult(100m); // Default: 100 lei
        }

        public async Task<decimal> GetShippingCostAsync()
        {
            string value = _configuration["AppSettings:ShippingCost"];
            if (decimal.TryParse(value, out decimal result))
                return await Task.FromResult(result);

            return await Task.FromResult(15m); // Default: 15 lei
        }

        public async Task<int> GetLoyaltyOrderCountAsync()
        {
            string value = _configuration["AppSettings:LoyaltyOrderCount"];
            if (int.TryParse(value, out int result))
                return await Task.FromResult(result);

            return await Task.FromResult(3); // Default: 3 orders
        }

        public async Task<int> GetLoyaltyTimePeriodDaysAsync()
        {
            string value = _configuration["AppSettings:LoyaltyTimePeriodDays"];
            if (int.TryParse(value, out int result))
                return await Task.FromResult(result);

            return await Task.FromResult(30); // Default: 30 days
        }

        public async Task<decimal> GetLoyaltyDiscountPercentageAsync()
        {
            string value = _configuration["AppSettings:LoyaltyDiscountPercentage"];
            if (decimal.TryParse(value, out decimal result))
                return await Task.FromResult(result);

            return await Task.FromResult(5m); // Default: 5%
        }

        public async Task<decimal> GetLowStockThresholdAsync()
        {
            string value = _configuration["AppSettings:LowStockThreshold"];
            if (decimal.TryParse(value, out decimal result))
                return await Task.FromResult(result);

            return await Task.FromResult(1000m); // Default: 1000 grams (1 kg)
        }
    }
}