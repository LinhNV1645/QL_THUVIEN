const { p, h1, h2, h3, bullets, table, spacer, Figures } = require('./lib');

module.exports = function ch1() {
  const F = new Figures(1);
  const tCN = F.tableCaption('Các công nghệ sử dụng và lý do lựa chọn');

  return [
    h1('CHƯƠNG 1. TỔNG QUAN VỀ ĐỀ TÀI'),

    h2('1.1. Giới thiệu đề tài'),

    h3('1.1.1. Bối cảnh'),
    p('Thư viện là nơi lưu trữ sách giáo khoa, sách tham khảo và truyện đọc phục vụ học sinh, giáo viên của Trường THCS Thanh Xuân. Theo khảo sát ban đầu của đề tài, thư viện hiện được quản lý theo phương pháp thủ công: thủ thư ghi chép phiếu mượn bằng tay vào sổ, việc tra cứu sách mất nhiều thời gian và không có cơ chế nhắc nhở khi sách sắp đến hạn trả. Hệ quả là nhiều sách bị mất, thất lạc hoặc trả trễ mà không được xử lý kịp thời.'),
    p('Từ thực tế đó, đề tài *“Quản lý thư viện cho Trường THCS Thanh Xuân”* được thực hiện nhằm xây dựng một ứng dụng web giúp nhân viên thư viện tin học hóa các nghiệp vụ hằng ngày: quản lý kho sách, quản lý độc giả, lập phiếu mượn – trả, gia hạn, tính tiền phạt và thống kê.'),

    h3('1.1.2. Mục tiêu'),
    p('Đề tài hướng tới các mục tiêu cụ thể sau:'),
    ...bullets([
      '**Quản lý kho sách:** theo dõi số lượng nhập, số lượng tồn, vị trí kệ và trạng thái của từng đầu sách; mỗi đầu sách có một mã QR riêng.',
      '**Quản lý mượn – trả:** lập phiếu nhanh, kiểm soát giới hạn số sách được mượn, tự động tính tiền phạt khi trả trễ hoặc khi sách bị hư hỏng, mất.',
      '**Nhắc nhở tự động:** gửi email cho độc giả khi phiếu mượn sắp đến hạn trả và tự động đánh dấu các phiếu quá hạn.',
      '**Tra cứu nhanh:** tìm sách theo từ khóa và thể loại, lọc sách còn tồn kho.',
      '**Thống kê – báo cáo:** thống kê sách được mượn nhiều, độc giả mượn nhiều theo tháng; xuất báo cáo ra tệp Excel.',
      '**Phân quyền:** tách biệt quyền của quản trị viên (Admin) và nhân viên thư viện (NhanVien).',
    ]),

    h3('1.1.3. Phạm vi'),
    p('Hệ thống phục vụ nội bộ nhà trường, vận hành trên mạng LAN hoặc máy chủ nội bộ. Người sử dụng trực tiếp là Admin và nhân viên thư viện. Phiên bản hiện tại chưa có cổng thông tin để học sinh tự đăng nhập; tuy nhiên trang *Tìm kiếm sách* được cấu hình cho phép truy cập không cần đăng nhập. Các nội dung nằm ngoài phạm vi gồm: ứng dụng di động, REST API, quét mã vạch ISBN và ảnh bìa sách.'),

    h3('1.1.4. Đối tượng sử dụng'),
    ...bullets([
      '**Admin (quản trị viên):** có toàn bộ quyền của nhân viên, ngoài ra quản lý tài khoản người dùng, cấu hình tham số nghiệp vụ và giám sát tác vụ nền (Hangfire Dashboard).',
      '**Nhân viên thư viện:** thực hiện nghiệp vụ hằng ngày như quản lý sách, độc giả, danh mục, lập phiếu mượn – trả, gia hạn, thu phạt và xem thống kê.',
      '**Độc giả (học sinh):** là đối tượng được quản lý trong hệ thống; nhận email nhắc hạn trả sách. Cơ sở dữ liệu có sẵn vai trò DocGia nhưng chưa có giao diện dành riêng.',
    ]),

    h3('1.1.5. Các chức năng chính'),
    ...bullets([
      'Đăng nhập, đăng xuất; phân quyền theo vai trò Admin và NhanVien.',
      'Dashboard tổng quan: tổng đầu sách, số phiếu đang mượn, quá hạn, tổng độc giả, tồn kho, tiền phạt chưa thu, lượt mượn trong tháng; danh sách phiếu sắp đến hạn, phiếu quá hạn lâu nhất và bảng xếp hạng.',
      'Quản lý kho sách (thêm, sửa, xem chi tiết kèm mã QR, ẩn sách) và quản lý danh mục thể loại, tác giả, nhà xuất bản (thêm, xóa).',
      'Quản lý độc giả: thêm, sửa, xem lịch sử mượn, khóa/mở khóa.',
      'Lập phiếu mượn, gia hạn phiếu mượn, lập phiếu trả sách, thu tiền phạt; xem danh sách phiếu quá hạn.',
      'Tìm kiếm sách theo từ khóa, thể loại, tình trạng tồn kho.',
      'Thống kê theo tháng và xuất Excel (danh sách sách, thống kê, lịch sử mượn).',
      'Quản lý tài khoản và cấu hình hệ thống (dành cho Admin).',
      'Tác vụ nền: gửi email nhắc hạn trả lúc 07:00 và cập nhật trạng thái quá hạn lúc 00:05 hằng ngày.',
    ]),

    h2('1.2. Công nghệ sử dụng'),
    p(`Hệ thống được xây dựng trên nền tảng .NET 8 theo mô hình Clean Architecture gồm bốn project: Domain, Application, Infrastructure và Web. ${tCN.label} liệt kê các công nghệ, phiên bản (lấy theo các tệp .csproj và giao diện của mã nguồn) cùng lý do lựa chọn.`),
    tCN.node,
    table(['Công nghệ', 'Phiên bản', 'Vai trò và lý do lựa chọn'], [
      ['ASP.NET Core MVC (C#)', '8.0', 'Khung ứng dụng web theo mô hình Model – View – Controller với Razor View; bản LTS, hỗ trợ sẵn Dependency Injection, xác thực và phân quyền.'],
      ['Clean Architecture', '4 project', 'Tách logic nghiệp vụ khỏi hạ tầng kỹ thuật; tầng Web chỉ phụ thuộc vào interface của tầng Application, dễ thay thế CSDL hoặc thư viện gửi email.'],
      ['Entity Framework Core', '8.0.13', 'ORM ánh xạ 12 lớp thực thể với bảng dữ liệu, dùng cho các thao tác CRUD đơn giản.'],
      ['ADO.NET + Stored Procedure', '–', 'Các nghiệp vụ phức tạp (mượn, trả, gia hạn, thống kê) được viết trong stored procedure, chạy trong transaction để bảo đảm toàn vẹn dữ liệu.'],
      ['Microsoft SQL Server', '2019 trở lên', 'Hệ quản trị CSDL quan hệ; collation Vietnamese_CI_AS hỗ trợ tiếng Việt; đồng thời là nơi lưu dữ liệu của Hangfire.'],
      ['Cookie Authentication', 'Có sẵn trong ASP.NET Core', 'Xác thực bằng cookie (hết hạn sau 8 giờ, gia hạn trượt), phân quyền bằng policy AdminOnly và Staff.'],
      ['BCrypt.Net-Next', '4.0.3', 'Băm mật khẩu một chiều, không thể giải ngược.'],
      ['Hangfire + Hangfire.SqlServer', '1.8.20', 'Chạy tác vụ định kỳ độc lập với request người dùng; có Dashboard theo dõi lịch sử job.'],
      ['MailKit', '4.18.1', 'Gửi email nhắc hạn trả qua SMTP có mã hóa SSL/TLS.'],
      ['QRCoder', '1.6.0', 'Sinh ảnh mã QR (PNG) cho từng đầu sách.'],
      ['ClosedXML', '0.102.3', 'Tạo tệp Excel (.xlsx) cho các báo cáo.'],
      ['Bootstrap / Bootstrap Icons', '5.3.3 / 1.11.3', 'Giao diện đáp ứng (responsive), nạp từ CDN jsDelivr.'],
      ['Newtonsoft.Json', '13.0.3', 'Tuần tự hóa JSON cho MVC (AddNewtonsoftJson).'],
    ], [27, 17, 56], { size: 22, center: [1] }),
    spacer(),
    p('Việc kết hợp EF Core cho thao tác đơn giản và stored procedure cho nghiệp vụ nhiều bước giúp mã nguồn ngắn gọn ở tầng ứng dụng, trong khi các quy tắc như kiểm tra tồn kho, giới hạn mượn, tính tiền phạt được thực thi tập trung tại CSDL trong cùng một transaction. Các tham số nghiệp vụ (số ngày mượn, mức phạt, số sách tối đa, số lần và số ngày gia hạn) được lưu trong bảng CauHinhHeThong nên có thể thay đổi mà không cần sửa mã nguồn.'),
  ];
};
