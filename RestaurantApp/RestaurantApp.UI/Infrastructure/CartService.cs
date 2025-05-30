﻿using RestaurantApp.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RestaurantApp.UI.Infrastructure
{
    public class CartService : ICartService
    {
        private readonly List<CartItem> _cartItems = new List<CartItem>();
        private readonly IDishService _dishService;
        private readonly IMenuService _menuService;
        private readonly IConfigurationService _configService;
        private readonly IUserSessionService _userSessionService;

        public CartService(
            IDishService dishService,
            IMenuService menuService,
            IConfigurationService configService,
            IUserSessionService userSessionService)
        {
            _dishService = dishService;
            _menuService = menuService;
            _configService = configService;
            _userSessionService = userSessionService;
        }

        public void AddDish(int dishId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            // Check if dish already in cart
            var existingItem = _cartItems.FirstOrDefault(i => i.Id == dishId && i.IsDish);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                System.Diagnostics.Debug.WriteLine($"Updated existing dish quantity to {existingItem.Quantity}");
                return;
            }

            // Add new item asynchronously to avoid UI thread blocking
            Task.Run(async () => {
                try
                {
                    // Get dish details
                    var dish = await _dishService.GetDishByIdAsync(dishId);
                    if (dish == null)
                        throw new ArgumentException($"Dish with ID {dishId} not found");

                    // Add to cart on UI thread to avoid collection modification issues
                    Application.Current.Dispatcher.Invoke(() => {
                        _cartItems.Add(new CartItem
                        {
                            Id = dishId,
                            IsDish = true,
                            Name = dish.Name,
                            UnitPrice = dish.Price,
                            Quantity = quantity,
                            ImageUrl = dish.Images?.FirstOrDefault()?.ImagePath ?? "/Images/default-dish.png"
                        });
                        System.Diagnostics.Debug.WriteLine($"Added new dish to cart: {dish.Name}");
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error adding dish to cart: {ex.Message}\n{ex.StackTrace}");
                    throw; // Rethrow to allow handling in the ViewModel
                }
            }).Wait(); // This could be causing a deadlock - consider removing .Wait()
        }

        public void AddMenu(int menuId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            // Check if menu already in cart
            var existingItem = _cartItems.FirstOrDefault(i => i.Id == menuId && !i.IsDish);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                System.Diagnostics.Debug.WriteLine($"Updated existing menu quantity to {existingItem.Quantity}");
                return;
            }

            // Add new item asynchronously to avoid UI thread blocking
            Task.Run(async () => {
                try
                {
                    // Get menu details
                    var menu = await _menuService.GetMenuByIdAsync(menuId);
                    if (menu == null)
                        throw new ArgumentException($"Menu with ID {menuId} not found");

                    // Calculate price
                    var menuDiscountPercentage = await _configService.GetMenuDiscountPercentageAsync();
                    var price = await _menuService.CalculateMenuPriceAsync(menuId, menuDiscountPercentage);

                    // Add to cart on UI thread to avoid collection modification issues
                    Application.Current.Dispatcher.Invoke(() => {
                        _cartItems.Add(new CartItem
                        {
                            Id = menuId,
                            IsDish = false,
                            Name = menu.Name,
                            UnitPrice = price,
                            Quantity = quantity,
                            ImageUrl = "/Images/default-menu.png" // Default image for menus
                        });
                        System.Diagnostics.Debug.WriteLine($"Added new menu to cart: {menu.Name}");
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error adding menu to cart: {ex.Message}\n{ex.StackTrace}");
                    throw; // Rethrow to allow handling in the ViewModel
                }
            }).Wait(); // This could be causing a deadlock - consider removing .Wait()
        }

        public void UpdateQuantity(int itemId, bool isDish, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                RemoveItem(itemId, isDish);
                return;
            }

            var item = _cartItems.FirstOrDefault(i => i.Id == itemId && i.IsDish == isDish);
            if (item != null)
            {
                item.Quantity = newQuantity;
            }
        }

        public void RemoveItem(int itemId, bool isDish)
        {
            var item = _cartItems.FirstOrDefault(i => i.Id == itemId && i.IsDish == isDish);
            if (item != null)
            {
                _cartItems.Remove(item);
            }
        }

        public void ClearCart()
        {
            _cartItems.Clear();
        }

        public List<CartItem> GetCartItems()
        {
            return new List<CartItem>(_cartItems);
        }

        public decimal GetSubtotal()
        {
            return _cartItems.Sum(i => i.TotalPrice);
        }

        public decimal GetShippingCost()
        {
            var subtotal = GetSubtotal();
            var freeShippingThreshold = _configService.GetOrderValueForFreeShippingAsync().Result;
            var shippingCost = _configService.GetShippingCostAsync().Result;

            return subtotal >= freeShippingThreshold ? 0 : shippingCost;
        }

        public decimal GetDiscount()
        {
            var subtotal = GetSubtotal();
            var orderValueThreshold = _configService.GetOrderValueForFreeShippingAsync().Result;
            var discountPercentage = _configService.GetLoyaltyDiscountPercentageAsync().Result;

            decimal discount = 0;

            // Apply discount for orders above threshold
            if (subtotal >= orderValueThreshold)
            {
                discount += subtotal * (discountPercentage / 100);
            }

            return discount;
        }

        public decimal GetTotal()
        {
            return GetSubtotal() + GetShippingCost() - GetDiscount();
        }

        public int GetItemCount()
        {
            return _cartItems.Sum(i => i.Quantity);
        }

        public bool HasItems()
        {
            return _cartItems.Any();
        }
    }
}