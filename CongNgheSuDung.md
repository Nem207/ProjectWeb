# 3. Công nghệ sử dụng trong dự án

## 3.1 Front-end

Phần Front-end của hệ thống được xây dựng bằng **ASP.NET Core Razor View (.cshtml)** kết hợp với **HTML, CSS và JavaScript** nhằm tạo giao diện thân thiện, trực quan và dễ sử dụng.

### Razor View (.cshtml)

Razor View là công nghệ giao diện của ASP.NET Core MVC, cho phép kết hợp mã HTML với ngôn ngữ C# trong cùng một tệp có phần mở rộng **.cshtml**. Razor hỗ trợ hiển thị dữ liệu từ Controller đến View thông qua cú pháp đơn giản, giúp việc xây dựng giao diện động trở nên thuận tiện và dễ bảo trì.

Ưu điểm:

* Kết hợp HTML và C# trong cùng một trang.
* Hỗ trợ hiển thị dữ liệu động từ Model.
* Dễ dàng sử dụng các tính năng của ASP.NET Core như Tag Helper, Partial View và Layout.
* Giảm lượng mã lặp lại khi xây dựng giao diện.

### CSS

CSS (Cascading Style Sheets) được sử dụng để định dạng giao diện của website, bao gồm màu sắc, bố cục, khoảng cách, hiệu ứng và khả năng hiển thị trên nhiều kích thước màn hình.

Vai trò:

* Thiết kế giao diện trực quan.
* Tăng trải nghiệm người dùng.
* Xây dựng giao diện đồng nhất trên toàn bộ hệ thống.
* Hỗ trợ Responsive Design giúp website hoạt động tốt trên nhiều thiết bị.

### JavaScript

JavaScript được sử dụng để xử lý các chức năng phía trình duyệt, tăng tính tương tác giữa người dùng và hệ thống.

Một số chức năng tiêu biểu:

* Kiểm tra dữ liệu đầu vào (Client-side Validation).
* Xử lý sự kiện như nhấn nút, tìm kiếm và lọc dữ liệu.
* Gửi yêu cầu bất đồng bộ (AJAX) để cập nhật dữ liệu mà không cần tải lại toàn bộ trang.
* Hiển thị thông báo và các hiệu ứng giao diện.

---

## 3.2 Back-end

Phần Back-end của hệ thống được phát triển bằng **C#** trên nền tảng **ASP.NET Core MVC**.

### Ngôn ngữ C#

C# là ngôn ngữ lập trình hướng đối tượng do Microsoft phát triển trên nền tảng .NET. Ngôn ngữ này có hiệu năng cao, bảo mật tốt, dễ bảo trì và phù hợp để phát triển các ứng dụng web quy mô vừa và lớn.

Đặc điểm nổi bật:

* Hỗ trợ lập trình hướng đối tượng (OOP).
* Quản lý bộ nhớ tự động bằng Garbage Collector.
* Tích hợp mạnh với hệ sinh thái .NET.
* Hỗ trợ nhiều thư viện phục vụ phát triển ứng dụng.

### ASP.NET Core MVC

ASP.NET Core MVC là framework mã nguồn mở của Microsoft dùng để phát triển ứng dụng web theo mô hình **Model – View – Controller (MVC)**.

Mô hình MVC chia hệ thống thành ba thành phần:

* **Model:** Quản lý dữ liệu, các lớp đối tượng và nghiệp vụ liên quan đến dữ liệu.
* **View:** Hiển thị giao diện cho người dùng.
* **Controller:** Tiếp nhận yêu cầu từ người dùng, xử lý nghiệp vụ và trả kết quả về View.

Việc sử dụng mô hình MVC mang lại nhiều lợi ích:

* Tách biệt giao diện và logic xử lý.
* Dễ bảo trì và mở rộng hệ thống.
* Hỗ trợ làm việc nhóm hiệu quả.
* Tăng khả năng tái sử dụng mã nguồn.

---

## 3.3 Hệ quản trị cơ sở dữ liệu

Hệ thống sử dụng **Microsoft SQL Server** để lưu trữ và quản lý dữ liệu.

SQL Server là hệ quản trị cơ sở dữ liệu quan hệ (Relational Database Management System - RDBMS) do Microsoft phát triển, đáp ứng tốt các yêu cầu về hiệu năng, tính ổn định và bảo mật.

Ưu điểm của SQL Server:

* Hiệu năng cao đối với lượng dữ liệu lớn.
* Hỗ trợ giao dịch (Transaction) đảm bảo tính toàn vẹn dữ liệu.
* Khả năng sao lưu và phục hồi dữ liệu tốt.
* Tích hợp chặt chẽ với nền tảng .NET.

---

## 3.4 Entity Framework Core

Để giao tiếp giữa ứng dụng và cơ sở dữ liệu, dự án sử dụng **Entity Framework Core (EF Core)**.

Entity Framework Core là thư viện ORM (Object-Relational Mapping) của Microsoft, cho phép lập trình viên thao tác với cơ sở dữ liệu thông qua các đối tượng C# thay vì viết trực tiếp câu lệnh SQL.

EF Core thực hiện việc ánh xạ (Mapping) giữa các bảng trong SQL Server và các lớp Model trong chương trình.

Các chức năng chính:

* Truy vấn dữ liệu bằng LINQ.
* Thêm, sửa và xóa dữ liệu.
* Quản lý quan hệ giữa các bảng.
* Theo dõi sự thay đổi của dữ liệu (Change Tracking).
* Hỗ trợ Migration để tạo và cập nhật cấu trúc cơ sở dữ liệu.

Lợi ích khi sử dụng Entity Framework Core:

* Giảm số lượng câu lệnh SQL cần viết.
* Tăng tốc quá trình phát triển.
* Giảm lỗi trong thao tác với cơ sở dữ liệu.
* Dễ bảo trì và mở rộng hệ thống.
* Tích hợp hoàn toàn với ASP.NET Core MVC.

---

## 3.5 Kiến trúc tổng thể của hệ thống

Dự án được xây dựng theo mô hình **ASP.NET Core MVC** với luồng xử lý như sau:

1. Người dùng gửi yêu cầu thông qua giao diện Razor View.
2. Controller tiếp nhận yêu cầu từ người dùng.
3. Controller gọi Model hoặc Service để xử lý nghiệp vụ.
4. Entity Framework Core thực hiện giao tiếp với SQL Server để truy xuất hoặc cập nhật dữ liệu.
5. Kết quả được trả về Controller.
6. Controller truyền dữ liệu đến View.
7. View hiển thị kết quả cho người dùng.

Kiến trúc này giúp hệ thống có cấu trúc rõ ràng, dễ phát triển, dễ kiểm thử và thuận tiện trong việc bảo trì cũng như mở rộng các chức năng trong tương lai.
