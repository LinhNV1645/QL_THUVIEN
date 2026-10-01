const { Paragraph, TextRun, ShadingType } = require('docx');
const { p, h1, h2, h3, bullets, numbered, table, spacer, Figures, SHOT, CM } = require('./lib');

const code = (lines) => lines.map((t, i) => new Paragraph({
  indent: { left: CM },
  shading: { type: ShadingType.CLEAR, color: 'auto', fill: 'F2F2F2' },
  spacing: { before: i === 0 ? 60 : 0, after: i === lines.length - 1 ? 120 : 0, line: 240 },
  children: [new TextRun({ text: t, font: 'Consolas', size: 20 })],
}));

// [file, caption, intro, description, maxW (cm, mặc định 15)]
const SHOTS = [
  ['Login.png', 'Màn hình đăng nhập',
    'Khi chưa đăng nhập, mọi đường dẫn nghiệp vụ đều chuyển về màn hình đăng nhập ({H}).',
    'Màn hình gồm biểu tượng sách, tiêu đề “Thư viện THCS Thanh Xuân – Hệ thống quản lý thư viện”, ô Tên đăng nhập, ô Mật khẩu có nút hiện/ẩn mật khẩu và nút Đăng nhập. Nếu thông tin sai hoặc tài khoản bị khóa, hệ thống hiển thị lại màn hình kèm thông báo lỗi.'],
  ['Dashboard.png', 'Màn hình Dashboard',
    'Sau khi đăng nhập thành công, người dùng được đưa đến Dashboard ({H}).',
    'Hàng thẻ chỉ số phía trên hiển thị tổng đầu sách, số phiếu đang mượn, số phiếu quá hạn, tổng độc giả, tổng sách tồn kho (kèm tỉ lệ đang mượn), tiền phạt chưa thu và số lượt mượn trong tháng. Khối “Sắp đến hạn trả” cho phép chọn 3, 7 hoặc 14 ngày; khối “Quá hạn lâu nhất” liệt kê các phiếu chưa trả kèm tiền phạt ước tính. Phần “Bảng xếp hạng” hiển thị Top 10 sách và Top 10 độc giả mượn nhiều nhất theo kỳ Tháng này, Năm nay hoặc Tất cả.'],
  ['Timkiem_Ten.png', 'Tìm kiếm sách theo tên',
    '{H} minh họa chức năng tìm kiếm sách theo từ khóa.',
    'Với từ khóa “Dế”, hệ thống trả về một kết quả là cuốn “Dế Mèn Phiêu Lưu Ký” của Tô Hoài. Mỗi kết quả được trình bày dạng thẻ gồm tên sách, tác giả, thể loại, nhà xuất bản, năm xuất bản, số lượng còn và vị trí kệ. Màn hình này cho phép truy cập không cần đăng nhập.'],
  ['Timkiem_theloai.png', 'Tìm kiếm sách theo thể loại',
    'Ngoài từ khóa, người dùng có thể lọc theo thể loại như {H}.',
    'Khi chọn thể loại “Văn học”, hệ thống hiển thị 7 cuốn sách thuộc thể loại này. Ô “Chỉ còn tồn kho” giúp loại bỏ các đầu sách đã hết.'],
  ['QL_Khosach.png', 'Màn hình quản lý kho sách',
    'Màn hình Kho sách ({H}) là nơi nhân viên quản lý toàn bộ đầu sách.',
    'Thanh lọc cho phép tìm theo tên sách, thể loại và chỉ hiển thị sách còn tồn kho. Bảng dữ liệu gồm các cột STT, Tên sách (kèm vị trí kệ), Tác giả, Thể loại, Tồn kho, Trạng thái và Thao tác (xem chi tiết, sửa, ẩn sách). Góc trên bên phải có nút Thêm sách và Xuất Excel.'],
  ['QL_Khosach_them_sua.png', 'Form thêm sách mới',
    'Khi nhấn Thêm sách, hệ thống mở form như {H}.',
    'Form gồm các trường bắt buộc Tên sách, Thể loại, Tác giả, Nhà xuất bản, Số lượng nhập và các trường tùy chọn Năm xuất bản, Số trang, Vị trí kệ, Mô tả. Thể loại, tác giả, nhà xuất bản được chọn từ danh mục có sẵn. Khi lưu, hệ thống tự sinh mã QR cho sách và đặt số lượng tồn bằng số lượng nhập.'],
  ['QL_Docgia.png', 'Màn hình quản lý độc giả',
    '{H} là màn hình danh sách độc giả.',
    'Nhân viên tìm độc giả theo họ tên và lớp. Bảng hiển thị Họ tên, Lớp, Email, Số điện thoại, Ngày đăng ký, Trạng thái và các nút xem chi tiết, sửa, khóa/mở khóa. Nút Thêm độc giả nằm ở góc trên bên phải.'],
  ['QL_Docgia_them_sua.png', 'Form thêm độc giả',
    'Form nhập thông tin độc giả được thể hiện ở {H}.',
    'Các trường gồm Họ tên (bắt buộc), Lớp, Ngày sinh, Giới tính, Email, Số điện thoại và Địa chỉ. Email được dùng để gửi thư nhắc hạn trả sách; độc giả không có email sẽ không nhận được nhắc nhở.'],
  ['QL_Danhmuc.png', 'Màn hình quản lý danh mục',
    'Ba danh mục dùng chung cho sách được quản lý trên cùng một màn hình ({H}).',
    'Màn hình chia ba cột Thể loại, Tác giả và Nhà xuất bản, mỗi cột có ô nhập tên kèm nút “+” để thêm và biểu tượng thùng rác để xóa từng mục. Phiên bản hiện tại hỗ trợ thêm và xóa, chưa hỗ trợ sửa tên danh mục.'],
  ['QL_Phieumuon.png', 'Màn hình danh sách phiếu mượn',
    '{H} là màn hình quản lý phiếu mượn.',
    'Thanh lọc gồm trạng thái, khoảng ngày mượn và mã độc giả. Bảng hiển thị Mã phiếu, Độc giả, Lớp, Ngày mượn, Hạn trả, Trạng thái (Đang mượn, Đã trả, Quá hạn kèm số ngày trễ), Số sách và Thao tác. Nút Quá hạn mở danh sách phiếu quá hạn, nút Lập phiếu mượn mở form lập phiếu.'],
  ['QL_Phieumuon_them.png', 'Màn hình lập phiếu mượn',
    'Màn hình lập phiếu mượn được thể hiện ở {H}.',
    'Sau khi chọn độc giả, hệ thống hiển thị lớp, số sách đang mượn và cảnh báo nếu độc giả có phiếu quá hạn chưa trả (như trường hợp trong hình). Phần “Danh sách sách mượn” cho phép thêm nhiều dòng sách, mỗi dòng hiển thị số lượng tồn và ô nhập số lượng. Khi nhấn “Xác nhận lập phiếu”, sp_LapPhieuMuon kiểm tra giới hạn mượn và tồn kho rồi tạo phiếu.'],
  ['QL_Trasach.png', 'Màn hình quản lý trả sách',
    '{H} là danh sách phiếu trả sách.',
    'Danh sách được chia hai thẻ “Chưa thu phạt” và “Đã hoàn tất”, mỗi thẻ có số lượng phiếu. Bảng gồm Mã phiếu, Độc giả, Ngày trả, Số ngày trễ, Tiền phạt, Tình trạng sách (Bình thường, Hư hỏng, Mất) và nút xem chi tiết. Nút Lập phiếu trả ở góc trên bên phải.'],
  ['QL_Trasach_themmoi.png', 'Màn hình lập phiếu trả sách',
    'Quy trình trả sách bắt đầu tại màn hình {H}.',
    'Nhân viên nhập mã phiếu mượn và nhấn Tìm phiếu; hệ thống hiển thị độc giả, ngày mượn, hạn trả và danh sách sách. Khối “Xác nhận trả sách” cho chọn tình trạng sách, nhập ghi chú và hiển thị tiền phạt ước tính gồm phạt trễ hạn (số ngày trễ × mức phạt) và phạt theo tình trạng sách trước khi nhấn “Xác nhận trả sách”.'],
  ['QL_ThuPhat.png', 'Chi tiết phiếu trả và thu phạt',
    'Sau khi trả sách, hệ thống chuyển đến trang chi tiết phiếu trả ({H}).',
    'Ví dụ trong hình là phiếu trả sách trễ 2 ngày và sách bị mất, tiền phạt 204.000 đồng, đúng với công thức trong sp_TraSach: 2 ngày × 2.000 đồng + 200.000 đồng phạt mất sách. Trang hiển thị thông tin phiếu trả, trạng thái “Chưa thu phạt” và nút Thu phạt; sau khi thu, phiếu chuyển sang thẻ “Đã hoàn tất”.'],
  ['QL_Thongke.png', 'Màn hình thống kê',
    '{H} là màn hình thống kê theo tháng.',
    'Người dùng chọn tháng, năm rồi nhấn Xem thống kê để xem Top 10 sách mượn nhiều nhất (số lượt mượn) và Top 10 độc giả mượn nhiều nhất (số lần mượn, tổng sách). Ba nút Xuất thống kê, Lịch sử mượn và DS sách xuất dữ liệu ra tệp Excel.'],
  ['QL_Taikhoan.png', 'Màn hình quản lý tài khoản (Admin)',
    'Chức năng quản lý tài khoản chỉ dành cho Admin ({H}).',
    'Bảng hiển thị Tên đăng nhập, Họ tên, Vai trò, Email, Trạng thái và các nút sửa, đặt lại mật khẩu, khóa/mở khóa; Admin không thể tự đổi vai trò hoặc tự khóa chính mình.'],
  ['QL_Taikhoan_themmoi.png', 'Form thêm tài khoản (Admin)',
    'Khi tạo tài khoản mới, Admin dùng form ở {H}.',
    'Các trường bắt buộc gồm Vai trò, Tên đăng nhập, Mật khẩu và Họ tên. Tên đăng nhập không được trùng, mật khẩu tối thiểu 6 ký tự và được băm bằng BCrypt.'],
  ['QL_Cauhinh.png', 'Màn hình cấu hình hệ thống (Admin)',
    'Các tham số nghiệp vụ được Admin điều chỉnh tại màn hình {H}.',
    'Bảng liệt kê 5 tham số nghiệp vụ, mỗi giá trị là số nguyên không âm; trong hình, số ngày mượn mặc định đã được đổi thành 10 (giá trị khởi tạo là 14). Nút “Lưu tất cả” ghi đồng thời các thay đổi.', 13.5],
];

module.exports = () => {
  const F = new Figures(4);
  const out = [];
  const add = (...xs) => out.push(...xs);

  add(h1('CHƯƠNG 4. CÀI ĐẶT VÀ KẾT QUẢ'));

  // ===================== 4.1 =====================
  add(h2('4.1. Yêu cầu cài đặt'));
  add(h3('4.1.1. Yêu cầu phần cứng'));
  const tHW = F.tableCaption('Yêu cầu phần cứng tối thiểu theo công bố của Microsoft');
  add(p(`Yêu cầu phần cứng được lấy theo công bố chính thức của Microsoft cho SQL Server 2019 (máy chủ CSDL) và Visual Studio 2022 (máy phát triển), tổng hợp trong ${tHW.label}. Máy trạm của nhân viên chỉ cần trình duyệt web kết nối được tới máy chủ.`, { keepNext: true }));
  add(tHW.node);
  add(table(['Thành phần', 'SQL Server 2019 [10]', 'Visual Studio 2022 [11]'], [
    ['Bộ xử lý', 'x64, tối thiểu 1,4 GHz; khuyến nghị 2,0 GHz trở lên', 'x64 hoặc ARM64; khuyến nghị 4 nhân trở lên'],
    ['Bộ nhớ RAM', 'Express: tối thiểu 512 MB, khuyến nghị 1 GB; các phiên bản khác: tối thiểu 1 GB, khuyến nghị từ 4 GB', 'Tối thiểu 4 GB; khuyến nghị 16 GB'],
    ['Ổ đĩa', 'Tối thiểu 6 GB dung lượng trống', 'Từ 850 MB đến 210 GB tùy thành phần cài đặt (thông thường 20–50 GB)'],
    ['Màn hình', 'Super-VGA 800×600 trở lên', 'Độ phân giải tối thiểu 1366×768'],
    ['Hệ điều hành', 'Windows 10 TH1 1507 trở lên hoặc Windows Server 2016 trở lên', 'Windows 11 hoặc Windows Server 2016 trở lên'],
  ], [18, 41, 41], { size: 22 }));
  add(spacer());
  add(p('Đối với .NET 8, Microsoft chỉ công bố danh sách hệ điều hành được hỗ trợ [12]: Windows 10, Windows 11 (các phiên bản còn được hỗ trợ), Windows Server 2012 đến 2025, trên kiến trúc x64, x86 và Arm64; tài liệu này không quy định cấu hình phần cứng tối thiểu riêng.'));

  add(h3('4.1.2. Yêu cầu phần mềm'));
  const tSW = F.tableCaption('Yêu cầu phần mềm');
  add(p(`${tSW.label} liệt kê các phần mềm cần có để chạy hệ thống.`, { keepNext: true }));
  add(tSW.node);
  add(table(['Phần mềm', 'Phiên bản', 'Ghi chú'], [
    ['.NET SDK', '8.0', 'Các project dùng TargetFramework net8.0; khi triển khai chỉ cần ASP.NET Core Runtime 8.0.'],
    ['SQL Server', '2019 trở lên (hoặc bản Express)', 'Lưu CSDL QuanLyThuVien và dữ liệu của Hangfire.'],
    ['Công cụ phát triển', 'Visual Studio 2022 hoặc VS Code kèm C# Extension', 'Mở solution LibraryManagement.sln.'],
    ['Công cụ chạy script SQL', '—', 'Dùng để thực thi các tệp trong thư mục sql/.'],
    ['Trình duyệt web', '—', 'Truy cập giao diện; cần Internet để tải Bootstrap 5.3.3 và Bootstrap Icons từ CDN jsDelivr.'],
    ['Tài khoản SMTP', '—', 'Gửi email nhắc hạn (cấu hình mẫu dùng Gmail, cổng 587).'],
  ], [24, 30, 46], { size: 22 }));
  add(spacer());
  add(p('Các thư viện NuGet (EF Core, BCrypt.Net-Next, MailKit, QRCoder, ClosedXML, Hangfire…) được khai báo trong tệp .csproj và tự động tải về khi build.'));

  add(h3('4.1.3. Các bước cài đặt'));
  add(p('**Bước 1 – Tạo cơ sở dữ liệu.** Thực thi lần lượt các script trong thư mục LibraryManagement/sql:', { keepNext: true }));
  add(...numbered([
    '`01-db-schema.sql`: tạo CSDL QuanLyThuVien (collation Vietnamese_CI_AS), 12 bảng, chỉ mục, dữ liệu vai trò và cấu hình mặc định. Lưu ý script sẽ xóa CSDL QuanLyThuVien nếu đã tồn tại.',
    '`02-stored-procedures.sql`: tạo các stored procedure nghiệp vụ.',
    '`03-views-triggers.sql`: tạo view, trigger và sp_CapNhatQuaHan.',
    '`04-seed-data.sql`: thêm dữ liệu mẫu (danh mục, sách, độc giả) và hai tài khoản đăng nhập.',
    '`dashboard.sql`: tạo các stored procedure phục vụ Dashboard (chạy sau script 03, có thể chạy lại nhiều lần).',
  ]));
  add(p('Tệp `05-fix-existing-db.sql` chỉ dùng để cập nhật CSDL đã tạo bằng phiên bản script cũ; tệp `06-them-10-sach.sql` bổ sung 10 đầu sách mẫu, không bắt buộc.'));

  add(p('**Bước 2 – Cấu hình ứng dụng.** Mở tệp `LibraryManagement.Web/appsettings.json` và khai báo chuỗi kết nối, thông tin máy chủ SMTP theo mẫu (các giá trị trong ngoặc nhọn thay bằng thông tin thực tế; không đưa mật khẩu thật vào tài liệu hoặc kho mã nguồn):', { keepNext: true }));
  add(...code([
    '"ConnectionStrings": {',
    '  "DefaultConnection": "Server={máy chủ};Database=QuanLyThuVien;',
    '      User Id={tài khoản};Password={mật khẩu};',
    '      TrustServerCertificate=True;MultipleActiveResultSets=True"',
    '},',
    '"EmailSettings": {',
    '  "Host": "smtp.gmail.com", "Port": 587, "EnableSsl": true,',
    '  "UserName": "{email gửi}", "Password": "{mật khẩu ứng dụng}",',
    '  "DisplayName": "{tên hiển thị}"',
    '}',
  ]));

  add(p('**Bước 3 – Build và chạy.** Tại thư mục LibraryManagement, chạy các lệnh sau (hoặc mở LibraryManagement.sln bằng Visual Studio và nhấn F5 với hồ sơ https):', { keepNext: true }));
  add(...code([
    'dotnet restore',
    'dotnet run --project LibraryManagement.Web --launch-profile https',
  ]));
  add(p('**Bước 4 – Truy cập hệ thống.** Mở trình duyệt tại https://localhost:7134 (hoặc http://localhost:5292) và đăng nhập bằng tài khoản mẫu: admin / Admin@123 (vai trò Admin) hoặc nhanvien01 / NhanVien@123 (vai trò Nhân viên). Nên đổi mật khẩu các tài khoản mẫu sau lần đăng nhập đầu tiên.'));
  add(p('**Bước 5 – Kiểm tra tác vụ nền.** Đăng nhập bằng Admin và truy cập /hangfire (menu Job Monitor) để xác nhận hai tác vụ định kỳ cap-nhat-qua-han (00:05 hằng ngày) và nhac-nho-han-tra (07:00 hằng ngày) đã được đăng ký.'));

  // ===================== 4.2 =====================
  add(h2('4.2. Kết quả cài đặt'));
  add(p('Solution được build thành công bằng lệnh dotnet build với 0 lỗi. Sau khi chạy các script và khởi động ứng dụng, hệ thống hoạt động với dữ liệu mẫu và hai tài khoản nêu trên.'));
  const tKQ = F.tableCaption('Tổng hợp các thành phần đã cài đặt');
  add(p(`${tKQ.label} tổng hợp các thành phần đã được cài đặt trong mã nguồn.`, { keepNext: true }));
  add(tKQ.node);
  add(table(['Hạng mục', 'Kết quả'], [
    ['Kiến trúc', '4 project: Domain, Application, Infrastructure, Web'],
    ['Tầng giao diện', '13 controller, 32 tệp Razor view (.cshtml)'],
    ['Cơ sở dữ liệu', '12 bảng, 12 stored procedure, 5 view, 1 trigger'],
    ['Tác vụ nền', '2 tác vụ Hangfire định kỳ: cập nhật quá hạn, gửi email nhắc hạn'],
    ['Chức năng nghiệp vụ', 'Đăng nhập, phân quyền; Dashboard; tìm kiếm sách; quản lý sách (kèm mã QR), độc giả, danh mục; lập phiếu mượn, gia hạn, trả sách và thu phạt; thống kê; xuất Excel; quản lý tài khoản; cấu hình hệ thống'],
  ], [25, 75], { size: 22 }));
  add(spacer());
  add(p('Một số điểm chưa hoàn thiện trong phiên bản hiện tại: chức năng xuất PDF mới tạo nội dung HTML và chưa được gọi từ giao diện; các lớp ngoại lệ ở tầng Domain chưa được sử dụng; danh mục chưa hỗ trợ sửa tên.'));

  // ===================== 4.3 =====================
  add(h2('4.3. Một số hình ảnh của hệ thống'));
  add(p('Mục này trình bày ảnh chụp màn hình thực tế của hệ thống khi chạy với dữ liệu mẫu, theo thứ tự: đăng nhập, Dashboard, tra cứu, quản lý sách, nghiệp vụ mượn – trả và các chức năng của Admin.'));
  for (const [file, cap, intro, desc, maxW = 15] of SHOTS) {
    const f = F.make(SHOT(file), cap, { maxW });
    add(p(intro.replace('{H}', f.label), { keepNext: true }), ...f.nodes, p(desc));
  }

  return out;
};
