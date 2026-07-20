Spotify Clone - Website nghe nhạc trực tuyến
1. Giới thiệu

Spotify Clone là website nghe nhạc trực tuyến được xây dựng nhằm mô phỏng các chức năng cốt lõi của nền tảng Spotify, giúp người dùng có thể nghe nhạc, khám phá nghệ sĩ, tạo playlist và quản lý thư viện nhạc cá nhân.

Hệ thống có ba vai trò chính:

User: đăng ký tài khoản, đăng nhập, nghe nhạc, tìm kiếm bài hát, tạo và quản lý playlist.
Artist: đăng tải bài hát, quản lý album, chỉnh sửa thông tin các sản phẩm âm nhạc của mình.
Admin: quản lý người dùng, nghệ sĩ, bài hát, album, thể loại và toàn bộ hoạt động của hệ thống.

Spotify Clone được xây dựng theo phạm vi MVP (Minimum Viable Product), tập trung vào các chức năng nghe nhạc cơ bản và một tính năng AI hỗ trợ gợi ý bài hát.

2. Chức năng chính
User
Đăng ký, đăng nhập và đăng xuất.
Tìm kiếm bài hát, album và nghệ sĩ.
Nghe nhạc trực tuyến.
Xem thông tin bài hát và album.
Tạo, chỉnh sửa và xóa playlist.
Thêm hoặc xóa bài hát khỏi playlist.
Quản lý thông tin cá nhân.
Nhận gợi ý bài hát từ AI.
Artist
Đăng tải bài hát mới.
Quản lý bài hát đã đăng.
Tạo và quản lý album.
Chỉnh sửa thông tin bài hát.
Xem thống kê lượt nghe cơ bản.
Admin
Quản lý tài khoản người dùng.
Quản lý nghệ sĩ.
Quản lý bài hát.
Quản lý album.
Quản lý thể loại nhạc.
Kiểm duyệt nội dung.
Xem thống kê hệ thống.
3. Tính năng AI

Tính năng AI hỗ trợ gợi ý bài hát phù hợp dựa trên:

Thể loại nhạc yêu thích.
Nghệ sĩ thường nghe.
Lịch sử nghe nhạc.
Playlist của người dùng.
Từ khóa tìm kiếm gần đây.

AI chỉ đề xuất các bài hát đã tồn tại trong cơ sở dữ liệu của hệ thống và không tự tạo bài hát, nghệ sĩ hoặc album mới.

Kết quả gợi ý được đánh dấu là nội dung do AI hỗ trợ tạo. Người dùng có thể bỏ qua các đề xuất và tự lựa chọn bài hát theo nhu cầu.

4. Tiến độ hiện tại - Tuần 8

Dự án hiện đang ở tuần 8 theo lộ trình học phần.

Nhóm đã hoàn thành:

Phân tích yêu cầu và xác định phạm vi MVP.
Xây dựng Persona, Scenario, User Story và Product Backlog.
Thiết kế giao diện và luồng sử dụng.
Thiết kế cơ sở dữ liệu và kiến trúc hệ thống.
Triển khai chức năng đăng ký, đăng nhập và phân quyền.
Triển khai chức năng nghe nhạc, quản lý playlist và quản lý bài hát.
Hoàn thành tính năng AI gợi ý bài hát.
Quản lý mã nguồn bằng GitHub với Issues và Pull Requests cho từng chức năng.

5. Công nghệ sử dụng
Frontend: HTML, CSS, JavaScript.
Backend: ASP.NET Core MVC (C#).
Database: SQL Server.
ORM: Entity Framework Core.
Quản lý mã nguồn: GitHub.
Thiết kế giao diện: Figma.
AI Agent: ChatGPT, CLAUDE hoặc công cụ tương đương.
AI Feature: API mô hình ngôn ngữ hoặc SDK phù hợp

CẤU TRÚC THƯ MỤC DỰ ÁN
```text
SpotifyClone/
├── Controllers/          # Xử lý request và điều hướng
├── Models/               # Các model dữ liệu
├── Views/                # Giao diện người dùng
├── wwwroot/
│   ├── css/              # File CSS
│   ├── js/               # File JavaScript
│   ├── images/           # Hình ảnh
│   └── uploads/          # File nhạc và ảnh upload
├── Data/                 # DbContext và Entity Framework
├── Services/             # Business Logic và AI Service
├── Migrations/           # Migration cơ sở dữ liệu
├── appsettings.json      # Cấu hình hệ thống
├── Program.cs            # Điểm khởi động ứng dụng
└── README.md
```

