const { Diagram } = require('../dio');
const { Seq } = require('./seq');

// ---------- Mức phân tích: Actor ↔ Hệ thống ----------
const PA = (actor = 'Nhân viên') => [
  { key: 'nv', label: actor, kind: 'actor', cx: 70 },
  { key: 'ht', label: ':Hệ thống', kind: 'object', cx: 390, w: 170 },
];

// Chạy danh sách bước; trả về y cuối. Bước: ['m'|'r', from, to, label, lines] | ['s', who, label, lines, opts] | ['gap', dy] | ['mark', name]
function run(s, steps, y0, marks = {}) {
  let y = y0;
  for (const st of steps) {
    const [k] = st;
    if (k === 'm' || k === 'r') {
      const lines = st[4] || 1;
      y += (lines - 1) * 16;
      s[k === 'm' ? 'msg' : 'ret'](st[1], st[2], st[3], y);
      y += 40;
    } else if (k === 's') {
      const lines = st[3] || 1;
      const h = 24 + (lines - 1) * 16;
      s.self(st[1], st[2], y - 6, h, st[4] || {});
      y += h + 26;
    } else if (k === 'gap') {
      y += st[1];
    } else if (k === 'mark') {
      marks[st[1]] = y;
    }
  }
  return y;
}

function seqAnalysis(out, file, name, steps, frames) {
  const d = new Diagram(name);
  const s = new Seq(d, PA());
  const marks = {};
  const end = run(s, steps, s.start, marks);
  for (const f of frames(marks, end)) s.frame(...f);
  s.finish(end + 10);
  return d.save(out, file);
}

function analysisLogin(out) {
  return seqAnalysis(out, 'H2.3-seq-pt-dangnhap.drawio', 'SD phân tích - Đăng nhập', [
    ['m', 'nv', 'ht', '1: Truy cập trang đăng nhập'],
    ['r', 'ht', 'nv', '2: Hiển thị form đăng nhập'],
    ['m', 'nv', 'ht', '3: Nhập tên đăng nhập, mật khẩu, nhấn Đăng nhập'],
    ['s', 'ht', '4: Kiểm tra tài khoản và mật khẩu'],
    ['mark', 'a1'], ['gap', 24],
    ['s', 'ht', '5: Ghi nhận thời điểm đăng nhập cuối'],
    ['s', 'ht', '6: Tạo phiên đăng nhập (cookie) theo vai trò'],
    ['r', 'ht', 'nv', '7: Chuyển đến trang Dashboard'],
    ['mark', 'a2'], ['gap', 30],
    ['r', 'ht', 'nv', '8: Thông báo đăng nhập không thành công'],
    ['mark', 'a3'],
  ], (m) => [['alt', '[thông tin đúng và tài khoản đang hoạt động]', 30, m.a1 - 14, 780, m.a3 - 10,
    [{ y: m.a2 - 14, guard: '[sai thông tin hoặc tài khoản bị khóa]' }]]]);
}

function analysisMuon(out) {
  return seqAnalysis(out, 'H2.4-seq-pt-muonsach.drawio', 'SD phân tích - Lập phiếu mượn', [
    ['m', 'nv', 'ht', '1: Chọn chức năng Lập phiếu mượn'],
    ['r', 'ht', 'nv', '2: Hiển thị form lập phiếu'],
    ['m', 'nv', 'ht', '3: Nhập từ khóa tìm độc giả'],
    ['r', 'ht', 'nv', '4: Danh sách độc giả đang hoạt động'],
    ['m', 'nv', 'ht', '5: Chọn độc giả'],
    ['r', 'ht', 'nv', '6: Thông tin độc giả (số sách đang mượn,<br>cảnh báo nếu có phiếu quá hạn)', 2],
    ['m', 'nv', 'ht', '7: Tìm sách, thêm vào danh sách mượn'],
    ['r', 'ht', 'nv', '8: Danh sách sách còn tồn kho'],
    ['m', 'nv', 'ht', '9: Nhập ghi chú, nhấn Xác nhận lập phiếu'],
    ['s', 'ht', '10: Kiểm tra điều kiện mượn'],
    ['mark', 'a1'], ['gap', 24],
    ['s', 'ht', '11: Tạo phiếu mượn và chi tiết phiếu,<br>trừ số lượng tồn kho', 2],
    ['r', 'ht', 'nv', '12: Hiển thị chi tiết phiếu mượn vừa lập'],
    ['mark', 'a2'], ['gap', 30],
    ['r', 'ht', 'nv', '13: Thông báo lỗi tương ứng'],
    ['mark', 'a3'],
  ], (m) => [['alt', '[độc giả hợp lệ, không vượt giới hạn, đủ tồn kho]', 30, m.a1 - 14, 780, m.a3 - 10,
    [{ y: m.a2 - 14, guard: '[độc giả bị khóa / vượt giới hạn / không đủ tồn kho]' }]]]);
}

function analysisTra(out) {
  return seqAnalysis(out, 'H2.5-seq-pt-trasach.drawio', 'SD phân tích - Trả sách', [
    ['m', 'nv', 'ht', '1: Chọn chức năng Lập phiếu trả'],
    ['r', 'ht', 'nv', '2: Hiển thị ô nhập mã phiếu mượn'],
    ['m', 'nv', 'ht', '3: Nhập mã phiếu mượn, nhấn Tìm phiếu'],
    ['r', 'ht', 'nv', '4: Thông tin phiếu mượn, danh sách sách,<br>tiền phạt ước tính', 2],
    ['m', 'nv', 'ht', '5: Chọn tình trạng sách, nhập ghi chú,<br>nhấn Xác nhận trả sách', 2],
    ['s', 'ht', '6: Tính số ngày trễ hạn và tiền phạt'],
    ['s', 'ht', '7: Lập phiếu trả, cập nhật trạng thái phiếu,<br>hoàn tồn kho (trừ sách bị mất)', 2],
    ['r', 'ht', 'nv', '8: Hiển thị phiếu trả và số tiền phạt'],
    ['mark', 'o1'], ['gap', 24],
    ['m', 'nv', 'ht', '9: Nhấn Thu phạt'],
    ['s', 'ht', '10: Ghi nhận đã thu tiền phạt'],
    ['r', 'ht', 'nv', '11: Thông báo thu phạt thành công'],
    ['mark', 'o2'],
  ], (m) => [['opt', '[tiền phạt > 0 và độc giả nộp phạt]', 30, m.o1 - 14, 780, m.o2 - 10]]);
}

function analysisGiaHan(out) {
  return seqAnalysis(out, 'H2.6-seq-pt-giahan.drawio', 'SD phân tích - Gia hạn', [
    ['m', 'nv', 'ht', '1: Mở chi tiết phiếu mượn, chọn Gia hạn'],
    ['r', 'ht', 'nv', '2: Thông tin phiếu, hạn trả hiện tại,<br>lịch sử gia hạn', 2],
    ['m', 'nv', 'ht', '3: Nhập ghi chú (tùy chọn), xác nhận gia hạn'],
    ['s', 'ht', '4: Kiểm tra số lần gia hạn, trạng thái<br>và hạn trả của phiếu', 2],
    ['mark', 'a1'], ['gap', 24],
    ['s', 'ht', '5: Lưu lịch sử gia hạn, cộng thêm số ngày<br>gia hạn theo cấu hình vào hạn trả', 2],
    ['r', 'ht', 'nv', '6: Thông báo thành công và hạn trả mới'],
    ['mark', 'a2'], ['gap', 30],
    ['r', 'ht', 'nv', '7: Thông báo lỗi tương ứng'],
    ['mark', 'a3'],
  ], (m) => [['alt', '[chưa đạt số lần tối đa, phiếu chưa trả và chưa quá hạn]', 30, m.a1 - 14, 780, m.a3 - 10,
    [{ y: m.a2 - 14, guard: '[đã đạt số lần tối đa / phiếu đã quá hạn]' }]]]);
}

// ---------- Mức thiết kế: View → Controller → Repository → SQL Server ----------
const PD = (view, ctrl, repo) => [
  { key: 'nv', label: 'Nhân viên', kind: 'actor', cx: 50 },
  { key: 'v', label: view, kind: 'boundary', cx: 210 },
  { key: 'c', label: ctrl, kind: 'control', cx: 385 },
  { key: 'r', label: repo, kind: 'repo', cx: 570, w: 180 },
  { key: 'db', label: 'SQL Server', kind: 'db', cx: 770, w: 110 },
];

function seqDesign(out, file, name, parts, steps, frames) {
  const d = new Diagram(name);
  const s = new Seq(d, parts);
  const marks = {};
  const end = run(s, steps, s.start, marks);
  for (const f of frames(marks, end)) s.frame(...f);
  s.finish(end + 10);
  return d.save(out, file);
}

const L = { left: true };

function designLogin(out) {
  return seqDesign(out, 'H3.6-seq-tk-dangnhap.drawio', 'SD thiết kế - Đăng nhập',
    PD('Giao diện<br>Login/Index', ':LoginController', '«repository»<br>:TaiKhoanRepository'), [
      ['m', 'nv', 'v', '1: Nhập tài khoản,<br>mật khẩu', 2],
      ['m', 'v', 'c', '2: POST /Login'],
      ['m', 'c', 'r', '3: XacThucAsync(tenDN, mk)'],
      ['m', 'r', 'db', '4: SELECT TaiKhoan<br>JOIN VaiTro', 2],
      ['r', 'db', 'r', '5: bản ghi tài khoản'],
      ['s', 'r', '6: BCrypt.Verify()'],
      ['mark', 'a1'], ['gap', 24],
      ['m', 'r', 'db', '7: UPDATE<br>LanDangNhapCuoi', 2],
      ['r', 'r', 'c', '8: TaiKhoanDto'],
      ['s', 'c', '9: SignInAsync()<br>(cookie, claims vai trò)', 2],
      ['r', 'c', 'v', '10: Redirect /Home'],
      ['r', 'v', 'nv', '11: Dashboard'],
      ['mark', 'a2'], ['gap', 30],
      ['r', 'r', 'c', '12: null'],
      ['r', 'c', 'v', '13: View + lỗi'],
      ['r', 'v', 'nv', '14: Thông báo lỗi'],
      ['mark', 'a3'],
    ], (m) => [['alt', '[mật khẩu đúng và TrangThai = 1]', 20, m.a1 - 14, 840, m.a3 - 10,
      [{ y: m.a2 - 14, guard: '[không tìm thấy tài khoản hoặc sai mật khẩu]' }]]]);
}

function designMuon(out) {
  return seqDesign(out, 'H3.7-seq-tk-muonsach.drawio', 'SD thiết kế - Lập phiếu mượn',
    PD('Giao diện<br>MuonSach/LapPhieu', ':MuonSachController', '«repository»<br>:PhieuMuonRepository'), [
      ['m', 'nv', 'v', '1: Xác nhận<br>lập phiếu', 2],
      ['m', 'v', 'c', '2: POST LapPhieu<br>(maDocGia, sachJson)', 2],
      ['s', 'c', '3: Kiểm tra dữ liệu,<br>lấy mã nhân viên', 2],
      ['m', 'c', 'r', '4: LapPhieuMuonAsync(dto)'],
      ['s', 'r', '5: Gộp sách trùng,<br>tạo chuỗi JSON', 2],
      ['m', 'r', 'db', '6: EXEC<br>sp_LapPhieuMuon', 2],
      ['s', 'db', '7: Kiểm tra độc giả,<br>giới hạn, tồn kho', 2, L],
      ['mark', 'a1'], ['gap', 24],
      ['s', 'db', '8: INSERT PhieuMuon,<br>CTPhieuMuon; UPDATE<br>Sach; COMMIT', 3, L],
      ['r', 'db', 'r', '9: @MaPhieuMuon'],
      ['r', 'r', 'c', '10: maPhieu'],
      ['r', 'c', 'v', '11: Redirect Detail/{id}'],
      ['r', 'v', 'nv', '12: Chi tiết phiếu'],
      ['mark', 'a2'], ['gap', 30],
      ['s', 'db', '13: ROLLBACK, THROW', 1, L],
      ['r', 'db', 'r', '14: SqlException'],
      ['r', 'r', 'c', '15: Exception'],
      ['r', 'c', 'v', '16: View + TempData lỗi'],
      ['r', 'v', 'nv', '17: Thông báo lỗi'],
      ['mark', 'a3'],
    ], (m) => [['alt', '[thỏa mọi điều kiện mượn]', 20, m.a1 - 14, 840, m.a3 - 10,
      [{ y: m.a2 - 14, guard: '[vi phạm điều kiện]' }]]]);
}

function designTra(out) {
  return seqDesign(out, 'H3.8-seq-tk-trasach.drawio', 'SD thiết kế - Trả sách',
    PD('Giao diện<br>TraSach/LapPhieu', ':TraSachController', '«repository»<br>:PhieuTraRepository'), [
      ['m', 'nv', 'v', '1: Chọn tình trạng,<br>xác nhận trả', 2],
      ['m', 'v', 'c', '2: POST LapPhieu<br>(maPhieuMuon, ...)', 2],
      ['m', 'c', 'r', '3: TraSachAsync(dto)'],
      ['m', 'r', 'db', '4: EXEC sp_TraSach'],
      ['mark', 'a1'], ['gap', 24],
      ['s', 'db', '5: Tính SoNgayTreHan,<br>TienPhat', 2, L],
      ['s', 'db', '6: INSERT PhieuTra;<br>UPDATE PhieuMuon,<br>CTPhieuMuon, Sach', 3, L],
      ['r', 'db', 'r', '7: MaPhieuTra, TienPhat'],
      ['r', 'r', 'c', '8: TraSachResultDto'],
      ['r', 'c', 'v', '9: Redirect Detail/{id}'],
      ['r', 'v', 'nv', '10: Phiếu trả'],
      ['mark', 'a2'], ['gap', 30],
      ['r', 'db', 'r', '11: THROW 50020'],
      ['r', 'r', 'c', '12: Exception'],
      ['r', 'c', 'v', '13: View + lỗi'],
      ['r', 'v', 'nv', '14: Thông báo lỗi'],
      ['mark', 'a3'],
    ], (m) => [['alt', '[phiếu mượn tồn tại và chưa trả]', 20, m.a1 - 14, 840, m.a3 - 10,
      [{ y: m.a2 - 14, guard: '[phiếu không tồn tại hoặc đã trả]' }]]]);
}

function designGiaHan(out) {
  return seqDesign(out, 'H3.9-seq-tk-giahan.drawio', 'SD thiết kế - Gia hạn',
    PD('Giao diện<br>GiaHan/Index', ':GiaHanController', '«repository»<br>:GiaHanRepository'), [
      ['m', 'nv', 'v', '1: Xác nhận<br>gia hạn', 2],
      ['m', 'v', 'c', '2: POST GiaHan<br>(maPhieuMuon, ghiChu)', 2],
      ['m', 'c', 'r', '3: GiaHanAsync(...)'],
      ['m', 'r', 'db', '4: EXEC<br>sp_GiaHanPhieuMuon', 2],
      ['s', 'db', '5: Đếm số lần gia hạn,<br>kiểm tra hạn trả', 2, L],
      ['mark', 'a1'], ['gap', 24],
      ['s', 'db', '6: INSERT GiaHan;<br>UPDATE PhieuMuon', 2, L],
      ['r', 'db', 'r', '7: NgayHanTraMoi'],
      ['r', 'r', 'c', '8: hanTraMoi'],
      ['r', 'c', 'v', '9: Redirect<br>MuonSach/Detail', 2],
      ['r', 'v', 'nv', '10: Hạn trả mới'],
      ['mark', 'a2'], ['gap', 30],
      ['r', 'db', 'r', '11: THROW 50010/50012'],
      ['r', 'r', 'c', '12: Exception'],
      ['r', 'c', 'v', '13: View + lỗi'],
      ['r', 'v', 'nv', '14: Thông báo lỗi'],
      ['mark', 'a3'],
    ], (m) => [['alt', '[chưa vượt số lần, phiếu chưa quá hạn]', 20, m.a1 - 14, 840, m.a3 - 10,
      [{ y: m.a2 - 14, guard: '[vượt số lần gia hạn hoặc phiếu quá hạn]' }]]]);
}

module.exports = {
  analysisLogin, analysisMuon, analysisTra, analysisGiaHan,
  designLogin, designMuon, designTra, designGiaHan,
};
