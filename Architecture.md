# 4. Kiến trúc hệ thống

Hệ thống được phát triển theo kiến trúc **Client – Server** kết hợp với mô hình **ASP.NET Core MVC (Model – View – Controller)**. Kiến trúc này giúp phân tách rõ ràng giữa giao diện người dùng, xử lý nghiệp vụ và quản lý dữ liệu, từ đó nâng cao khả năng bảo trì, mở rộng và phát triển hệ thống trong tương lai.

## 4.1 Kiến trúc Client – Server

Hệ thống gồm hai thành phần chính:

* **Client:** Là trình duyệt web của người dùng. Client có nhiệm vụ hiển thị giao diện, tiếp nhận thao tác của người dùng và gửi các yêu cầu (HTTP Request) đến máy chủ.
* **Server:** Là ứng dụng ASP.NET Core MVC. Server tiếp nhận yêu cầu từ Client, xử lý nghiệp vụ, truy xuất cơ sở dữ liệu SQL Server thông qua Entity Framework Core và trả kết quả về Client dưới dạng HTTP Response.

Việc sử dụng mô hình Client – Server giúp giảm tải cho phía người dùng, đồng thời tập trung toàn bộ quá trình xử lý nghiệp vụ và quản lý dữ liệu tại máy chủ, đảm bảo tính nhất quán và bảo mật của hệ thống.

## 4.2 Kiến trúc MVC

Ứng dụng được xây dựng theo mô hình **Model – View – Controller (MVC)**.

### Model

Model đại diện cho dữ liệu và các đối tượng nghiệp vụ của hệ thống. Các lớp Model được ánh xạ với các bảng trong cơ sở dữ liệu thông qua Entity Framework Core. Model chịu trách nhiệm lưu trữ dữ liệu, định nghĩa các thuộc tính và mối quan hệ giữa các thực thể.

### View

View được xây dựng bằng **Razor (.cshtml)** kết hợp với HTML, CSS và JavaScript. Thành phần này có nhiệm vụ hiển thị dữ liệu cho người dùng và cung cấp giao diện để thực hiện các thao tác như tìm kiếm, thêm, sửa hoặc xóa dữ liệu.

### Controller

Controller đóng vai trò trung gian giữa View và Model, Service. Khi nhận yêu cầu từ người dùng, Controller sẽ xử lý nghiệp vụ, gọi Entity Framework Core để truy xuất hoặc cập nhật dữ liệu trong SQL Server, sau đó truyền dữ liệu đến View để hiển thị.

## 4.3 Luồng xử lý của hệ thống

Quy trình xử lý một yêu cầu trong hệ thống diễn ra theo các bước sau:

**Bước 1:** Người dùng thực hiện thao tác trên giao diện thông qua trình duyệt, chẳng hạn truy cập địa chỉ **/Songs/Detail/5**.

**Bước 2:** Yêu cầu HTTP được gửi đến ứng dụng ASP.NET Core. Hệ thống Routing phân tích URL và xác định Controller cùng Action tương ứng để xử lý yêu cầu.

**Bước 3:** Controller nhận yêu cầu, kiểm tra các tham số đầu vào và thực hiện các nghiệp vụ cần thiết.

**Bước 4:** Controller sử dụng Entity Framework Core để giao tiếp với cơ sở dữ liệu SQL Server. Thông qua DbContext, Entity Framework Core thực hiện truy vấn, thêm, sửa hoặc xóa dữ liệu mà không cần viết trực tiếp các câu lệnh SQL.

**Bước 5:** Dữ liệu lấy được từ cơ sở dữ liệu được ánh xạ thành các đối tượng Model và trả về Controller.

**Bước 6:** Controller truyền Model sang View. Razor View kết hợp dữ liệu với giao diện HTML để sinh nội dung động gửi về trình duyệt.

**Bước 7:** Trình duyệt nhận HTTP Response và hiển thị kết quả cho người dùng.

## 4.4 Vai trò của Entity Framework Core

Entity Framework Core đóng vai trò là lớp trung gian giữa ứng dụng và cơ sở dữ liệu SQL Server. Công nghệ ORM này giúp ánh xạ các bảng dữ liệu thành các lớp C#, cho phép lập trình viên thao tác với dữ liệu thông qua các đối tượng thay vì viết trực tiếp câu lệnh SQL.

Việc sử dụng Entity Framework Core mang lại nhiều lợi ích như:

* Giảm lượng mã SQL cần viết.
* Tăng tốc độ phát triển ứng dụng.
* Hỗ trợ truy vấn dữ liệu bằng LINQ.
* Quản lý quan hệ giữa các bảng dễ dàng.
* Hỗ trợ Migration để quản lý và cập nhật cấu trúc cơ sở dữ liệu.

## 4.5 Ưu điểm của kiến trúc hệ thống

Kiến trúc Client – Server kết hợp với mô hình MVC mang lại nhiều ưu điểm cho hệ thống:

* Phân tách rõ giao diện, nghiệp vụ và dữ liệu.
* Dễ dàng bảo trì, nâng cấp và mở rộng chức năng.
* Hỗ trợ phát triển theo nhóm nhờ các thành phần độc lập.
* Tăng khả năng tái sử dụng mã nguồn.
* Thuận tiện trong việc kiểm thử và sửa lỗi.
* Tích hợp hiệu quả với SQL Server thông qua Entity Framework Core.
* Đảm bảo hiệu năng và khả năng mở rộng khi số lượng người dùng tăng lên.

Nhờ áp dụng kiến trúc này, hệ thống có cấu trúc rõ ràng, dễ quản lý và đáp ứng tốt các yêu cầu phát triển cũng như mở rộng trong tương lai.

<img width="368" height="247" alt="image" src="https://github.com/user-attachments/assets/fd9fde2c-84af-47aa-8d7e-a3e8dc81a5d8" />

