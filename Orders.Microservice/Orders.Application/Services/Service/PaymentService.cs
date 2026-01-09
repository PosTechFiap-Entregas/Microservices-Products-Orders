using Microsoft.Extensions.Logging;
using Orders.Application.DTOs;
using Orders.Application.Services.Interface;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Application.Services.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IOrderRepository orderRepository,
            ILogger<PaymentService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<PaymentWebhookResponseDto> ProcessWebhookAsync(PaymentWebhookDto webhookDto)
        {
            try
            {
                _logger.LogInformation(
                    "Processando webhook de pagamento. OrderId: {OrderId}, Status: {Status}, PaymentId: {PaymentId}",
                    webhookDto.OrderId, webhookDto.Status, webhookDto.PaymentId);

                // 1. Validar dados do webhook
                if (!int.TryParse(webhookDto.OrderId, out int orderId))
                {
                    _logger.LogWarning("OrderId inválido: {OrderId}", webhookDto.OrderId);
                    return new PaymentWebhookResponseDto(
                        Success: false,
                        Message: "OrderId inválido"
                    );
                }

                // 2. Buscar o pedido
                var order = await _orderRepository.GetByIdWithItemsAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning("Pedido não encontrado. OrderId: {OrderId}", orderId);
                    return new PaymentWebhookResponseDto(
                        Success: false,
                        Message: $"Pedido {orderId} não encontrado"
                    );
                }

                // 3. Validar se o pedido já foi processado
                if (!string.IsNullOrEmpty(order.PaymentId) &&
                    order.PaymentId == webhookDto.PaymentId &&
                    order.PaymentStatus == PaymentStatusEnum.PAID)
                {
                    _logger.LogInformation(
                        "Webhook duplicado ignorado. OrderId: {OrderId}, PaymentId: {PaymentId}",
                        orderId, webhookDto.PaymentId);

                    return new PaymentWebhookResponseDto(
                        Success: true,
                        Message: "Pagamento já processado anteriormente",
                        OrderNumber: order.Number
                    );
                }

                // 4. Converter status do webhook para enum
                var paymentStatus = webhookDto.Status.ToUpper() switch
                {
                    "PAID" => PaymentStatusEnum.PAID,
                    "REFUSED" => PaymentStatusEnum.REFUSED,
                    "CANCELLED" => PaymentStatusEnum.CANCELLED,
                    _ => PaymentStatusEnum.PENDING
                };

                // 5. Processar o pagamento
                order.ProcessPayment(webhookDto.PaymentId, paymentStatus);

                // 6. Se pagamento falhou, cancelar pedido
                if (paymentStatus == PaymentStatusEnum.REFUSED ||
                    paymentStatus == PaymentStatusEnum.CANCELLED)
                {
                    order.CancelDueToPaymentFailure();
                }

                // 7. Salvar no banco
                await _orderRepository.UpdateAsync(order);

                _logger.LogInformation(
                    "Webhook processado com sucesso. OrderId: {OrderId}, Status: {Status}",
                    orderId, paymentStatus);

                return new PaymentWebhookResponseDto(
                    Success: true,
                    Message: $"Pagamento {paymentStatus} processado com sucesso",
                    OrderNumber: order.Number
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao processar webhook. OrderId: {OrderId}, PaymentId: {PaymentId}",
                    webhookDto.OrderId, webhookDto.PaymentId);

                return new PaymentWebhookResponseDto(
                    Success: false,
                    Message: "Erro ao processar webhook de pagamento"
                );
            }
        }
    }
}