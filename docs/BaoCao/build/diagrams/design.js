const { Diagram, S, C, FONT, base, esc } = require('../dio');

// ======================= Dữ liệu bảng / lớp (theo sql/01-db-schema.sql và Domain/Entities) =======================
// [tên cột, kiểu SQL, kiểu C#, khóa]
const T = {
  TheLoai: [['MaTheLoai', 'INT', 'int', 'PK'], ['TenTheLoai', 'NVARCHAR(100)', 'string', 'UQ'], ['MoTa', 'NVARCHAR(500)', 'string?'], ['NgayTao', 'DATETIME2', 'DateTime']],
  TacGia: [['MaTacGia', 'INT', 'int', 'PK'], ['TenTacGia', 'NVARCHAR(150)', 'string', 'UQ'], ['QuocTich', 'NVARCHAR(100)', 'string?'], ['GhiChu', 'NVARCHAR(500)', 'string?']],
  NhaXuatBan: [['MaNXB', 'INT', 'int', 'PK'], ['TenNXB', 'NVARCHAR(150)', 'string', 'UQ'], ['DiaChi', 'NVARCHAR(300)', 'string?'], ['DienThoai', 'VARCHAR(20)', 'string?']],
  Sach: [['MaSach', 'INT', 'int', 'PK'], ['MaTheLoai', 'INT', 'int', 'FK'], ['MaTacGia', 'INT', 'int', 'FK'], ['MaNXB', 'INT', 'int', 'FK'],
    ['TenSach', 'NVARCHAR(300)', 'string'], ['NamXuatBan', 'SMALLINT', 'short?'], ['SoTrang', 'SMALLINT', 'short?'], ['SoLuongNhap', 'INT', 'int'],
    ['SoLuongTon', 'INT', 'int'], ['ViTri', 'NVARCHAR(50)', 'string?'], ['MaQR', 'VARCHAR(100)', 'string?', 'UQ'], ['MoTa', 'NVARCHAR(1000)', 'string?'],
    ['NgayNhap', 'DATE', 'DateOnly'], ['TrangThai', 'TINYINT', 'byte']],
  DocGia: [['MaDocGia', 'INT', 'int', 'PK'], ['HoTen', 'NVARCHAR(150)', 'string'], ['Lop', 'NVARCHAR(20)', 'string?'], ['NgaySinh', 'DATE', 'DateOnly?'],
    ['GioiTinh', 'BIT', 'bool?'], ['DiaChi', 'NVARCHAR(300)', 'string?'], ['Email', 'VARCHAR(150)', 'string?', 'UQ'], ['SoDienThoai', 'VARCHAR(15)', 'string?'],
    ['NgayDangKy', 'DATE', 'DateOnly'], ['TrangThai', 'TINYINT', 'byte']],
  CauHinhHeThong: [['MaCauHinh', 'INT', 'int', 'PK'], ['TenCauHinh', 'NVARCHAR(100)', 'string', 'UQ'], ['GiaTri', 'NVARCHAR(200)', 'string'], ['GhiChu', 'NVARCHAR(300)', 'string?']],
  PhieuMuon: [['MaPhieuMuon', 'INT', 'int', 'PK'], ['MaDocGia', 'INT', 'int', 'FK'], ['NgayMuon', 'DATE', 'DateOnly'], ['NgayHanTra', 'DATE', 'DateOnly'],
    ['TrangThai', 'TINYINT', 'byte'], ['GhiChu', 'NVARCHAR(500)', 'string?'], ['NhanVienLap', 'INT', 'int?']],
  CTPhieuMuon: [['MaCT', 'INT', 'int', 'PK'], ['MaPhieuMuon', 'INT', 'int', 'FK'], ['MaSach', 'INT', 'int', 'FK'], ['SoLuongMuon', 'INT', 'int'], ['TrangThaiCT', 'TINYINT', 'byte']],
  GiaHan: [['MaGiaHan', 'INT', 'int', 'PK'], ['MaPhieuMuon', 'INT', 'int', 'FK'], ['NgayGiaHan', 'DATE', 'DateOnly'], ['HanTraCu', 'DATE', 'DateOnly'],
    ['HanTraMoi', 'DATE', 'DateOnly'], ['LanGiaHan', 'TINYINT', 'byte'], ['NhanVienDuyet', 'INT', 'int?'], ['GhiChu', 'NVARCHAR(300)', 'string?']],
  PhieuTra: [['MaPhieuTra', 'INT', 'int', 'PK'], ['MaPhieuMuon', 'INT', 'int', 'FK'], ['NgayTra', 'DATE', 'DateOnly'], ['SoNgayMuon', 'INT', 'int'],
    ['SoNgayTreHan', 'INT', 'int'], ['TienPhat', 'DECIMAL(12,0)', 'decimal'], ['DaThuPhat', 'BIT', 'bool'], ['TrangThaiSach', 'TINYINT', 'byte'],
    ['GhiChu', 'NVARCHAR(500)', 'string?'], ['NhanVienThu', 'INT', 'int?']],
  VaiTro: [['MaVaiTro', 'INT', 'int', 'PK'], ['TenVaiTro', 'NVARCHAR(50)', 'string', 'UQ'], ['MoTa', 'NVARCHAR(200)', 'string?']],
  TaiKhoan: [['MaTaiKhoan', 'INT', 'int', 'PK'], ['MaVaiTro', 'INT', 'int', 'FK'], ['MaDocGia', 'INT', 'int?', 'FK'], ['TenDangNhap', 'VARCHAR(50)', 'string', 'UQ'],
    ['MatKhau', 'VARCHAR(255)', 'string'], ['HoTen', 'NVARCHAR(150)', 'string'], ['Email', 'VARCHAR(150)', 'string?'], ['SoDienThoai', 'VARCHAR(15)', 'string?'],
    ['NgayTao', 'DATETIME2', 'DateTime'], ['LanDangNhapCuoi', 'DATETIME2', 'DateTime?'], ['TrangThai', 'TINYINT', 'byte']],
};

const GROUP = {
  TheLoai: 'g', TacGia: 'g', NhaXuatBan: 'g', Sach: 'g', DocGia: 'g',
  PhieuMuon: 'b', CTPhieuMuon: 'b', GiaHan: 'b', PhieuTra: 'b',
  VaiTro: 'o', TaiKhoan: 'o', CauHinhHeThong: 'o',
};
const GC = { g: [C.greenM, C.green], b: [C.blueM, C.blue], o: [C.orangeM, C.orange] };

const W = 232, RH = 20, HH = 28;
const X1 = 10, X2 = 290, X3 = 570;
const POS = {
  TheLoai: [X1, 10], TacGia: [X1, 130], NhaXuatBan: [X1, 250], CauHinhHeThong: [X1, 400], VaiTro: [X1, 840],
  Sach: [X2, 10], CTPhieuMuon: [X2, 554], TaiKhoan: [X2, 776],
  PhieuTra: [X3, 10], GiaHan: [X3, 292], PhieuMuon: [X3, 534], DocGia: [X3, 776],
};
const tableH = (n, extra = 0) => HH + n * RH + 4 + extra;

function drawTable(d, name, mode) {
  const [x, y] = POS[name];
  const cols = T[name];
  const [fill, stroke] = GC[GROUP[name]];
  const extra = mode === 'class' ? 10 : 0;
  const h = tableH(cols.length, extra);
  const id = d.vertex(name,
    `swimlane;html=1;fontFamily=${FONT};fontSize=14;fontStyle=1;fontColor=${C.text};startSize=${HH};horizontal=1;collapsible=0;` +
    `fillColor=${fill};strokeColor=${stroke};swimlaneFillColor=#FFFFFF;strokeWidth=1.4;rounded=0;`, x, y, W, h);
  cols.forEach(([col, sql, cs, key], i) => {
    let html;
    if (mode === 'erd') {
      const k = key === 'PK' ? '<b>PK</b>' : key === 'FK' ? '<i>FK</i>' : key === 'UQ' ? '<span style="color:#6B7280">UQ</span>' : '';
      const nm = key === 'PK' ? `<b><u>${col}</u></b>` : key === 'FK' ? `<i>${col}</i>` : col;
      html = `<table style="width:100%;border-collapse:collapse;font-size:12px"><tr><td style="width:26px">${k}</td><td>${nm}</td>` +
        `<td style="text-align:right;color:#4B5563">${sql}</td></tr></table>`;
    } else {
      html = `+ ${col}: <span style="color:#4B5563">${esc(cs)}</span>`;
    }
    d.vertex(html, `text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=6;spacingRight=6;overflow=hidden;` +
      `fontFamily=${FONT};fontSize=12;fontColor=${C.text};`, 0, HH + i * RH + 2, W, RH, { parent: id });
  });
  if (mode === 'class') {
    d.vertex('', `line;strokeWidth=1;fillColor=none;align=left;verticalAlign=middle;spacingTop=-1;spacingLeft=3;spacingRight=3;rotatable=0;labelPosition=right;points=[];portConstraint=eastwest;strokeColor=${stroke};`,
      0, HH + cols.length * RH + 4, W, 6, { parent: id });
  }
  return { id, x, y, h };
}

// Quan hệ giữa các bảng: [cha, con, kiểu đầu cha, kiểu đầu con, exit, entry, points, nhãn UML cha, nhãn UML con]
function relations(t) {
  const ey = (child, yAbs) => ((yAbs - t[child].y) / t[child].h).toFixed(3);
  const cy = (n) => t[n].y + t[n].h / 2;
  return [
    ['TheLoai', 'Sach', 'ERmandOne', 'ERzeroToMany', 'exitX=1;exitY=0.5;', `entryX=0;entryY=${ey('Sach', cy('TheLoai'))};`, [], '1', '0..*'],
    ['TacGia', 'Sach', 'ERmandOne', 'ERzeroToMany', 'exitX=1;exitY=0.5;', `entryX=0;entryY=${ey('Sach', cy('TacGia'))};`, [], '1', '0..*'],
    ['NhaXuatBan', 'Sach', 'ERmandOne', 'ERzeroToMany', 'exitX=1;exitY=0.5;', `entryX=0;entryY=${ey('Sach', cy('NhaXuatBan'))};`, [], '1', '0..*'],
    ['Sach', 'CTPhieuMuon', 'ERmandOne', 'ERzeroToMany', 'exitX=0.5;exitY=1;', 'entryX=0.5;entryY=0;', [], '1', '0..*'],
    ['PhieuMuon', 'CTPhieuMuon', 'ERmandOne', 'ERoneToMany', 'exitX=0;exitY=0.5;', `entryX=1;entryY=${ey('CTPhieuMuon', cy('PhieuMuon'))};`, [], '1', '1..*'],
    ['PhieuMuon', 'GiaHan', 'ERmandOne', 'ERzeroToMany', 'exitX=0.5;exitY=0;', 'entryX=0.5;entryY=1;', [], '1', '0..*'],
    ['PhieuMuon', 'PhieuTra', 'ERmandOne', 'ERzeroToOne', 'exitX=1;exitY=0.3;', 'entryX=1;entryY=0.5;',
      [[X3 + W + 26, t.PhieuMuon.y + t.PhieuMuon.h * 0.3], [X3 + W + 26, cy('PhieuTra')]], '1', '0..1'],
    ['DocGia', 'PhieuMuon', 'ERmandOne', 'ERzeroToMany', 'exitX=0.5;exitY=0;', 'entryX=0.5;entryY=1;', [], '1', '0..*'],
    ['DocGia', 'TaiKhoan', 'ERzeroToOne', 'ERzeroToMany', 'exitX=0;exitY=0.5;', `entryX=1;entryY=${ey('TaiKhoan', cy('DocGia'))};`, [], '0..1', '0..*'],
    ['VaiTro', 'TaiKhoan', 'ERmandOne', 'ERzeroToMany', 'exitX=1;exitY=0.5;', `entryX=0;entryY=${ey('TaiKhoan', cy('VaiTro'))};`, [], '1', '0..*'],
  ];
}

function legend(d, x, y, items) {
  d.vertex('<b>Chú thích nhóm bảng</b>', S.text(12, 'align=left;'), x, y, 220, 20);
  items.forEach(([label, fill, stroke], i) => {
    d.vertex('', S.rect(fill, stroke, 12), x + 4, y + 26 + i * 24, 26, 16);
    d.vertex(label, S.text(12, 'align=left;'), x + 38, y + 24 + i * 24, 190, 20);
  });
}

function erd(out) {
  const d = new Diagram('ERD');
  const t = {};
  for (const n of Object.keys(T)) t[n] = drawTable(d, n, 'erd');
  for (const [p, c, sa, ea, ex, en, pts] of relations(t)) {
    d.edge(t[p].id, t[c].id, '', `edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;startArrow=${sa};endArrow=${ea};startFill=0;endFill=0;startSize=12;endSize=12;strokeColor=${C.line};strokeWidth=1.3;${ex}${en}`, { points: pts });
  }
  legend(d, X1, 960, [['Danh mục, sách, độc giả', C.greenM, C.green], ['Mượn – trả – gia hạn', C.blueM, C.blue], ['Tài khoản, vai trò, cấu hình', C.orangeM, C.orange]]);
  return d.save(out, 'H3.5-erd.drawio');
}

function classEntity(out) {
  const d = new Diagram('Class Entities');
  const t = {};
  for (const n of Object.keys(T)) t[n] = drawTable(d, n, 'class');
  for (const [p, c, , , ex, en, pts] of relations(t)) {
    d.edge(t[p].id, t[c].id, '', `edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=none;strokeColor=${C.line};strokeWidth=1.3;${ex}${en}`, { points: pts });
  }
  // Bội số đặt bằng toạ độ tuyệt đối để không đè lên tiêu đề lớp.
  const m = (text, x, y) => d.vertex(text, S.text(12, 'align=left;'), x, y, 32, 16);
  const cy = (n) => t[n].y + t[n].h / 2;
  const bot = (n) => t[n].y + t[n].h;
  for (const n of ['TheLoai', 'TacGia', 'NhaXuatBan']) { m('1', X1 + W + 4, cy(n) - 18); m('0..*', X2 - 30, cy(n) + 2); }
  m('1', X2 + W / 2 + 6, bot('Sach') + 2); m('0..*', X2 + W / 2 + 6, t.CTPhieuMuon.y - 18);
  m('1', X3 - 14, cy('PhieuMuon') - 18); m('1..*', X2 + W + 4, cy('PhieuMuon') + 2);
  m('1', X3 + W / 2 - 16, t.PhieuMuon.y - 17); m('0..*', X3 + W / 2 + 6, bot('GiaHan') + 1);
  m('1', X3 + W + 4, t.PhieuMuon.y + t.PhieuMuon.h * 0.3 - 18); m('0..1', X3 + W + 4, cy('PhieuTra') - 18);
  m('1', X3 + W / 2 - 16, t.DocGia.y - 17); m('0..*', X3 + W / 2 + 6, bot('PhieuMuon') + 1);
  m('0..1', X3 - 30, cy('DocGia') - 18); m('0..*', X2 + W + 4, cy('DocGia') + 2);
  m('1', X1 + W + 4, cy('VaiTro') - 18); m('0..*', X2 - 30, cy('VaiTro') + 2);
  // Ngoại lệ miền (Domain/Exceptions)
  const exBase = d.vertex('System.Exception', S.rect(C.grayL, C.gray, 13, 'fontStyle=2;'), X1, 530, W, 28);
  const exs = [['TonKhoKhongDuException', '+ ctor(tenSach: string)'], ['GioiHanMuonException', '+ ctor(soSachToiDa: int)'], ['QuaHanGiaHanException', '+ ctor(soLanToiDa: int)']];
  exs.forEach(([n, m], i) => {
    const y = 574 + i * 66;
    const id = d.vertex(n, `swimlane;html=1;fontFamily=${FONT};fontSize=13;fontStyle=1;fontColor=${C.text};startSize=26;collapsible=0;fillColor=${C.grayM};strokeColor=${C.gray};swimlaneFillColor=#FFFFFF;strokeWidth=1.3;`, X1 + 24, y, W - 24, 56);
    d.vertex(m, `text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=6;fontFamily=${FONT};fontSize=12;fontColor=${C.text};`, 0, 30, W - 24, 22, { parent: id });
    d.edge(id, exBase, '', S.general() + 'edgeStyle=orthogonalEdgeStyle;exitX=0;exitY=0.5;entryX=0;entryY=0.5;', { points: [[X1 + 10, y + 28], [X1 + 10, 544]] });
  });
  d.vertex('<i>«exception» – định nghĩa trong Domain,<br>hiện chưa được tham chiếu</i>', S.text(11, 'align=left;'), X1, 772, W, 30);
  legend(d, X1, 960, [['Danh mục, sách, độc giả', C.greenM, C.green], ['Mượn – trả – gia hạn', C.blueM, C.blue], ['Tài khoản, vai trò, cấu hình', C.orangeM, C.orange]]);
  return d.save(out, 'H3.3-class-entity.drawio');
}

// ======================= Biểu đồ lớp các interface tầng Application =======================
const IFACES = [
  ['ISachRepository', ['GetAllAsync(filter): PaginatedResult', 'GetByIdAsync(maSach): SachDto?', 'GetByQrAsync(maQR): SachDto?', 'CreateAsync(dto): int', 'UpdateAsync(dto)', 'SoftDeleteAsync(maSach)', 'GetTonKhoAsync(maSach): int']],
  ['IDocGiaRepository', ['GetAllAsync(keyword, lop, page, size)', 'GetByIdAsync(maDocGia): DocGiaDto?', 'CreateAsync(dto): int', 'UpdateAsync(dto)', 'SetTrangThaiAsync(maDocGia, tt)', 'GetLichSuAsync(maDocGia)']],
  ['IPhieuMuonRepository', ['LapPhieuMuonAsync(dto): int', 'GetByIdAsync(maPhieu)', 'GetAllAsync(filter): PaginatedResult', 'GetQuaHanAsync()', 'GetDangMuonByDocGiaAsync(maDocGia)']],
  ['IGiaHanRepository', ['GiaHanAsync(maPM, nv, ghiChu): DateOnly', 'GetByPhieuMuonAsync(maPhieuMuon)']],
  ['IPhieuTraRepository', ['TraSachAsync(dto): TraSachResultDto', 'GetByIdAsync(maPhieuTra)', 'GetByPhieuMuonAsync(maPhieuMuon)', 'GetAllAsync(top)', 'GetChuaThuPhatAsync()', 'ThuPhatAsync(maPhieuTra)']],
  ['ITaiKhoanRepository', ['XacThucAsync(tenDN, mk): TaiKhoanDto?', 'GetByIdAsync(maTK)', 'GetByUsernameAsync(tenDN)', 'GetAllAsync()', 'GetVaiTrosAsync()', 'CreateAsync(dto): int', 'UpdateAsync(dto)', 'ChangePasswordAsync(maTK, mk)', 'SetTrangThaiAsync(maTK, tt)']],
  ['IDanhMucRepository', ['GetTheLoaisAsync()', 'GetTacGiasAsync()', 'GetNhaXuatBansAsync()', 'AddTheLoaiAsync(ten)', 'AddTacGiaAsync(ten)', 'AddNhaXuatBanAsync(ten)', 'DeleteTheLoaiAsync(id)', 'DeleteTacGiaAsync(id)', 'DeleteNhaXuatBanAsync(id)']],
  ['IThongKeRepository', ['GetDashboardAsync(): DashboardDto', 'GetTopQuaHanAsync(top)', 'GetSachMuonNhieuAsync(thang, nam, n)', 'GetDocGiaMuonNhieuAsync(thang, nam, n)', 'GetSapDenHanAsync(soNgay, n)', 'GetTopSachAsync(tu, den, n)', 'GetTopDocGiaAsync(tu, den, n)']],
  ['ICauHinhRepository', ['GetValueAsync(ten): string?', 'GetAllAsync()', 'UpdateAsync(ten, giaTri)']],
  ['IEmailService', ['SendNhacNhoHanTraAsync(email, ...)']],
  ['IQrService', ['GenerateQrImagePath(maQR): string', 'GenerateQrBytes(maQR): byte[]']],
  ['IExportService', ['ExportSachToExcelAsync(filter): byte[]', 'ExportLichSuMuonToExcelAsync(...)', 'ExportThongKeToExcelAsync(thang, nam)', 'ExportSachToPdfAsync(filter)']],
];

function classRepo(out) {
  const d = new Diagram('Class Interfaces');
  const IW = 266, GX = 16, RHi = 18, HHi = 40;
  const colX = [10, 10 + IW + GX, 10 + 2 * (IW + GX)];
  const colY = [10, 10, 10];
  const order = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  const layout = [[0, 3, 8, 9], [1, 4, 6], [2, 5, 7, 10, 11]];
  layout.forEach((list, ci) => {
    for (const idx of list) {
      const [name, ms] = IFACES[order[idx]];
      const svc = name.endsWith('Service');
      const [fill, stroke] = svc ? [C.orangeM, C.orange] : [C.purpleL, C.purple];
      const h = HHi + 8 + ms.length * RHi + 6;
      const id = d.vertex(`«interface»<br>${name}`,
        `swimlane;html=1;fontFamily=${FONT};fontSize=13;fontStyle=1;fontColor=${C.text};startSize=${HHi};collapsible=0;fillColor=${fill};strokeColor=${stroke};swimlaneFillColor=#FFFFFF;strokeWidth=1.3;`,
        colX[ci], colY[ci], IW, h);
      d.vertex('', `line;strokeWidth=1;fillColor=none;rotatable=0;points=[];strokeColor=${stroke};`, 0, HHi, IW, 6, { parent: id });
      ms.forEach((m, i) => d.vertex(`+ ${esc(m)}`, `text;html=1;strokeColor=none;fillColor=none;align=left;verticalAlign=middle;spacingLeft=6;overflow=hidden;fontFamily=${FONT};fontSize=12;fontColor=${C.text};`,
        0, HHi + 6 + i * RHi, IW, RHi, { parent: id }));
      colY[ci] += h + 18;
    }
  });
  const bottom = Math.max(...colY);
  d.vertex('Mỗi interface được hiện thực bởi một lớp cùng tên (bỏ tiền tố I) trong LibraryManagement.Infrastructure, ví dụ SachRepository : ISachRepository, ' +
    'và được đăng ký AddScoped trong Program.cs. Các phương thức đều bất đồng bộ (trả về Task / Task&lt;T&gt;).',
  S.note(12), 10, bottom, 3 * IW + 2 * GX, 52);
  return d.save(out, 'H3.4-class-interface.drawio');
}

// ======================= Sơ đồ điều hướng màn hình =======================
function navigation(out) {
  const d = new Diagram('Navigation');
  const box = (label, x, y, w, h, fill, stroke, size = 13, extra = '') => d.vertex(label, S.box(fill, stroke, size, extra), x, y, w, h);
  const fl = (a, b, ex = 'exitX=1;exitY=0.5;', en = 'entryX=0;entryY=0.5;') =>
    d.edge(a, b, '', `edgeStyle=orthogonalEdgeStyle;rounded=1;html=1;endArrow=block;endFill=1;endSize=7;strokeColor=${C.line};strokeWidth=1.2;${ex}${en}`);

  const SH = 50, GAP = 12, XS = 470, WS = 320;
  const groups = [
    ['Tra cứu', false, [['Tìm kiếm sách', 'Từ khóa, thể loại, chỉ sách còn tồn kho']]],
    ['Quản lý sách', false, [['Kho sách', 'Danh sách · Thêm · Sửa · Chi tiết (mã QR) · Ẩn · Xuất Excel'],
      ['Độc giả', 'Danh sách · Thêm · Sửa · Chi tiết (lịch sử mượn) · Khóa/Mở'], ['Danh mục', 'Thể loại · Tác giả · Nhà xuất bản (thêm, xóa)']]],
    ['Nghiệp vụ', false, [['Mượn sách', 'Danh sách phiếu · Lập phiếu · Chi tiết · Gia hạn · Phiếu quá hạn'],
      ['Trả sách', 'Phiếu trả (chưa thu phạt / đã hoàn tất) · Lập phiếu trả · Chi tiết · Thu phạt'], ['Thống kê', 'Top sách, top độc giả theo tháng · Xuất Excel']]],
    ['Hệ thống', true, [['Tài khoản', 'Danh sách · Thêm · Sửa · Đặt lại mật khẩu · Khóa'], ['Cấu hình', '5 tham số mượn – trả – phạt'], ['Job Monitor', 'Hangfire Dashboard (/hangfire)']]],
  ];
  let y = 10;
  const gIds = [];
  for (const [gname, admin, screens] of groups) {
    const y0 = y;
    const sIds = [];
    for (const [sname, sub] of screens) {
      const [fill, stroke] = admin ? [C.orangeL, C.orange] : [C.blueL, C.blue];
      sIds.push(box(`<b>${sname}</b><br><span style="font-size:11px;color:#374151">${sub}</span>`, XS, y, WS, SH, fill, stroke, 13, 'align=left;spacingLeft=10;arcSize=10;'));
      y += SH + GAP;
    }
    const gh = 40;
    const gy = (y0 + y - GAP) / 2 - gh / 2;
    const [fill, stroke] = admin ? [C.orangeM, C.orange] : [C.greenM, C.green];
    const g = box(admin ? `${gname}<br><span style="font-size:11px">(chỉ Admin)</span>` : gname, 290, gy, 140, gh, fill, stroke, 13, 'fontStyle=1;');
    sIds.forEach((s) => fl(g, s));
    gIds.push(g);
    y += 10;
  }
  const mid = (y - 10) / 2;
  const login = box('Đăng nhập', 10, mid - 22, 110, 44, C.grayL, C.gray, 13, 'fontStyle=1;');
  const dash = box('Dashboard<br><span style="font-size:11px">(trang chủ + menu)</span>', 150, mid - 26, 112, 52, C.greenM, C.green, 13, 'fontStyle=1;');
  fl(login, dash);
  gIds.forEach((g) => fl(dash, g));
  return d.save(out, 'H3.1-navigation.drawio');
}

// ======================= Bố cục giao diện chung (wireframe theo _Layout.cshtml) =======================
function layoutWire(out) {
  const d = new Diagram('Layout');
  const Wd = 780, Hd = 470;
  d.vertex('', S.rect('#FFFFFF', C.gray, 12), 10, 10, Wd, Hd);
  d.vertex('', S.rect(C.green, C.green, 12), 10, 10, Wd, 44);
  d.vertex('☰', S.rect('none', '#FFFFFF', 14, 'fontColor=#FFFFFF;'), 22, 20, 28, 24);
  d.vertex('<b>Thư viện THCS Thanh Xuân</b>', S.text(14, 'fontColor=#FFFFFF;align=left;'), 58, 20, 260, 24);
  d.vertex('Họ tên người dùng  <span style="background:#FFC107;color:#000;padding:1px 4px;border-radius:3px">Vai trò</span>  ⎋', S.text(12, 'fontColor=#FFFFFF;align=right;'), 480, 20, 300, 24);

  d.vertex('', S.rect('#212529', '#212529', 12), 10, 54, 190, Hd - 44);
  const menu = [['Dashboard', 0], ['TRA CỨU', 1], ['Tìm kiếm sách', 0], ['QUẢN LÝ SÁCH', 1], ['Kho sách', 0], ['Độc giả', 0], ['Danh mục', 0],
    ['NGHIỆP VỤ', 1], ['Mượn sách', 0], ['Trả sách', 0], ['Thống kê', 0], ['HỆ THỐNG (Admin)', 1], ['Tài khoản', 0], ['Cấu hình', 0], ['Job Monitor', 0]];
  menu.forEach(([m, sec], i) => d.vertex(m, S.text(sec ? 10 : 12, `fontColor=${sec ? '#ADB5BD' : '#FFFFFF'};align=left;${sec ? 'fontStyle=1;' : ''}`), 26, 62 + i * 26, 170, 22));

  const cx = 216, cw = Wd - 216;
  d.vertex('Vùng thông báo (TempData Success / Error)', S.rect('#D1E7DD', '#A3CFBB', 12, 'align=left;spacingLeft=10;'), cx, 66, cw, 30);
  d.vertex('<b>Tiêu đề trang</b>', S.text(15, 'align=left;'), cx, 106, 260, 26);
  d.vertex('+ Thêm mới', S.box(C.green, C.green, 12, 'fontColor=#FFFFFF;'), cx + cw - 230, 106, 110, 26);
  d.vertex('Xuất Excel', S.box('#FFFFFF', C.gray, 12), cx + cw - 110, 106, 100, 26);
  d.vertex('Thanh lọc / tìm kiếm: ô từ khóa · danh sách chọn · nút Tìm kiếm · Xóa lọc', S.rect(C.grayL, C.grayM, 12, 'align=left;spacingLeft=10;'), cx, 144, cw - 10, 36);
  d.vertex('', S.rect('#FFFFFF', C.grayM, 12), cx, 192, cw - 10, 270);
  d.vertex('STT · Cột dữ liệu · Trạng thái (badge) · Thao tác (xem / sửa / xóa)', S.rect(C.greenM, C.greenM, 12, 'align=left;spacingLeft=10;fontStyle=1;'), cx, 192, cw - 10, 30);
  for (let i = 0; i < 6; i++) d.vertex('', S.rect('none', C.grayM, 12), cx, 222 + i * 34, cw - 10, 34);
  d.vertex('Bảng dữ liệu (danh sách) hoặc thẻ (card) chứa form nhập liệu', S.text(12, `fontColor=${C.gray};fontStyle=2;`), cx, 300, cw - 10, 30);
  // chú thích vùng
  const tag = (t, x, y) => d.vertex(t, `ellipse;${base(12)}fillColor=${C.amber};strokeColor=${C.amber};fontColor=#FFFFFF;fontStyle=1;`, x, y, 24, 24);
  tag('1', 330, 20); tag('2', 160, 62); tag('3', cx + cw - 30, 70); tag('4', cx + 170, 106); tag('5', cx + cw - 38, 150); tag('6', cx + cw - 38, 196);
  return d.save(out, 'H3.2-layout.drawio');
}

// ======================= Biểu đồ thành phần =======================
function component(out) {
  const d = new Diagram('Component');
  const comp = (label, x, y, w, h, fill, stroke, size = 13, extra = '') =>
    d.vertex(label, `shape=component;align=left;spacingLeft=36;verticalAlign=top;spacingTop=6;${base(size)}fillColor=${fill};strokeColor=${stroke};strokeWidth=1.4;fontStyle=1;${extra}`, x, y, w, h);
  const sub = (label, x, y, w, h, fill, stroke) => d.vertex(label, S.rect(fill, stroke, 12, 'rounded=1;arcSize=10;'), x, y, w, h);
  const dep = (a, b, label, ex, en, pts = []) =>
    d.edge(a, b, label, S.dep(12) + `edgeStyle=orthogonalEdgeStyle;rounded=0;${ex}${en}`, { points: pts });

  // Trình duyệt
  const br = d.vertex('<b>Trình duyệt web</b><br><span style="font-size:11px">Razor HTML + Bootstrap 5 (CDN)</span>', S.rect(C.grayL, C.gray, 13, 'rounded=1;'), 260, 10, 280, 50);

  const web = comp('«component»<br>LibraryManagement.Web', 120, 100, 560, 150, C.greenL, C.green);
  sub('<b>Controllers</b><br>13 controller MVC', 140, 150, 160, 80, '#FFFFFF', C.green);
  sub('<b>Views (Razor)</b><br>_Layout, CRUD, nghiệp vụ', 320, 150, 160, 80, '#FFFFFF', C.green);
  sub('<b>Program.cs</b><br>DI, Cookie Auth, Hangfire Dashboard', 500, 150, 160, 80, '#FFFFFF', C.green);

  const app = comp('«component»<br>LibraryManagement.Application', 30, 320, 330, 130, C.purpleL, C.purple);
  sub('<b>Interfaces</b><br>9 Repository, 3 Service', 50, 370, 140, 64, '#FFFFFF', C.purple);
  sub('<b>DTOs</b><br>Data Transfer Objects, PaginatedResult', 205, 370, 140, 64, '#FFFFFF', C.purple);

  const inf = comp('«component»<br>LibraryManagement.Infrastructure', 420, 320, 380, 200, C.blueL, C.blue);
  sub('<b>Repositories</b><br>EF Core + ADO.NET (SP)', 440, 370, 165, 60, '#FFFFFF', C.blue);
  sub('<b>Services</b><br>Email, QR, Export', 620, 370, 165, 60, '#FFFFFF', C.blue);
  sub('<b>Persistence</b><br>AppDbContext', 440, 442, 165, 60, '#FFFFFF', C.blue);
  sub('<b>Jobs (Hangfire)</b><br>NhacNhoHanTra, CapNhatQuaHan', 620, 442, 165, 60, '#FFFFFF', C.blue);

  const dom = comp('«component»<br>LibraryManagement.Domain', 30, 520, 330, 110, C.amberL, C.amber);
  sub('<b>Entities</b><br>12 lớp thực thể', 50, 568, 140, 50, '#FFFFFF', C.amber);
  sub('<b>Exceptions</b><br>3 lớp ngoại lệ', 205, 568, 140, 50, '#FFFFFF', C.amber);

  const db = d.vertex('<b>SQL Server</b><br><span style="font-size:11px">CSDL QuanLyThuVien<br>(bảng, SP, view, Hangfire)</span>', `shape=cylinder3;boundedLbl=1;size=10;${base(13)}fillColor=${C.orangeL};strokeColor=${C.orange};strokeWidth=1.4;`, 440, 580, 160, 90);
  const smtp = d.vertex('<b>Máy chủ SMTP</b><br><span style="font-size:11px">smtp.gmail.com:587</span>', S.rect(C.grayL, C.gray, 13, 'rounded=1;'), 640, 595, 160, 60);
  const libs = d.vertex('<b>Thư viện NuGet</b>: EF Core 8 · BCrypt.Net-Next · MailKit · QRCoder · ClosedXML · Hangfire', S.rect('#FFFFFF', C.gray, 12, 'dashed=1;'), 30, 680, 770, 34);

  dep(br, web, 'HTTP(S)', 'exitX=0.5;exitY=1;', 'entryX=0.5;entryY=0;');
  dep(web, app, '«use» interface, DTO', 'exitX=0.25;exitY=1;', 'entryX=0.5;entryY=0;', [[260, 285], [195, 285]]);
  dep(web, inf, '«use» đăng ký DI', 'exitX=0.75;exitY=1;', 'entryX=0.5;entryY=0;', [[540, 285], [610, 285]]);
  d.line(440, 400, 360, 400, '', `endArrow=block;endFill=0;endSize=12;dashed=1;html=1;strokeColor=${C.line};strokeWidth=1.2;`);
  d.vertex('«realize»', S.text(12), 360, 378, 80, 18);
  dep(app, dom, '«use»', 'exitX=0.5;exitY=1;', 'entryX=0.5;entryY=0;');
  d.line(440, 472, 360, 575, '«use»', S.dep(12) + 'edgeStyle=orthogonalEdgeStyle;rounded=0;', { points: [[400, 472], [400, 575]] });
  dep(inf, db, 'EF Core / ADO.NET', 'exitX=0.25;exitY=1;', 'entryX=0.5;entryY=0;', [[515, 550], [520, 550]]);
  dep(inf, smtp, 'MailKit (SMTP)', 'exitX=0.85;exitY=1;', 'entryX=0.5;entryY=0;', [[743, 550], [720, 550]]);
  void libs;
  return d.save(out, 'H3.10-component.drawio');
}

// ======================= Biểu đồ triển khai =======================
function deployment(out) {
  const d = new Diagram('Deployment');
  const node = (label, x, y, w, h, fill, stroke) => d.vertex(label,
    `shape=cube;size=12;direction=south;html=1;whiteSpace=wrap;boundedLbl=1;verticalAlign=top;align=left;spacingLeft=10;spacingTop=4;fontFamily=${FONT};fontSize=13;fontColor=${C.text};fontStyle=1;fillColor=${fill};strokeColor=${stroke};strokeWidth=1.4;`, x, y, w, h);
  const env = (label, x, y, w, h, fill, stroke) => d.vertex(label, S.rect(fill, stroke, 12, 'verticalAlign=top;align=left;spacingLeft=8;spacingTop=2;fontStyle=1;'), x, y, w, h);
  const art = (label, x, y, w, h) => d.vertex(label, `shape=note;size=12;${base(12)}fillColor=#FFFFFF;strokeColor=${C.gray};strokeWidth=1.2;align=left;spacingLeft=8;`, x, y, w, h);
  const link = (a, b, label, ex, en, pts = []) => d.edge(a, b, label,
    `endArrow=none;html=1;strokeColor=${C.line};strokeWidth=1.6;fontFamily=${FONT};fontSize=12;fontColor=${C.text};labelBackgroundColor=#FFFFFF;edgeStyle=orthogonalEdgeStyle;rounded=0;${ex}${en}`, { points: pts });

  const pc = node('«device»<br>Máy trạm Nhân viên / Admin', 10, 10, 300, 150, C.grayL, C.gray);
  env('«execution environment»<br>Trình duyệt web', 30, 70, 260, 70, '#FFFFFF', C.gray);
  d.vertex('Giao diện HTML/CSS/JS (Bootstrap 5.3.3)', S.text(12, 'align=left;'), 40, 108, 240, 22);

  const cdn = node('«node»<br>CDN jsDelivr', 480, 10, 310, 90, C.grayL, C.gray);
  d.vertex('bootstrap.min.css/js, bootstrap-icons', S.text(12, 'align=left;'), 500, 62, 270, 22);

  const app = node('«device»<br>Máy chủ ứng dụng', 10, 230, 520, 290, C.greenL, C.green);
  env('«execution environment»<br>.NET 8 Runtime – ASP.NET Core (Kestrel)', 30, 290, 480, 210, '#FFFFFF', C.green);
  art('«artifact»<br><b>LibraryManagement.Web.dll</b><br><span style="font-size:11px">kèm Application.dll, Infrastructure.dll, Domain.dll, wwwroot, appsettings.json</span>', 46, 340, 448, 66);
  art('«component» <b>Hangfire Server</b><br><span style="font-size:11px">RecurringJob: cap-nhat-qua-han (00:05), nhac-nho-han-tra (07:00)</span>', 46, 420, 448, 60);

  const dbs = node('«device»<br>Máy chủ CSDL', 10, 600, 520, 150, C.orangeL, C.orange);
  env('«execution environment»<br>Microsoft SQL Server 2019+', 30, 658, 480, 76, '#FFFFFF', C.orange);
  d.vertex('«database» <b>QuanLyThuVien</b> (12 bảng, SP, view, trigger, schema HangFire)', S.text(12, 'align=left;'), 40, 698, 460, 26);

  const smtp = node('«node»<br>Máy chủ SMTP Gmail', 580, 600, 210, 100, C.grayL, C.gray);
  d.vertex('smtp.gmail.com', S.text(12, 'align=left;'), 596, 654, 180, 22);

  link(pc, app, 'HTTPS (cookie xác thực)', '', '', [[160, 195]]);
  link(pc, cdn, 'HTTPS', '', '', [[400, 55]]);
  link(app, dbs, 'TCP 1433 (TDS)', '', '', [[270, 560]]);
  link(app, smtp, 'SMTP 587 + TLS', '', '', [[685, 470]]);
  return d.save(out, 'H3.11-deployment.drawio');
}

module.exports = { navigation, layoutWire, classEntity, classRepo, erd, component, deployment, TABLES: T };
