Tuần 7: Phát triển các chức năng Admin và chuyển đổi giao diện sang Razor View

Sau khi thống nhất công nghệ và điều chỉnh lại kế hoạch trong thời gian nghỉ hè, nhóm bắt đầu tập trung vào việc hiện thực hóa các chức năng của hệ thống. Trong tuần này, nhóm ưu tiên phát triển module Admin nhằm xây dựng nền tảng quản trị cho toàn bộ ứng dụng SpotifyClone.

Các thành viên phụ trách Back-end và Front-end phối hợp để hoàn thiện các chức năng quản lý cơ bản của Admin như quản lý người dùng, nghệ sĩ, bài hát, album và danh mục dữ liệu. Đồng thời, nhóm kết nối các chức năng này với cơ sở dữ liệu SQL Server thông qua Entity Framework Core, giúp các thao tác thêm, sửa, xóa và truy vấn dữ liệu được thực hiện trực tiếp trên hệ thống.

Bên cạnh đó, giao diện Home trước đây được xây dựng dưới dạng HTML tĩnh cũng được nhóm chuyển đổi sang Razor View (HTML nhúng C#) để phù hợp với kiến trúc ASP.NET Core MVC. Việc chuyển đổi này giúp giao diện có thể hiển thị dữ liệu động từ cơ sở dữ liệu thay vì sử dụng dữ liệu mẫu như trước, đồng thời tạo điều kiện thuận lợi cho việc tích hợp các chức năng tìm kiếm, hiển thị danh sách bài hát, album và nghệ sĩ trong các giai đoạn tiếp theo.

Trong quá trình thực hiện, nhóm cũng bắt đầu tích hợp Front-end với Back-end theo mô hình Model – View – Controller, giúp từng thành phần của hệ thống hoạt động thống nhất và dễ bảo trì. Việc phân chia rõ ràng giữa giao diện, xử lý nghiệp vụ và truy cập dữ liệu giúp các thành viên có thể phát triển song song nhưng vẫn đảm bảo tính nhất quán của toàn bộ dự án.

Đến cuối tuần, nhóm đã hoàn thành các chức năng chính của module Admin, chuyển đổi thành công trang Home sang Razor View và tích hợp với cơ sở dữ liệu. Mặc dù vẫn còn nhiều chức năng cần phát triển, dự án đã đạt khoảng 25% tiến độ, tạo nền tảng quan trọng để nhóm tiếp tục hoàn thiện các chức năng dành cho người dùng và nghệ sĩ trong những tuần tiếp theo.
