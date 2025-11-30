# ADR-002: Clean Architecture + Modular Monolith

## Context
ShopSphere tiến hóa từ nền tảng monolith đến scalable/event-driven qua 3 submission. Team 3 người cần một kiến trúc:
- Đủ đơn giản để phát triển nhanh ở Sub1
- Ranh giới module rõ ràng để mở rộng Cart/Order/Payment ở Sub2–3
- Không vỡ khi bổ sung caching, messaging, outbox, saga
- Dễ test và maintain

## Decision
Áp dụng:
1. **Clean Architecture (Layered)**
2. **Modular Monolith theo Domain Modules**

## Rationale
- **Clean Architecture** tách rõ 4 layer:
  - **Domain**: entities, enums, domain rules
  - **Application**: use-cases/services, interfaces
  - **Infrastructure**: EF Core, repositories, Redis, RabbitMQ
  - **API**: controllers, DTO/contracts  
  → giúp giảm coupling, dễ test và refactor.
- **Modular Monolith** cho phép phát triển nhanh trong một codebase nhưng vẫn giữ boundaries:
  - **Catalog (Sub1)**: Product, Category
  - **Cart (Sub3)**: Cart, CartItem
  - **Order (Sub3)**: Order, OrderItem
  - **Payment (Sub3)**: Payment  
  → mỗi module độc lập về domain, share chung hạ tầng.
- Kiến trúc này chuẩn bị sẵn cho:
  - **Redis caching (Sub2)** ở Infrastructure
  - **RabbitMQ events (Sub2)** ở Infrastructure
  - **Outbox + Saga (Sub3)** qua Worker/Infrastructure
- Ranh giới module rõ giúp chuyển sang event-driven không làm rối code.

## Consequences
- Controllers không được truy cập trực tiếp `DbContext` hoặc query SQL.
- Application chỉ giao tiếp dữ liệu qua repository interfaces.
- Infrastructure là nơi duy nhất “biết” EF Core/Redis/RabbitMQ.
- Các module phải tránh circular dependencies; mọi liên kết cross-module sẽ đi qua event contracts ở Sub2–3.
- Khi thêm async, event schema phải ổn định để không phá use-cases đã có.
