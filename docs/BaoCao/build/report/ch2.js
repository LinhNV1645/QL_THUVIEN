const { p, h1, h2, h3, bullets, table, scenario, spacer, Figures, DIAGRAM } = require('./lib');

module.exports = function ch2() {
  const F = new Figures(2);
  const out = [];
  const add = (...xs) => out.push(...xs);

  add(h1('CHƯƠNG 2. PHÂN TÍCH YÊU CẦU'));

  // ===================== 2.1 =====================
  add(h2('2.1. Khảo sát yêu cầu'));
  add(p('Yêu cầu của hệ thống được tổng hợp từ tài liệu mô tả dự án và đối chiếu với mã nguồn đã cài đặt. Tác nhân sử dụng gồm Nhân viên thư viện (vai trò NhanVien) và Admin; ngoài ra hệ thống có các tác vụ nền chạy tự động bằng Hangfire.'));

  add(h3('2.1.1. Yêu cầu chức năng'));
  const tF = F.tableCaption('Danh sách yêu cầu chức năng');
  add(p(`${tF.label} liệt kê các yêu cầu chức năng, mô tả ngắn gọn và tác nhân thực hiện.`, { keepNext: true }));
  add(tF.node);
  add(table(['Mã', 'Chức năng', 'Mô tả', 'Tác nhân'], [
    ['F01', 'Đăng nhập, đăng xuất', 'Xác thực bằng tên đăng nhập và mật khẩu (BCrypt); tài khoản bị khóa không đăng nhập được.', 'NV, Admin'],
    ['F02', 'Dashboard', 'Hiển thị các chỉ số tổng quan, phiếu sắp đến hạn (3/7/14 ngày), phiếu quá hạn lâu nhất, xếp hạng sách và độc giả.', 'NV, Admin'],
    ['F03', 'Quản lý kho sách', 'Thêm, sửa, xem chi tiết kèm mã QR, ẩn sách (không cho ẩn khi đang được mượn); lọc theo tên, thể loại, còn tồn kho; xuất Excel.', 'NV, Admin'],
    ['F04', 'Quản lý danh mục', 'Thêm, xóa thể loại, tác giả, nhà xuất bản.', 'NV, Admin'],
    ['F05', 'Quản lý độc giả', 'Thêm, sửa, xem chi tiết và lịch sử mượn, khóa/mở khóa; tìm theo tên và lớp.', 'NV, Admin'],
    ['F06', 'Lập phiếu mượn', 'Chọn độc giả và sách; kiểm tra trạng thái độc giả, giới hạn số sách, tồn kho; hạn trả = ngày mượn + số ngày mượn mặc định.', 'NV, Admin'],
    ['F07', 'Gia hạn phiếu mượn', 'Cộng thêm số ngày gia hạn theo cấu hình; giới hạn số lần gia hạn; không gia hạn phiếu đã trả hoặc quá hạn.', 'NV, Admin'],
    ['F08', 'Trả sách, thu phạt', 'Lập phiếu trả; tiền phạt = số ngày trễ × mức phạt/ngày, cộng 50.000đ nếu hư hỏng, 200.000đ nếu mất; ghi nhận thu phạt.', 'NV, Admin'],
    ['F09', 'Phiếu quá hạn', 'Xem danh sách phiếu mượn đã quá hạn trả.', 'NV, Admin'],
    ['F10', 'Tìm kiếm sách', 'Tìm theo từ khóa, thể loại, chỉ sách còn tồn kho; kết quả phân trang.', 'NV, Admin'],
    ['F11', 'Thống kê, xuất Excel', 'Top sách và độc giả mượn nhiều theo tháng/năm; xuất Excel danh sách sách, thống kê, lịch sử mượn.', 'NV, Admin'],
    ['F12', 'Quản lý tài khoản', 'Thêm, cập nhật, đặt lại mật khẩu, khóa/mở khóa tài khoản.', 'Admin'],
    ['F13', 'Cấu hình hệ thống', 'Sửa 5 tham số nghiệp vụ lưu trong bảng CauHinhHeThong.', 'Admin'],
    ['F14', 'Giám sát tác vụ nền', 'Xem Hangfire Dashboard tại /hangfire.', 'Admin'],
    ['F15', 'Nhắc hạn trả qua email', 'Lúc 07:00 hằng ngày gửi email cho độc giả có phiếu đến hạn trong 3 ngày tới.', 'Hangfire'],
    ['F16', 'Cập nhật quá hạn', 'Lúc 00:05 hằng ngày chuyển các phiếu đã quá hạn trả sang trạng thái Quá hạn.', 'Hangfire'],
  ], [8, 20, 56, 16], { size: 22, center: [0, 3] }));
  add(spacer());

  add(h3('2.1.2. Yêu cầu phi chức năng'));
  const tN = F.tableCaption('Yêu cầu phi chức năng');
  add(tN.node);
  add(table(['Hạng mục', 'Yêu cầu'], [
    ['Bảo mật', 'Mật khẩu lưu dạng băm BCrypt; xác thực bằng cookie (hết hạn 8 giờ, gia hạn trượt); phân quyền theo policy AdminOnly/Staff; các form POST dùng chống giả mạo (AntiForgeryToken); chuyển hướng HTTPS.'],
    ['Toàn vẹn dữ liệu', 'Nghiệp vụ mượn, trả, gia hạn chạy trong transaction của stored procedure; ràng buộc CHECK, UNIQUE, khóa ngoại tại CSDL; sách đang được mượn không thể bị ẩn hoặc xóa.'],
    ['Hiệu năng', 'Danh sách có phân trang; tìm kiếm dùng stored procedure và chỉ mục (index) trên các cột tìm kiếm. Tài liệu dự án đặt mục tiêu trang danh sách tải dưới 2 giây với 1.000 bản ghi.'],
    ['Tính sẵn sàng', 'Tác vụ gửi email và cập nhật quá hạn chạy nền bằng Hangfire, không chặn thao tác của người dùng.'],
    ['Khả năng bảo trì', 'Clean Architecture 4 tầng; tham số nghiệp vụ thay đổi qua giao diện cấu hình, không cần triển khai lại.'],
    ['Tính dễ dùng', 'Giao diện Bootstrap 5 đáp ứng, tiếng Việt, menu bên trái theo nhóm chức năng, thông báo kết quả sau mỗi thao tác.'],
  ], [22, 78], { size: 22 }));
  add(spacer());

  // ===================== 2.2 =====================
  add(h2('2.2. Biểu đồ use case'));
  add(h3('2.2.1. Các tác nhân'));
  add(...bullets([
    '**Nhân viên:** nhân viên thư viện, thực hiện các nghiệp vụ hằng ngày.',
    '**Admin:** kế thừa toàn bộ use case của Nhân viên và có thêm các chức năng quản trị.',
    '**Hangfire (tác nhân hệ thống):** bộ lập lịch kích hoạt các tác vụ định kỳ.',
  ]));

  add(h3('2.2.2. Biểu đồ use case của Nhân viên'));
  const f21 = F.make(DIAGRAM('H2.1-usecase-nhanvien.png'), 'Biểu đồ use case của tác nhân Nhân viên', { maxH: 17 });
  add(p(`${f21.label} mô tả các use case mà nhân viên thư viện được phép thực hiện sau khi đăng nhập.`, { keepNext: true }));
  add(...f21.nodes);
  add(p('Nhân viên liên kết trực tiếp với 11 use case: Đăng nhập, Đăng xuất, Xem Dashboard, Tìm kiếm sách, Quản lý kho sách, Quản lý độc giả, Quản lý danh mục, Lập phiếu mượn, Gia hạn phiếu mượn, Lập phiếu trả sách và Xem thống kê. Quan hệ «include» thể hiện bước bắt buộc: thêm sách luôn sinh mã QR, lập phiếu mượn luôn kiểm tra điều kiện mượn, gia hạn luôn kiểm tra điều kiện gia hạn và trả sách luôn tính tiền phạt. Quan hệ «extend» thể hiện bước tùy chọn: xem lịch sử mượn của độc giả, thu tiền phạt khi phiếu trả có phạt và xuất báo cáo Excel từ màn hình kho sách hoặc thống kê.'));

  add(h3('2.2.3. Biểu đồ use case của Admin'));
  const f22 = F.make(DIAGRAM('H2.2-usecase-admin.png'), 'Biểu đồ use case của tác nhân Admin', { maxH: 13 });
  add(p(`${f22.label} chỉ thể hiện các use case riêng của Admin; phần nghiệp vụ chung được kế thừa thông qua quan hệ tổng quát hóa Admin → Nhân viên.`, { keepNext: true }));
  add(...f22.nodes);
  add(p('Admin có ba use case riêng: Quản lý tài khoản (mở rộng thành thêm, cập nhật, đặt lại mật khẩu, khóa/mở khóa tài khoản), Cấu hình hệ thống và Giám sát tác vụ nền qua Hangfire Dashboard. Tác nhân hệ thống Hangfire kích hoạt hai use case tự động là Gửi email nhắc hạn trả (bao gồm gửi email qua SMTP bằng MailKit) và Cập nhật trạng thái quá hạn.'));

  // ===================== 2.3 =====================
  add(h2('2.3. Kịch bản use case'));
  add(p('Mục này trình bày kịch bản của các use case quan trọng. Thông báo lỗi và quy tắc nghiệp vụ được ghi theo mã nguồn và stored procedure tương ứng.'));

  const sc = (title, rows) => {
    const t = F.tableCaption(title);
    add(t.node, scenario(rows), spacer());
  };

  sc('Kịch bản use case Đăng nhập', [
    ['Tên use case', 'Đăng nhập'],
    ['Tác nhân', 'Nhân viên, Admin'],
    ['Tiền điều kiện', 'Người dùng đã có tài khoản và chưa đăng nhập.'],
    ['Luồng chính', ['1. Người dùng truy cập trang /Login, hệ thống hiển thị form đăng nhập.', '2. Người dùng nhập tên đăng nhập, mật khẩu và nhấn Đăng nhập.', '3. Hệ thống tìm tài khoản đang hoạt động (TrangThai = 1) theo tên đăng nhập và kiểm tra mật khẩu bằng BCrypt.', '4. Hệ thống cập nhật thời điểm đăng nhập cuối, tạo cookie xác thực chứa mã tài khoản, họ tên, vai trò.', '5. Hệ thống chuyển đến Dashboard (hoặc trang nội bộ được yêu cầu trước đó).']],
    ['Luồng phụ', ['1a. Người dùng đã đăng nhập truy cập /Login: hệ thống chuyển thẳng đến Dashboard.', '3a. Không tìm thấy tài khoản, tài khoản bị khóa hoặc sai mật khẩu: hiển thị “Tên đăng nhập hoặc mật khẩu không đúng.”, quay lại bước 2.']],
    ['Hậu điều kiện', 'Phiên đăng nhập có hiệu lực 8 giờ (gia hạn trượt); menu hiển thị theo vai trò.'],
  ]);

  sc('Kịch bản use case Lập phiếu mượn', [
    ['Tên use case', 'Lập phiếu mượn'],
    ['Tác nhân', 'Nhân viên (Admin kế thừa)'],
    ['Tiền điều kiện', 'Đã đăng nhập với vai trò NhanVien hoặc Admin.'],
    ['Luồng chính', ['1. Nhân viên chọn Mượn sách → Lập phiếu mượn; hệ thống hiển thị form.', '2. Nhân viên nhập từ khóa; hệ thống gợi ý tối đa 10 độc giả đang hoạt động.', '3. Nhân viên chọn độc giả; hệ thống hiển thị lớp, số sách đang mượn và cảnh báo nếu độc giả có phiếu quá hạn.', '4. Nhân viên tìm sách (chỉ sách còn tồn kho), thêm vào danh sách và nhập số lượng.', '5. Nhân viên nhập ghi chú, nhấn Xác nhận lập phiếu.', '6. Hệ thống gọi sp_LapPhieuMuon: kiểm tra độc giả, giới hạn số sách mượn, tồn kho; tạo PhieuMuon với hạn trả = ngày mượn + SoNgayMuonMacDinh, tạo CTPhieuMuon và trừ tồn kho trong một transaction.', '7. Hệ thống chuyển đến trang chi tiết phiếu và thông báo lập phiếu thành công.']],
    ['Luồng phụ', ['5a. Chưa chọn độc giả hoặc chưa có sách: thông báo “Vui lòng chọn độc giả.” hoặc “Vui lòng chọn ít nhất một cuốn sách.”.', '6a. Độc giả bị khóa (lỗi 50001), vượt giới hạn mượn (50002), sách không đủ hoặc ngừng lưu hành (50003), danh sách sách không hợp lệ (50005): giao dịch bị hủy (ROLLBACK), hệ thống hiển thị lỗi và giữ nguyên form.']],
    ['Hậu điều kiện', 'Phiếu mượn ở trạng thái Đang mượn; số lượng tồn của các sách giảm tương ứng.'],
  ]);

  sc('Kịch bản use case Lập phiếu trả sách', [
    ['Tên use case', 'Lập phiếu trả sách'],
    ['Tác nhân', 'Nhân viên (Admin kế thừa)'],
    ['Tiền điều kiện', 'Đã đăng nhập; phiếu mượn đang ở trạng thái Đang mượn, Quá hạn hoặc Đã gia hạn.'],
    ['Luồng chính', ['1. Nhân viên chọn Trả sách → Lập phiếu trả, nhập mã phiếu mượn và nhấn Tìm phiếu.', '2. Hệ thống hiển thị thông tin phiếu, danh sách sách và tiền phạt ước tính.', '3. Nhân viên chọn tình trạng sách (Bình thường / Hư hỏng / Mất), nhập ghi chú, nhấn Xác nhận trả sách.', '4. Hệ thống gọi sp_TraSach: tính số ngày trễ, tiền phạt = số ngày trễ × MucPhatNgayTreHan, cộng 50.000đ nếu hư hỏng hoặc 200.000đ nếu mất; tạo PhieuTra; chuyển phiếu mượn và chi tiết sang Đã trả; hoàn tồn kho (trừ sách mất).', '5. Hệ thống chuyển đến trang chi tiết phiếu trả.']],
    ['Luồng phụ', ['1a. Không tìm thấy phiếu: thông báo “Không tìm thấy phiếu mượn #…”.', '4a. Phiếu không tồn tại hoặc đã trả: lỗi 50020, giao dịch bị hủy.', '5a. Tiền phạt lớn hơn 0: nhân viên thu tiền và nhấn Thu phạt; hệ thống đặt DaThuPhat = 1 và thông báo “Đã ghi nhận thu phạt thành công.”.']],
    ['Hậu điều kiện', 'Phiếu trả được lưu; phiếu có phạt chưa thu nằm ở thẻ “Chưa thu phạt”, phiếu đã xong nằm ở thẻ “Đã hoàn tất”.'],
  ]);

  sc('Kịch bản use case Gia hạn phiếu mượn', [
    ['Tên use case', 'Gia hạn phiếu mượn'],
    ['Tác nhân', 'Nhân viên (Admin kế thừa)'],
    ['Tiền điều kiện', 'Đã đăng nhập; phiếu mượn ở trạng thái Đang mượn hoặc Đã gia hạn.'],
    ['Luồng chính', ['1. Nhân viên mở chi tiết phiếu mượn và chọn Gia hạn.', '2. Hệ thống hiển thị phiếu, hạn trả hiện tại, số lần đã gia hạn và lịch sử gia hạn.', '3. Nhân viên nhập ghi chú (tùy chọn) và xác nhận.', '4. Hệ thống gọi sp_GiaHanPhieuMuon: kiểm tra số lần gia hạn nhỏ hơn SoLanGiaHanToiDa và hạn trả chưa qua.', '5. Hệ thống thêm bản ghi GiaHan (lần gia hạn, hạn cũ, hạn mới = hạn cũ + SoNgayGiaHanMoiLan), cập nhật NgayHanTra và TrangThai = Đã gia hạn.', '6. Hệ thống quay lại chi tiết phiếu với thông báo “Gia hạn thành công. Hạn trả mới: …”.']],
    ['Luồng phụ', ['4a. Đã đạt số lần gia hạn tối đa: lỗi 50010.', '4b. Phiếu không tồn tại hoặc không thể gia hạn: lỗi 50011.', '4c. Phiếu đã quá hạn: lỗi 50012. Ở các trường hợp lỗi, giao dịch bị hủy và lỗi được hiển thị trên trang gia hạn.']],
    ['Hậu điều kiện', 'Hạn trả của phiếu được kéo dài; lịch sử gia hạn được lưu.'],
  ]);

  sc('Kịch bản use case Thêm sách mới (Quản lý kho sách)', [
    ['Tên use case', 'Thêm sách mới'],
    ['Tác nhân', 'Nhân viên (Admin kế thừa)'],
    ['Tiền điều kiện', 'Đã đăng nhập; đã có danh mục thể loại, tác giả, nhà xuất bản.'],
    ['Luồng chính', ['1. Nhân viên chọn Kho sách → Thêm sách; hệ thống hiển thị form với danh sách chọn thể loại, tác giả, nhà xuất bản.', '2. Nhân viên nhập tên sách, chọn thể loại, tác giả, nhà xuất bản, nhập năm xuất bản, số trang, số lượng nhập, vị trí kệ, mô tả và nhấn Lưu sách.', '3. Hệ thống kiểm tra dữ liệu, sinh mã QR dạng “SACH-…”, đặt số lượng tồn bằng số lượng nhập, ngày nhập là ngày hiện tại, trạng thái hoạt động.', '4. Hệ thống quay về danh sách sách và thông báo “Thêm sách thành công.”.']],
    ['Luồng phụ', ['3a. Dữ liệu không hợp lệ: hiển thị lỗi tại form, giữ dữ liệu đã nhập.', '3b. Lỗi khi lưu: thông báo “Lỗi: …”.']],
    ['Hậu điều kiện', 'Sách mới xuất hiện trong kho; ảnh mã QR xem được tại trang chi tiết sách.'],
  ]);

  sc('Kịch bản use case Cấu hình hệ thống', [
    ['Tên use case', 'Cấu hình hệ thống'],
    ['Tác nhân', 'Admin'],
    ['Tiền điều kiện', 'Đã đăng nhập với vai trò Admin.'],
    ['Luồng chính', ['1. Admin chọn Cấu hình; hệ thống hiển thị 5 tham số: mức phạt mỗi ngày trễ, số lần gia hạn tối đa, số ngày gia hạn thêm, số ngày mượn mặc định, số sách mượn tối đa.', '2. Admin sửa giá trị và nhấn Lưu tất cả.', '3. Hệ thống kiểm tra mọi giá trị là số nguyên không âm rồi cập nhật bảng CauHinhHeThong.', '4. Hệ thống thông báo “Đã lưu cấu hình thành công.”.']],
    ['Luồng phụ', ['1a. Người dùng không phải Admin truy cập: chuyển đến trang lỗi 403.', '3a. Có giá trị không hợp lệ: thông báo “Giá trị phải là số nguyên không âm: …” và không lưu.']],
    ['Hậu điều kiện', 'Các nghiệp vụ mượn, trả, gia hạn sau đó dùng giá trị cấu hình mới.'],
  ]);

  // ===================== 2.4 =====================
  add(h2('2.4. Biểu đồ tuần tự mức phân tích'));
  add(p('Ở mức phân tích, hệ thống được xem như một đối tượng duy nhất (:Hệ thống); biểu đồ chỉ thể hiện trao đổi giữa tác nhân và hệ thống, chưa đi vào các lớp bên trong.'));

  const seqs = [
    ['H2.3-seq-pt-dangnhap.png', 'Biểu đồ tuần tự mức phân tích – Đăng nhập', 9.5,
      'trình tự đăng nhập của người dùng.',
      'Sau khi người dùng gửi tên đăng nhập và mật khẩu, hệ thống tự kiểm tra tài khoản. Khung alt tách hai nhánh: nếu thông tin đúng và tài khoản đang hoạt động, hệ thống ghi nhận thời điểm đăng nhập, tạo phiên (cookie) theo vai trò và chuyển đến Dashboard; ngược lại hệ thống thông báo đăng nhập không thành công.'],
    ['H2.4-seq-pt-muonsach.png', 'Biểu đồ tuần tự mức phân tích – Lập phiếu mượn', 12,
      'các bước nhân viên lập phiếu mượn.',
      'Nhân viên lần lượt tìm, chọn độc giả và sách; hệ thống phản hồi thông tin độc giả (kèm cảnh báo quá hạn) và danh sách sách còn tồn. Khi xác nhận, hệ thống kiểm tra điều kiện mượn; nếu hợp lệ thì tạo phiếu, chi tiết phiếu và trừ tồn kho, nếu vi phạm thì trả về thông báo lỗi tương ứng.'],
    ['H2.5-seq-pt-trasach.png', 'Biểu đồ tuần tự mức phân tích – Lập phiếu trả sách', 12,
      'quy trình trả sách và thu phạt.',
      'Nhân viên tìm phiếu theo mã, chọn tình trạng sách và xác nhận. Hệ thống tự tính số ngày trễ, tiền phạt rồi lập phiếu trả, hoàn tồn kho. Khung opt thể hiện bước thu phạt chỉ xảy ra khi tiền phạt lớn hơn 0 và độc giả nộp phạt.'],
    ['H2.6-seq-pt-giahan.png', 'Biểu đồ tuần tự mức phân tích – Gia hạn phiếu mượn', 10.5,
      'quy trình gia hạn một phiếu mượn.',
      'Hệ thống kiểm tra số lần gia hạn, trạng thái và hạn trả của phiếu. Nếu hợp lệ, hệ thống lưu lịch sử gia hạn và cộng thêm số ngày theo cấu hình; nếu đã đạt số lần tối đa hoặc phiếu quá hạn thì báo lỗi.'],
  ];
  for (const [file, cap, h, intro, expl] of seqs) {
    const f = F.make(DIAGRAM(file), cap, { maxH: h + 4 });
    add(p(`${f.label} mô tả ${intro}`, { keepNext: true }), ...f.nodes, p(expl));
  }

  // ===================== 2.5 =====================
  add(h2('2.5. Biểu đồ hoạt động'));
  add(p('Biểu đồ hoạt động chia hai làn (swimlane): làn Nhân viên chứa các thao tác của người dùng, làn Hệ thống chứa các bước xử lý và điểm rẽ nhánh.'));
  const acts = [
    ['H2.7-act-dangnhap.png', 'Biểu đồ hoạt động – Đăng nhập', 12,
      'luồng hoạt động đăng nhập.',
      'Hệ thống có hai điểm quyết định: tìm thấy tài khoản đang hoạt động và mật khẩu đúng (BCrypt.Verify). Chỉ khi cả hai thỏa mãn, hệ thống mới cập nhật thời điểm đăng nhập cuối, tạo cookie xác thực và chuyển đến Dashboard; các trường hợp còn lại quay về thông báo lỗi.'],
    ['H2.8-act-muonsach.png', 'Biểu đồ hoạt động – Lập phiếu mượn', 13,
      'luồng lập phiếu mượn.',
      'Điểm quyết định thứ nhất kiểm tra dữ liệu nhập (đã chọn độc giả và sách); điểm thứ hai do sp_LapPhieuMuon thực hiện (độc giả, giới hạn, tồn kho). Khi không thỏa, giao dịch được ROLLBACK và nhân viên quay lại chỉnh sửa danh sách sách.'],
    ['H2.9-act-trasach.png', 'Biểu đồ hoạt động – Lập phiếu trả sách', 13,
      'luồng trả sách và thu phạt.',
      'Sau khi tìm thấy phiếu và xác nhận, hệ thống kiểm tra phiếu chưa được trả, tính tiền phạt (kể cả phụ phí hư hỏng, mất), lập phiếu trả và hoàn tồn kho. Nếu tiền phạt lớn hơn 0, nhân viên thu tiền và hệ thống ghi nhận DaThuPhat = 1.'],
    ['H2.10-act-giahan.png', 'Biểu đồ hoạt động – Gia hạn phiếu mượn', 13,
      'luồng gia hạn phiếu mượn.',
      'Nếu phiếu đã trả hoặc đã quá hạn, giao diện ẩn nút gia hạn. Khi nhân viên xác nhận, hệ thống kiểm tra còn lượt gia hạn và phiếu chưa trả, còn hạn; nếu hợp lệ thì thêm bản ghi GiaHan, cập nhật hạn trả và trạng thái Đã gia hạn.'],
  ];
  for (const [file, cap, h, intro, expl] of acts) {
    const f = F.make(DIAGRAM(file), cap, { maxH: h + 5 });
    add(p(`${f.label} mô tả ${intro}`, { keepNext: true }), ...f.nodes, p(expl));
  }

  return out;
};
