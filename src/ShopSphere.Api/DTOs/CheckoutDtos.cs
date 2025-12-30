namespace ShopSphere.Api.DTOs;

public record CheckoutRequestDto(string BuyerEmail, Guid ProductId, int Quantity);
public record CheckoutResponseDto(Guid OrderId, string BuyerEmail, Guid ProductId, int Quantity, decimal TotalAmount, string Status);
public record PaymentResultDto(Guid OrderId, string Status, string Message);
