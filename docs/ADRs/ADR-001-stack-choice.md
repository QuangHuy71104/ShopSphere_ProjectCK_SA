# ADR-001: Technology Stack Choice

## Context
Dự án ShopSphere yêu cầu xây dựng một backend quy mô lớn, tiến hóa qua 3 milestone:
- Sub1: nền tảng kiến trúc + data layer + CRUD catalog
- Sub2: authentication, caching, async messaging
- Sub3: reliability (outbox), consistency, saga orchestration

Team cần stack phổ biến, hỗ trợ REST API, ORM + migrations tốt, caching cho catalog đọc-heavy và message broker cho event-driven.

## Decision
Chọn stack chính:
- **.NET 9 (ASP.NET Core Web API)**
- **EF Core**
- **SQL Server**
- **Redis**
- **RabbitMQ**

## Rationale
- **ASP.NET Core Web API (.NET 9)**: middleware/DI mạnh, phù hợp clean architecture và phát triển nhanh.
- **EF Core**: code-first dễ evolve schema theo milestone, mapping linh hoạt.
- **SQL Server**: CSDL quan hệ ổn định, phù hợp mô hình e-commerce (product, order, payment).
- **Redis**: tối ưu hiệu năng cho các endpoint đọc-heavy của catalog ở Sub2.
- **RabbitMQ**: hỗ trợ retry/DLQ rõ ràng, phù hợp cho event-driven, outbox và saga ở Sub2–3.

## Consequences
- Domain/Application không phụ thuộc trực tiếp EF Core; chỉ dùng interfaces.
- Migrations được quản lý bằng EF Core và lưu tại **`/db/migrations`** theo yêu cầu đề bài.
- Local dev dùng Docker cho SQL Server/Redis/RabbitMQ; connection string dev cần:
  `TrustServerCertificate=True;Encrypt=False` để tránh lỗi TLS.
- Các milestone sau phải mở rộng theo boundaries đã chốt để không phá vỡ kiến trúc.
