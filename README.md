# NotificationService (Synapse Project)

NotificationService là một hệ thống microservice xử lý thông báo sự kiện (event-driven) hiệu năng cao, được xây dựng trên nền tảng .NET 9. Hệ thống được thiết kế để xử lý hàng triệu thông báo với độ trễ thấp, đảm bảo tính nhất quán và khả năng mở rộng ngang (Horizontal Scaling).

## 🚀 Tính Năng Nổi Bật

- **Kiến trúc Clean Architecture**: Phân tách rõ ràng giữa Domain, Application và Infrastructure.
- **Xử lý sự kiện với Kafka**: Tích hợp 6 partitions để phân tải xử lý đồng thời.
- **Chống trùng lặp (Idempotency)**: Sử dụng Redis để đảm bảo mỗi thông báo chỉ được gửi 1 lần duy nhất.
- **SignalR Multi-instance**: Hỗ trợ nhiều instance API chạy song song nhờ Redis Backplane.
- **Nhật ký Audit**: Lưu trữ lịch sử thông báo vào PostgreSQL thông qua Repository Pattern.
- **Công cụ Test**: Bao gồm giao diện `tester.html` chuyên dụng hỗ trợ Load Test (Mega Bomb).

## 🛠️ Khởi Chạy Nhanh (Docker Compose)

Hãy đảm bảo bạn đã cài đặt Docker, sau đó chạy lệnh:

```bash
docker-compose up -d --build
```

Lệnh này sẽ khởi chạy:
- **ZooKeeper & Kafka** (Port 9092)
- **Redis** (Port 6379)
- **PostgreSQL** (Port 5433)
- **Kafka-UI** (Port 8081) - Truy cập để quản lý Kafka trực quan.
- **API Instance 1** (Port 5027)
- **API Instance 2** (Port 5028)

## 📄 Tài Liệu Chi Tiết

Tất cả thông tin chi tiết về kiến trúc, cách vận hành, các lệnh Test tải (Mega Bomb) và lộ trình phát triển (Roadmap) đã được tổng hợp tại đây:

👉 **[Tài liệu bàn giao (Handover Document)](file:///D:/100_Synapse/NotificationService/handover_document.md)**

## 🧪 Giao Diện Thử Nghiệm

Mở file `tester.html` trực tiếp trên trình duyệt để kết nối và theo dõi hệ thống trong thời gian thực. Giao diện hỗ trợ Connect đa instance và Auto-Refresh Audit Logs.

---
© 2026 Synapse Network Development Team.
