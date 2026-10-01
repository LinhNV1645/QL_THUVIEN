const { Diagram, S, C } = require('../dio');

function usecaseNhanVien(out) {
  const d = new Diagram('UC Nhân viên');
  d.vertex('Hệ thống quản lý thư viện THCS Thanh Xuân', S.boundary(15), 150, 10, 620, 800);
  const actor = d.vertex('Nhân viên', S.actor(15), 40, 360, 44, 84);

  const W = 220, H = 48, XA = 185, XB = 520;
  const uc = (label, x, y, fill, stroke) => d.vertex(label, S.usecase(fill, stroke, 14), x, y, W, H);

  const A = [
    ['Đăng nhập', 50], ['Đăng xuất', 110], ['Xem Dashboard', 170], ['Tìm kiếm sách', 230],
    ['Quản lý kho sách', 290], ['Quản lý độc giả', 350], ['Quản lý danh mục', 410],
    ['Lập phiếu mượn', 470], ['Gia hạn phiếu mượn', 530], ['Lập phiếu trả sách', 625],
    ['Xem thống kê', 735],
  ];
  const a = {};
  for (const [label, y] of A) {
    a[label] = uc(label, XA, y);
    d.edge(actor, a[label], '', S.assocLeft());
  }

  const sub = (label, y) => uc(label, XB, y, C.blueL, C.blue);
  const inc = (from, to) => d.edge(from, to, '«include»', S.dep(13));
  const ext = (from, to) => d.edge(from, to, '«extend»', S.dep(13));

  inc(a['Quản lý kho sách'], sub('Sinh mã QR cho sách', 290));
  ext(sub('Xem lịch sử mượn của độc giả', 350), a['Quản lý độc giả']);
  inc(a['Lập phiếu mượn'], sub('Kiểm tra điều kiện mượn', 470));
  inc(a['Gia hạn phiếu mượn'], sub('Kiểm tra điều kiện gia hạn', 530));
  inc(a['Lập phiếu trả sách'], sub('Tính tiền phạt', 595));
  ext(sub('Thu tiền phạt', 655), a['Lập phiếu trả sách']);
  ext(sub('Xuất báo cáo Excel', 735), a['Xem thống kê']);

  return d.save(out, 'H2.1-usecase-nhanvien.drawio');
}

function usecaseAdmin(out) {
  const d = new Diagram('UC Admin');
  d.vertex('Hệ thống quản lý thư viện THCS Thanh Xuân', S.boundary(15), 170, 10, 600, 640);

  const nv = d.vertex('Nhân viên', S.actor(15), 60, 40, 44, 84);
  const admin = d.vertex('Admin', S.actor(15), 60, 260, 44, 84);
  const job = d.vertex('«system»<br>Hangfire', S.actor(14), 60, 500, 44, 84);

  d.edge(admin, nv, '', S.general() + 'edgeStyle=orthogonalEdgeStyle;exitX=0;exitY=0.5;entryX=0;entryY=0.5;',
    { points: [[25, 302], [25, 82]] });

  const W = 230, H = 50, XA = 205, XB = 520;
  const uc = (label, x, y, fill, stroke, h = H) => d.vertex(label, S.usecase(fill, stroke, 14), x, y, W, h);

  const nhom = uc('Các use case nghiệp vụ của Nhân viên (Hình 2.1)', XA, 55, C.grayL, C.gray, 60);
  d.edge(nv, nhom, '', S.assocLeft());

  const qltk = uc('Quản lý tài khoản', XA, 205);
  const ch = uc('Cấu hình hệ thống', XA, 300);
  const gs = uc('Giám sát tác vụ nền (Job Monitor)', XA, 385);
  for (const u of [qltk, ch, gs]) d.edge(admin, u, '', S.assocLeft());

  const sub = (label, y) => uc(label, XB, y, C.blueL, C.blue, 46);
  for (const [label, y] of [['Thêm tài khoản', 140], ['Cập nhật tài khoản', 200], ['Đặt lại mật khẩu', 260], ['Khóa / mở khóa tài khoản', 320]]) {
    d.edge(sub(label, y), qltk, '«extend»', S.dep(13));
  }

  const email = uc('Gửi email nhắc hạn trả', XA, 480);
  const quahan = uc('Cập nhật trạng thái quá hạn', XA, 565);
  d.edge(job, email, '', S.assocLeft());
  d.edge(job, quahan, '', S.assocLeft());
  d.edge(email, sub('Gửi email qua SMTP (MailKit)', 482), '«include»', S.dep(13));

  return d.save(out, 'H2.2-usecase-admin.drawio');
}

module.exports = { usecaseNhanVien, usecaseAdmin };
