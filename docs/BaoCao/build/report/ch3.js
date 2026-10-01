const { p, h1, h2, h3, bullets, table, spacer, Figures, DIAGRAM } = require('./lib');

// Chi tiết bảng theo sql/01-db-schema.sql: [cột, kiểu, khóa, ràng buộc / ghi chú]
const PK = 'IDENTITY(1,1), NOT NULL';
const DB = [
  ['TheLoai', 'Thể loại sách', [
    ['MaTheLoai', 'INT', 'PK', PK],
    ['TenTheLoai', 'NVARCHAR(100)', 'UQ', 'NOT NULL, duy nhất'],
    ['MoTa', 'NVARCHAR(500)', '', 'NULL'],
    ['NgayTao', 'DATETIME2', '', 'NOT NULL, mặc định GETDATE()'],
  ]],
  ['TacGia', 'Tác giả', [
    ['MaTacGia', 'INT', 'PK', PK],
    ['TenTacGia', 'NVARCHAR(150)', 'UQ', 'NOT NULL, duy nhất'],
    ['QuocTich', 'NVARCHAR(100)', '', 'NULL'],
    ['GhiChu', 'NVARCHAR(500)', '', 'NULL'],
  ]],
  ['NhaXuatBan', 'Nhà xuất bản', [
    ['MaNXB', 'INT', 'PK', PK],
    ['TenNXB', 'NVARCHAR(150)', 'UQ', 'NOT NULL, duy nhất'],
    ['DiaChi', 'NVARCHAR(300)', '', 'NULL'],
    ['DienThoai', 'VARCHAR(20)', '', 'NULL'],
  ]],
  ['Sach', 'Đầu sách trong kho', [
    ['MaSach', 'INT', 'PK', PK],
    ['MaTheLoai', 'INT', 'FK', 'NOT NULL, tham chiếu TheLoai(MaTheLoai)'],
    ['MaTacGia', 'INT', 'FK', 'NOT NULL, tham chiếu TacGia(MaTacGia)'],
    ['MaNXB', 'INT', 'FK', 'NOT NULL, tham chiếu NhaXuatBan(MaNXB)'],
    ['TenSach', 'NVARCHAR(300)', '', 'NOT NULL'],
    ['NamXuatBan', 'SMALLINT', '', 'NULL'],
    ['SoTrang', 'SMALLINT', '', 'NULL'],
    ['SoLuongNhap', 'INT', '', 'NOT NULL, mặc định 0, CHECK ≥ 0'],
    ['SoLuongTon', 'INT', '', 'NOT NULL, mặc định 0, CHECK ≥ 0'],
    ['ViTri', 'NVARCHAR(50)', '', 'NULL, vị trí kệ sách'],
    ['MaQR', 'VARCHAR(100)', 'UQ', 'NULL, duy nhất'],
    ['MoTa', 'NVARCHAR(1000)', '', 'NULL'],
    ['NgayNhap', 'DATE', '', 'NOT NULL, mặc định ngày hiện tại'],
    ['TrangThai', 'TINYINT', '', 'NOT NULL, mặc định 1 (1 = có sẵn, 0 = ngừng lưu hành)'],
  ]],
  ['DocGia', 'Độc giả (học sinh)', [
    ['MaDocGia', 'INT', 'PK', PK],
    ['HoTen', 'NVARCHAR(150)', '', 'NOT NULL'],
    ['Lop', 'NVARCHAR(20)', '', 'NULL'],
    ['NgaySinh', 'DATE', '', 'NULL'],
    ['GioiTinh', 'BIT', '', 'NULL (1 = Nam, 0 = Nữ)'],
    ['DiaChi', 'NVARCHAR(300)', '', 'NULL'],
    ['Email', 'VARCHAR(150)', 'UQ', 'NULL, duy nhất khi khác NULL (filtered unique index)'],
    ['SoDienThoai', 'VARCHAR(15)', '', 'NULL'],
    ['NgayDangKy', 'DATE', '', 'NOT NULL, mặc định ngày hiện tại'],
    ['TrangThai', 'TINYINT', '', 'NOT NULL, mặc định 1 (1 = hoạt động, 0 = khóa)'],
  ]],
  ['PhieuMuon', 'Phiếu mượn', [
    ['MaPhieuMuon', 'INT', 'PK', PK],
    ['MaDocGia', 'INT', 'FK', 'NOT NULL, tham chiếu DocGia(MaDocGia)'],
    ['NgayMuon', 'DATE', '', 'NOT NULL, mặc định ngày hiện tại'],
    ['NgayHanTra', 'DATE', '', 'NOT NULL, CHECK NgayHanTra > NgayMuon'],
    ['TrangThai', 'TINYINT', '', 'NOT NULL, mặc định 1 (1 = đang mượn, 2 = đã trả, 3 = quá hạn, 4 = đã gia hạn)'],
    ['GhiChu', 'NVARCHAR(500)', '', 'NULL'],
    ['NhanVienLap', 'INT', '', 'NULL, mã tài khoản lập phiếu (tham chiếu logic, không khai báo khóa ngoại)'],
  ]],
  ['CTPhieuMuon', 'Chi tiết phiếu mượn', [
    ['MaCT', 'INT', 'PK', PK],
    ['MaPhieuMuon', 'INT', 'FK', 'NOT NULL, tham chiếu PhieuMuon; UNIQUE (MaPhieuMuon, MaSach)'],
    ['MaSach', 'INT', 'FK', 'NOT NULL, tham chiếu Sach(MaSach)'],
    ['SoLuongMuon', 'INT', '', 'NOT NULL, mặc định 1, CHECK > 0'],
    ['TrangThaiCT', 'TINYINT', '', 'NOT NULL, mặc định 1 (1 = đang mượn, 2 = đã trả)'],
  ]],
  ['GiaHan', 'Lịch sử gia hạn', [
    ['MaGiaHan', 'INT', 'PK', PK],
    ['MaPhieuMuon', 'INT', 'FK', 'NOT NULL, tham chiếu PhieuMuon(MaPhieuMuon)'],
    ['NgayGiaHan', 'DATE', '', 'NOT NULL, mặc định ngày hiện tại'],
    ['HanTraCu', 'DATE', '', 'NOT NULL'],
    ['HanTraMoi', 'DATE', '', 'NOT NULL, CHECK HanTraMoi > HanTraCu'],
    ['LanGiaHan', 'TINYINT', '', 'NOT NULL, mặc định 1'],
    ['NhanVienDuyet', 'INT', '', 'NULL, mã tài khoản duyệt (tham chiếu logic)'],
    ['GhiChu', 'NVARCHAR(300)', '', 'NULL'],
  ]],
  ['PhieuTra', 'Phiếu trả sách', [
    ['MaPhieuTra', 'INT', 'PK', PK],
    ['MaPhieuMuon', 'INT', 'FK', 'NOT NULL, tham chiếu PhieuMuon(MaPhieuMuon)'],
    ['NgayTra', 'DATE', '', 'NOT NULL, mặc định ngày hiện tại'],
    ['SoNgayMuon', 'INT', '', 'NOT NULL, mặc định 0'],
    ['SoNgayTreHan', 'INT', '', 'NOT NULL, mặc định 0'],
    ['TienPhat', 'DECIMAL(12,0)', '', 'NOT NULL, mặc định 0, CHECK ≥ 0'],
    ['DaThuPhat', 'BIT', '', 'NOT NULL, mặc định 0'],
    ['TrangThaiSach', 'TINYINT', '', 'NOT NULL, mặc định 1 (1 = bình thường, 2 = hư hỏng, 3 = mất)'],
    ['GhiChu', 'NVARCHAR(500)', '', 'NULL'],
    ['NhanVienThu', 'INT', '', 'NULL, mã tài khoản thu (tham chiếu logic)'],
  ]],
  ['VaiTro', 'Vai trò người dùng', [
    ['MaVaiTro', 'INT', 'PK', PK],
    ['TenVaiTro', 'NVARCHAR(50)', 'UQ', 'NOT NULL, duy nhất; dữ liệu mẫu: Admin, NhanVien, DocGia'],
    ['MoTa', 'NVARCHAR(200)', '', 'NULL'],
  ]],
  ['TaiKhoan', 'Tài khoản đăng nhập', [
    ['MaTaiKhoan', 'INT', 'PK', PK],
    ['MaVaiTro', 'INT', 'FK', 'NOT NULL, tham chiếu VaiTro(MaVaiTro)'],
    ['MaDocGia', 'INT', 'FK', 'NULL, tham chiếu DocGia(MaDocGia)'],
    ['TenDangNhap', 'VARCHAR(50)', 'UQ', 'NOT NULL, duy nhất'],
    ['MatKhau', 'VARCHAR(255)', '', 'NOT NULL, chuỗi băm BCrypt'],
    ['HoTen', 'NVARCHAR(150)', '', 'NOT NULL'],
    ['Email', 'VARCHAR(150)', 'UQ', 'NULL, duy nhất khi khác NULL (filtered unique index)'],
    ['SoDienThoai', 'VARCHAR(15)', '', 'NULL'],
    ['NgayTao', 'DATETIME2', '', 'NOT NULL, mặc định GETDATE()'],
    ['LanDangNhapCuoi', 'DATETIME2', '', 'NULL'],
    ['TrangThai', 'TINYINT', '', 'NOT NULL, mặc định 1 (1 = hoạt động, 0 = khóa)'],
  ]],
  ['CauHinhHeThong', 'Tham số nghiệp vụ', [
    ['MaCauHinh', 'INT', 'PK', PK],
    ['TenCauHinh', 'NVARCHAR(100)', 'UQ', 'NOT NULL, duy nhất'],
    ['GiaTri', 'NVARCHAR(200)', '', 'NOT NULL'],
    ['GhiChu', 'NVARCHAR(300)', '', 'NULL'],
  ]],
];

module.exports = function ch3() {
  const F = new Figures(3);
  const out = [];
  const add = (...xs) => out.push(...xs);
  const fig = (file, cap, opts, intro, expl) => {
    const f = F.make(DIAGRAM(file), cap, opts);
    add(p(intro.replace('{H}', f.label), { keepNext: true }), ...f.nodes);
    if (Array.isArray(expl)) add(...expl); else if (expl) add(p(expl));
  };

  add(h1('CHƯƠNG 3. THIẾT KẾ HỆ THỐNG'));

  // ===================== 3.1 =====================
  add(h2('3.1. Thiết kế giao diện và kịch bản'));
  add(h3('3.1.1. Sơ đồ điều hướng'));
  fig('H3.1-navigation.png', 'Sơ đồ điều hướng giữa các màn hình', { maxH: 14 },
    '{H} thể hiện cách người dùng di chuyển giữa các màn hình, bám theo menu bên trái của ứng dụng.',
    'Sau khi đăng nhập, người dùng vào Dashboard và chọn một trong bốn nhóm menu: Tra cứu, Quản lý sách, Nghiệp vụ và Hệ thống. Nhóm Hệ thống (màu cam) gồm Tài khoản, Cấu hình và Job Monitor, chỉ hiển thị với Admin; nếu người dùng khác truy cập trực tiếp đường dẫn sẽ bị chuyển đến trang lỗi 403.');

  add(h3('3.1.2. Bố cục chung của giao diện'));
  fig('H3.2-layout.png', 'Bố cục chung của các màn hình (_Layout.cshtml)', { maxW: 15 },
    'Mọi màn hình nghiệp vụ dùng chung trang khung _Layout.cshtml; {H} là bản phác thảo bố cục này với các vùng được đánh số.',
    bullets([
      '**(1) Thanh trên cùng:** nút thu gọn menu, tên “Thư viện THCS Thanh Xuân”, họ tên người dùng kèm nhãn vai trò và nút đăng xuất.',
      '**(2) Menu bên trái:** chia nhóm Tra cứu, Quản lý sách, Nghiệp vụ, Hệ thống (Admin).',
      '**(3) Vùng thông báo:** hiển thị kết quả thao tác thành công hoặc lỗi (TempData Success/Error).',
      '**(4) Tiêu đề trang và nút hành động:** ví dụ “Thêm mới”, “Xuất Excel”.',
      '**(5) Thanh lọc, tìm kiếm:** ô từ khóa, danh sách chọn, nút Tìm kiếm và Xóa lọc.',
      '**(6) Vùng nội dung:** bảng dữ liệu có cột trạng thái và nút thao tác, hoặc thẻ (card) chứa form nhập liệu.',
    ]));

  add(h3('3.1.3. Danh sách màn hình và kịch bản giao diện'));
  const tMH = F.tableCaption('Danh sách màn hình và kịch bản thao tác');
  add(p(`${tMH.label} tóm tắt các màn hình chính, đường dẫn và kịch bản thao tác; hình ảnh thực tế được trình bày ở mục 4.3.`, { keepNext: true }));
  add(tMH.node);
  add(table(['Màn hình', 'Đường dẫn', 'Kịch bản thao tác'], [
    ['Đăng nhập', '/Login', 'Nhập tên đăng nhập, mật khẩu (có nút hiện/ẩn mật khẩu), nhấn Đăng nhập để vào Dashboard.'],
    ['Dashboard', '/Home', 'Xem thẻ chỉ số; chọn 3/7/14 ngày cho khối “Sắp đến hạn trả”; chọn kỳ Tháng này/Năm nay/Tất cả cho bảng xếp hạng.'],
    ['Tìm kiếm sách', '/TimKiem', 'Nhập từ khóa, chọn thể loại, tích “Chỉ còn tồn kho”; kết quả hiển thị dạng thẻ kèm số lượng còn và vị trí kệ.'],
    ['Kho sách', '/Sach, /Sach/Create, /Sach/Edit/{id}, /Sach/Detail/{id}', 'Lọc theo tên, thể loại, còn tồn; thêm, sửa, xem chi tiết kèm ảnh QR, ẩn sách; xuất Excel.'],
    ['Độc giả', '/DocGia, /DocGia/Create, /DocGia/Edit/{id}, /DocGia/Detail/{id}', 'Tìm theo tên và lớp; thêm, sửa, xem lịch sử mượn, khóa/mở khóa.'],
    ['Danh mục', '/DanhMuc', 'Ba cột Thể loại, Tác giả, Nhà xuất bản; nhập tên rồi nhấn “+” để thêm, nhấn biểu tượng thùng rác để xóa.'],
    ['Phiếu mượn', '/MuonSach, /MuonSach/LapPhieu, /MuonSach/Detail/{id}, /MuonSach/QuaHan', 'Lọc theo trạng thái, khoảng ngày, mã độc giả; lập phiếu; xem chi tiết; mở danh sách quá hạn.'],
    ['Gia hạn', '/GiaHan?maPhieuMuon={id}', 'Xem hạn trả, số lần và lịch sử gia hạn; nhập ghi chú, xác nhận gia hạn.'],
    ['Trả sách', '/TraSach, /TraSach/LapPhieu, /TraSach/Detail/{id}', 'Hai thẻ “Chưa thu phạt” và “Đã hoàn tất”; lập phiếu trả theo mã phiếu mượn; nhấn Thu phạt ở trang chi tiết.'],
    ['Thống kê', '/ThongKe', 'Chọn tháng, năm, nhấn Xem thống kê; xuất Excel thống kê, lịch sử mượn, danh sách sách.'],
    ['Tài khoản (Admin)', '/TaiKhoan', 'Danh sách tài khoản; thêm, sửa, đặt lại mật khẩu, khóa/mở khóa.'],
    ['Cấu hình (Admin)', '/CauHinh', 'Sửa 5 tham số nghiệp vụ, nhấn Lưu tất cả.'],
    ['Job Monitor (Admin)', '/hangfire', 'Theo dõi lịch sử và trạng thái các tác vụ nền.'],
    ['Trang lỗi', '/Error/{mã}', 'Hiển thị thông báo khi gặp lỗi 403, 404, 500.'],
  ], [17, 30, 53], { size: 22 }));
  add(spacer());

  // ===================== 3.2 =====================
  add(h2('3.2. Thiết kế lớp thực thể'));
  fig('H3.3-class-entity.png', 'Biểu đồ lớp thực thể (LibraryManagement.Domain)', { maxH: 20.5 },
    'Tầng Domain gồm 12 lớp thực thể trong thư mục Entities và 3 lớp ngoại lệ trong thư mục Exceptions. {H} trình bày các lớp này cùng thuộc tính và bội số quan hệ.',
    [
      p('Mỗi lớp thực thể ánh xạ một bảng cùng tên; kiểu dữ liệu C# tương ứng với kiểu SQL (DATE ánh xạ sang DateOnly, cột cho phép NULL ánh xạ sang kiểu nullable như string?, int?). Các lớp chỉ chứa thuộc tính và thuộc tính điều hướng, ngăn phương thức để trống vì quy tắc nghiệp vụ được đặt trong stored procedure và repository. Quan hệ chính: một PhieuMuon có nhiều CTPhieuMuon (1..*), nhiều GiaHan (0..*) và tối đa một PhieuTra (0..1); một DocGia có nhiều PhieuMuon; Sach thuộc về một TheLoai, một TacGia, một NhaXuatBan; TaiKhoan thuộc một VaiTro và có thể liên kết với một DocGia.'),
      p('Ba lớp ngoại lệ TonKhoKhongDuException, GioiHanMuonException và QuaHanGiaHanException kế thừa System.Exception. Trong phiên bản hiện tại, các lớp này đã được định nghĩa nhưng chưa được tham chiếu; lỗi nghiệp vụ được báo bằng lệnh THROW trong stored procedure.'),
    ]);

  fig('H3.4-class-interface.png', 'Biểu đồ lớp các interface tầng Application', { maxH: 20.5 },
    'Tầng Application định nghĩa các hợp đồng (interface) mà tầng Web sử dụng; {H} liệt kê 9 interface repository và 3 interface dịch vụ cùng các phương thức chính.',
    'Các phương thức đều bất đồng bộ (trả về Task hoặc Task<T>) và nhận, trả về đối tượng DTO thay vì thực thể. Mỗi interface có một lớp hiện thực cùng tên (bỏ tiền tố I) trong project Infrastructure, được đăng ký vòng đời Scoped trong Program.cs. Riêng IExportService có phương thức ExportSachToPdfAsync nhưng phương thức này chỉ tạo nội dung HTML và chưa được gọi từ giao diện.');

  // ===================== 3.3 =====================
  add(h2('3.3. Thiết kế cơ sở dữ liệu'));
  add(h3('3.3.1. Sơ đồ thực thể – liên kết'));
  fig('H3.5-erd.png', 'Sơ đồ thực thể – liên kết (ERD) của CSDL QuanLyThuVien', { maxH: 20.5 },
    'CSDL QuanLyThuVien dùng collation Vietnamese_CI_AS và gồm 12 bảng chia thành ba nhóm. {H} là sơ đồ thực thể – liên kết theo ký hiệu chân chim (crow’s foot).',
    'Nhóm màu xanh lá gồm danh mục, sách và độc giả; nhóm màu xanh dương gồm nghiệp vụ mượn – trả – gia hạn; nhóm màu cam gồm tài khoản, vai trò và cấu hình. Quan hệ PhieuMuon – PhieuTra được cấu hình một – một trong EF Core, còn ở CSDL là khóa ngoại thông thường. Các cột NhanVienLap, NhanVienDuyet, NhanVienThu lưu mã tài khoản thực hiện thao tác nhưng không khai báo khóa ngoại. Bảng CauHinhHeThong độc lập, được các stored procedure đọc theo tên tham số.');

  add(h3('3.3.2. Mô tả chi tiết các bảng'));
  add(p('Tất cả khóa chính là số nguyên tự tăng IDENTITY(1,1). Ký hiệu: PK – khóa chính, FK – khóa ngoại, UQ – ràng buộc duy nhất.'));
  for (const [name, desc, cols] of DB) {
    const t = F.tableCaption(`Cấu trúc bảng ${name} – ${desc}`);
    add(t.node);
    add(table(['Tên cột', 'Kiểu dữ liệu', 'Khóa', 'Ràng buộc / ghi chú'], cols, [22, 20, 9, 49], { size: 22, center: [2] }));
    add(spacer());
  }
  const tCH = F.tableCaption('Dữ liệu mặc định của bảng CauHinhHeThong');
  add(p(`Bảng CauHinhHeThong được khởi tạo với 5 tham số như ${tCH.label}.`, { keepNext: true }));
  add(tCH.node);
  add(table(['TenCauHinh', 'GiaTri', 'Ý nghĩa'], [
    ['SoNgayMuonMacDinh', '14', 'Số ngày mượn tối đa'],
    ['MucPhatNgayTreHan', '2000', 'Tiền phạt mỗi ngày trễ (VNĐ)'],
    ['SoSachMuonToiDa', '3', 'Số sách một độc giả được mượn đồng thời'],
    ['SoLanGiaHanToiDa', '2', 'Số lần gia hạn tối đa mỗi phiếu mượn'],
    ['SoNgayGiaHanMoiLan', '7', 'Số ngày gia hạn thêm mỗi lần'],
  ], [32, 13, 55], { size: 22, center: [1] }));
  add(spacer());

  add(h3('3.3.3. Stored procedure, view và trigger'));
  const tSP = F.tableCaption('Các đối tượng lập trình trong CSDL');
  add(p(`Ngoài các bảng, CSDL có 12 stored procedure, 5 view và 1 trigger (${tSP.label}).`, { keepNext: true }));
  add(tSP.node);
  add(table(['Đối tượng', 'Chức năng'], [
    ['sp_LapPhieuMuon', 'Nhận danh sách sách dạng JSON; kiểm tra độc giả, giới hạn mượn, tồn kho; tạo phiếu, chi tiết và trừ tồn trong transaction (lỗi 50001–50005).'],
    ['sp_GiaHanPhieuMuon', 'Kiểm tra số lần gia hạn và hạn trả; thêm GiaHan, cập nhật hạn trả (lỗi 50010–50012).'],
    ['sp_TraSach', 'Tính số ngày trễ, tiền phạt; tạo PhieuTra; cập nhật trạng thái; hoàn tồn kho trừ sách mất (lỗi 50020).'],
    ['sp_TimKiemSach', 'Tìm kiếm sách theo nhiều tiêu chí, có phân trang.'],
    ['sp_ThongKeSachMuonNhieu, sp_ThongKeDocGiaMuonNhieu', 'Top sách và độc giả mượn nhiều theo tháng/năm.'],
    ['sp_LayPhieuMuonSapDenHan', 'Lấy phiếu sắp đến hạn trả kèm email độc giả và danh sách sách (dùng cho job nhắc hạn).'],
    ['sp_CapNhatQuaHan', 'Chuyển các phiếu đang mượn/đã gia hạn đã qua hạn trả sang trạng thái 3 (quá hạn).'],
    ['sp_Dashboard, sp_Dashboard_SapDenHan, sp_Dashboard_TopSach, sp_Dashboard_TopDocGia', 'Cung cấp số liệu cho các khối của Dashboard.'],
    ['vw_SachDayDu, vw_PhieuMuonDayDu', 'Sách kèm tên thể loại, tác giả, NXB; phiếu mượn kèm độc giả, số sách, số ngày trễ, trạng thái dạng chữ.'],
    ['vw_SachMuonNhieuNhatThang, vw_PhieuMuonQuaHan, vw_LichSuMuonTra', 'Top 10 sách tháng hiện tại; phiếu quá hạn kèm tiền phạt ước tính; lịch sử mượn trả theo độc giả.'],
    ['trg_Sach_NgaXoaKhiDangMuon', 'INSTEAD OF DELETE trên Sach: chặn xóa sách đang được mượn (lỗi 50030), ngược lại chuyển thành ẩn (TrangThai = 0).'],
  ], [38, 62], { size: 22 }));
  add(spacer());

  // ===================== 3.4 =====================
  add(h2('3.4. Biểu đồ tuần tự mức thiết kế'));
  add(p('Ở mức thiết kế, các đối tượng tham gia phản ánh đúng các tầng trong mã nguồn: giao diện Razor (boundary), Controller (control), Repository được gọi qua interface của tầng Application và SQL Server thực thi truy vấn hoặc stored procedure. Hệ thống không có tầng Service riêng cho nghiệp vụ; Controller gọi trực tiếp Repository.'));
  const seqs = [
    ['H3.6-seq-tk-dangnhap.png', 'Biểu đồ tuần tự mức thiết kế – Đăng nhập', 'quá trình đăng nhập qua LoginController và TaiKhoanRepository.',
      'LoginController nhận POST /Login và gọi XacThucAsync. TaiKhoanRepository dùng EF Core truy vấn tài khoản đang hoạt động kèm vai trò, kiểm tra mật khẩu bằng BCrypt.Verify rồi cập nhật LanDangNhapCuoi. Khi nhận được TaiKhoanDto, Controller tạo ClaimsPrincipal và gọi SignInAsync để phát hành cookie, sau đó chuyển hướng về /Home; nếu repository trả về null, View được hiển thị lại kèm thông báo lỗi.'],
    ['H3.7-seq-tk-muonsach.png', 'Biểu đồ tuần tự mức thiết kế – Lập phiếu mượn', 'luồng lập phiếu mượn từ giao diện đến stored procedure.',
      'MuonSachController kiểm tra dữ liệu, lấy mã nhân viên từ claim rồi gọi LapPhieuMuonAsync. PhieuMuonRepository gộp các dòng sách trùng, tạo chuỗi JSON và gọi sp_LapPhieuMuon bằng ADO.NET. Stored procedure thực hiện toàn bộ kiểm tra và ghi dữ liệu trong một transaction; khi vi phạm, procedure ROLLBACK và THROW, ngoại lệ được Controller bắt và hiển thị qua TempData.'],
    ['H3.8-seq-tk-trasach.png', 'Biểu đồ tuần tự mức thiết kế – Lập phiếu trả sách', 'luồng trả sách qua TraSachController và PhieuTraRepository.',
      'Controller gọi TraSachAsync với mã phiếu mượn, tình trạng sách và mã nhân viên thu. Repository gọi sp_TraSach; procedure tính SoNgayTreHan, TienPhat, ghi PhieuTra và cập nhật PhieuMuon, CTPhieuMuon, Sach rồi trả về mã phiếu trả và tiền phạt dưới dạng TraSachResultDto. Trường hợp phiếu không tồn tại hoặc đã trả, procedure ném lỗi 50020.'],
    ['H3.9-seq-tk-giahan.png', 'Biểu đồ tuần tự mức thiết kế – Gia hạn phiếu mượn', 'luồng gia hạn qua GiaHanController và GiaHanRepository.',
      'GiaHanController gọi GiaHanAsync với mã phiếu, mã nhân viên duyệt và ghi chú. sp_GiaHanPhieuMuon đếm số lần đã gia hạn, kiểm tra hạn trả, thêm bản ghi GiaHan, cập nhật PhieuMuon và trả về ngày hạn trả mới; Controller chuyển hướng về trang chi tiết phiếu mượn kèm thông báo. Các lỗi 50010, 50011, 50012 được hiển thị lại trên trang gia hạn.'],
  ];
  for (const [file, cap, intro, expl] of seqs) fig(file, cap, { maxH: 17 }, `{H} mô tả ${intro}`, expl);

  // ===================== 3.5 =====================
  add(h2('3.5. Biểu đồ thành phần'));
  fig('H3.10-component.png', 'Biểu đồ thành phần của hệ thống', { maxW: 16 },
    '{H} thể hiện bốn thành phần tương ứng bốn project và sự phụ thuộc giữa chúng.',
    'LibraryManagement.Web chứa Controllers, Views và Program.cs (đăng ký DI, Cookie Auth, Hangfire Dashboard); Web sử dụng interface và DTO của Application, đồng thời tham chiếu Infrastructure để đăng ký DI. LibraryManagement.Infrastructure hiện thực (realize) các interface của Application bằng Repositories, Services (Email, QR, Export), AppDbContext và các Job Hangfire; thành phần này truy cập SQL Server qua EF Core/ADO.NET và gửi email qua máy chủ SMTP bằng MailKit. LibraryManagement.Domain chứa thực thể và ngoại lệ, không phụ thuộc thành phần nào khác.');

  // ===================== 3.6 =====================
  add(h2('3.6. Biểu đồ triển khai'));
  fig('H3.11-deployment.png', 'Biểu đồ triển khai của hệ thống', { maxH: 17 },
    '{H} mô tả các nút phần cứng, môi trường thực thi và giao thức kết nối khi vận hành hệ thống.',
    'Máy trạm của nhân viên chỉ cần trình duyệt web; giao diện tải Bootstrap từ CDN jsDelivr. Máy chủ ứng dụng chạy .NET 8 Runtime với Kestrel, chứa artifact LibraryManagement.Web.dll (kèm các DLL của Application, Infrastructure, Domain) và Hangfire Server thực thi hai job định kỳ. Ứng dụng kết nối SQL Server qua giao thức TDS (cổng 1433) theo chuỗi kết nối trong appsettings.json; CSDL QuanLyThuVien chứa cả dữ liệu nghiệp vụ và schema HangFire. Email nhắc hạn được gửi qua máy chủ SMTP của Gmail, cổng 587 có TLS.');

  return out;
};
