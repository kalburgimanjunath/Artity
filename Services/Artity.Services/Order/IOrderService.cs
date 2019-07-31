﻿namespace Artity.Services.Order
{
    using Artity.Data.Models;

    using Artity.Web.InputModels.Order;

    using System.Threading.Tasks;

    public interface IOrderService
    {
        Task<bool> CreateOrder(OrderCreateInputModel inputModel);
    }
}
