using ShopSphere.Application.Messaging;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Errors;

namespace ShopSphere.Application.Services;

public class CheckoutService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _publisher;

    public CheckoutService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IProductRepository productRepository,
        IEventPublisher publisher)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _productRepository = productRepository;
        _publisher = publisher;
    }

    public async Task<Order> StartCheckoutAsync(string buyerEmail, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(buyerEmail))
            throw new BadRequestException("Buyer email is required.");
        if (quantity <= 0)
            throw new BadRequestException("Quantity must be > 0.");

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("Product not found.");
        if (product.Stock < quantity)
            throw new BadRequestException("Not enough stock.");

        var total = product.Price * quantity;

        product.Stock -= quantity;
        await _productRepository.UpdateAsync(product);

        var order = new Order
        {
            BuyerEmail = buyerEmail.Trim(),
            TotalAmount = total,
            Status = OrderStatus.PendingPayment
        };

        order = await _orderRepository.AddAsync(order, cancellationToken);

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = total,
            Status = PaymentStatus.Pending
        };
        await _paymentRepository.AddAsync(payment, cancellationToken);

        await _publisher.PublishAsync("order.created", new
        {
            order.Id,
            order.BuyerEmail,
            order.TotalAmount,
            order.Status,
            paymentId = payment.Id,
            productId,
            quantity,
            product.Stock
        }, order.Id.ToString());

        return order;
    }

    public async Task<Order> MarkPaymentCompletedAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new NotFoundException("Order not found");

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null) throw new NotFoundException("Payment not found");

        payment.Status = PaymentStatus.Completed;
        payment.UpdatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.PaymentCompleted;
        order.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _publisher.PublishAsync("payment.completed", new
        {
            orderId = order.Id,
            paymentId = payment.Id,
            payment.Amount,
            orderStatus = order.Status
        }, payment.Id.ToString());
        return order;
    }

    public async Task<Order> FailPaymentAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new NotFoundException("Order not found");

        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (payment == null) throw new NotFoundException("Payment not found");

        payment.Status = PaymentStatus.Failed;
        payment.UpdatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.Failed;
        order.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _orderRepository.UpdateAsync(order, cancellationToken);

        await _publisher.PublishAsync("payment.failed", new
        {
            orderId = order.Id,
            paymentId = payment.Id,
            payment.Amount,
            reason
        }, payment.Id.ToString());


        return order;
    }

    public async Task<Order> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order == null) throw new NotFoundException("Order not found");
        return order;
    }
}
