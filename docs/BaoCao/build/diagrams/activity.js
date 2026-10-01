const { Diagram, S, C, FONT, base } = require('../dio');

// Hai làn: Nhân viên (trái) và Hệ thống (phải, có cột chính và cột phụ).
const LANE = { nv: { x: 10, w: 320, cx: 170 }, ht: { x: 330, w: 470, cx: 500 }, side: { cx: 718 } };
const TOP = 10, HEAD = 36;

const flowStyle = (extra = '') =>
  `edgeStyle=orthogonalEdgeStyle;rounded=1;arcSize=10;html=1;endArrow=block;endFill=1;endSize=7;` +
  `strokeColor=${C.line};strokeWidth=1.3;fontFamily=${FONT};fontSize=12;fontColor=${C.text};labelBackgroundColor=#FFFFFF;${extra}`;

const side = { r: 'exitX=1;exitY=0.5;', l: 'exitX=0;exitY=0.5;', b: 'exitX=0.5;exitY=1;', t: 'exitX=0.5;exitY=0;' };
const ent = { r: 'entryX=1;entryY=0.5;', l: 'entryX=0;entryY=0.5;', b: 'entryX=0.5;entryY=1;', t: 'entryX=0.5;entryY=0;' };

class Act {
  constructor(name) {
    this.d = new Diagram(name);
    this.insertAt = this.d.cells.length;
  }
  _c(col) { return col === 'nv' ? LANE.nv.cx : col === 'side' ? LANE.side.cx : LANE.ht.cx; }
  start(col, y) { const cx = this._c(col); return this.d.vertex('', `ellipse;html=1;fillColor=${C.text};strokeColor=${C.text};`, cx - 13, y, 26, 26); }
  end(col, y, cx = this._c(col)) { return this.d.vertex('', `ellipse;shape=endState;html=1;fillColor=${C.text};strokeColor=${C.text};`, cx - 14, y, 28, 28); }
  act(col, y, label, h = 46) {
    const cx = this._c(col);
    const w = col === 'side' ? 150 : 232;
    const [fill, stroke] = col === 'nv' ? [C.greenL, C.green] : [C.blueL, C.blue];
    return this.d.vertex(label, S.box(fill, stroke, 13, 'arcSize=30;spacingLeft=8;spacingRight=8;'), cx - w / 2, y, w, h);
  }
  dec(col, y, label) {
    const cx = this._c(col);
    return this.d.vertex(label, `rhombus;${base(12)}fillColor=${C.amberL};strokeColor=${C.amber};strokeWidth=1.3;`, cx - 78, y, 156, 62);
  }
  flow(a, b, { from = 'b', to = 't', points = [], label, pos = -0.4, dx = 0, dy = 0 } = {}) {
    return this.d.edge(a, b, '', flowStyle(side[from] + ent[to]), {
      points, labels: label ? [{ text: label, pos, dx, dy }] : [],
    });
  }
  finish(height, file, out) {
    const d = this.d;
    const tail = d.cells.splice(this.insertAt);
    const lane = (l, title, fill, stroke) => {
      d.vertex('', S.rect('#FFFFFF', C.gray, 13, 'strokeWidth=1.2;'), l.x, TOP, l.w, height);
      d.vertex(title, S.rect(fill, stroke, 14, 'fontStyle=1;strokeWidth=1.2;'), l.x, TOP, l.w, HEAD);
    };
    lane(LANE.nv, 'Nhân viên', C.greenM, C.green);
    lane(LANE.ht, 'Hệ thống', C.blueM, C.blue);
    d.cells.push(...tail);
    return d.save(out, file);
  }
}

function actLogin(out) {
  const a = new Act('AD Đăng nhập');
  const st = a.start('nv', 60);
  const a1 = a.act('nv', 110, 'Mở trang đăng nhập');
  const a2 = a.act('nv', 190, 'Nhập tên đăng nhập, mật khẩu, nhấn Đăng nhập');
  const err = a.act('nv', 288, 'Xem thông báo lỗi đăng nhập');
  const b1 = a.act('ht', 190, 'Tìm tài khoản đang hoạt động theo tên đăng nhập');
  const d1 = a.dec('ht', 280, 'Tìm thấy?');
  const b2 = a.act('ht', 380, 'Kiểm tra mật khẩu bằng BCrypt.Verify');
  const d2 = a.dec('ht', 460, 'Mật khẩu đúng?');
  const b3 = a.act('ht', 560, 'Cập nhật thời điểm đăng nhập cuối');
  const b4 = a.act('ht', 640, 'Tạo cookie xác thực kèm claims vai trò');
  const b5 = a.act('ht', 720, 'Chuyển hướng đến Dashboard');
  const a3 = a.act('nv', 720, 'Xem trang Dashboard');
  const en = a.end('nv', 810);

  a.flow(st, a1); a.flow(a1, a2);
  a.flow(a2, b1, { from: 'r', to: 'l' });
  a.flow(b1, d1);
  a.flow(d1, err, { from: 'l', to: 'r', label: '[không]', pos: 0, dy: -12 });
  a.flow(d1, b2, { label: '[có]', pos: 0, dx: 18 });
  a.flow(b2, d2);
  a.flow(d2, err, { from: 'l', to: 'b', points: [[LANE.nv.cx, 491]], label: '[sai]', pos: -0.6, dy: -12 });
  a.flow(d2, b3, { label: '[đúng]', pos: 0, dx: 24 });
  a.flow(err, a2, { from: 'l', to: 'l', points: [[30, 313], [30, 213]] });
  a.flow(b3, b4); a.flow(b4, b5);
  a.flow(b5, a3, { from: 'l', to: 'r' });
  a.flow(a3, en);
  return a.finish(840, 'H2.7-act-dangnhap.drawio', out);
}

function actMuon(out) {
  const a = new Act('AD Lập phiếu mượn');
  const st = a.start('nv', 60);
  const a1 = a.act('nv', 106, 'Chọn chức năng Lập phiếu mượn');
  const a2 = a.act('nv', 182, 'Tìm và chọn độc giả', 52);
  const b1 = a.act('ht', 182, 'Hiển thị thông tin độc giả, cảnh báo nếu có phiếu quá hạn', 52);
  const a3 = a.act('nv', 268, 'Tìm sách, thêm vào danh sách, nhập số lượng');
  const a4 = a.act('nv', 348, 'Nhập ghi chú, nhấn Xác nhận lập phiếu');
  const err = a.act('nv', 428, 'Xem thông báo lỗi');
  const d1 = a.dec('ht', 420, 'Dữ liệu hợp lệ?');
  const b2 = a.act('ht', 520, 'sp_LapPhieuMuon kiểm tra độc giả, giới hạn mượn, tồn kho', 52);
  const d2 = a.dec('ht', 610, 'Thỏa điều kiện?');
  const b3 = a.act('ht', 710, 'Tạo PhieuMuon, CTPhieuMuon; trừ tồn kho (COMMIT)', 52);
  const b4 = a.act('ht', 800, 'Hiển thị chi tiết phiếu mượn');
  const a5 = a.act('nv', 800, 'Xem phiếu mượn vừa lập');
  const en = a.end('nv', 890);

  a.flow(st, a1); a.flow(a1, a2);
  a.flow(a2, b1, { from: 'r', to: 'l' });
  a.flow(b1, a3, { from: 'b', to: 'r', points: [[LANE.ht.cx, 291]] });
  a.flow(a3, a4);
  a.flow(a4, d1, { from: 'r', to: 't', points: [[LANE.ht.cx, 371]] });
  a.flow(d1, err, { from: 'l', to: 'r', label: '[không]', pos: 0, dy: -12 });
  a.flow(d1, b2, { label: '[có]', pos: 0, dx: 18 });
  a.flow(b2, d2);
  a.flow(d2, err, { from: 'l', to: 'b', points: [[LANE.nv.cx, 641]], label: '[không] ROLLBACK', pos: -0.55, dy: -12 });
  a.flow(d2, b3, { label: '[có]', pos: 0, dx: 18 });
  a.flow(err, a3, { from: 'l', to: 'l', points: [[30, 451], [30, 291]] });
  a.flow(b3, b4);
  a.flow(b4, a5, { from: 'l', to: 'r' });
  a.flow(a5, en);
  return a.finish(920, 'H2.8-act-muonsach.drawio', out);
}

function actTra(out) {
  const a = new Act('AD Trả sách');
  const st = a.start('nv', 56);
  const a1 = a.act('nv', 100, 'Chọn chức năng Lập phiếu trả');
  const a2 = a.act('nv', 176, 'Nhập mã phiếu mượn, nhấn Tìm phiếu');
  const err1 = a.act('nv', 258, 'Xem thông báo không tìm thấy phiếu');
  const d1 = a.dec('ht', 250, 'Tìm thấy phiếu?');
  const b1 = a.act('ht', 346, 'Hiển thị phiếu mượn, danh sách sách, tiền phạt ước tính', 52);
  const a3 = a.act('nv', 430, 'Chọn tình trạng sách, nhập ghi chú, nhấn Xác nhận', 52);
  const d2 = a.dec('ht', 510, 'Phiếu chưa trả?');
  const err2 = a.act('nv', 518, 'Xem thông báo lỗi');
  const en1 = a.end('nv', 600);
  const b2 = a.act('ht', 606, 'Tính số ngày trễ và tiền phạt (+50.000đ hư hỏng, +200.000đ mất)', 52);
  const b3 = a.act('ht', 690, 'Lập phiếu trả, cập nhật trạng thái, hoàn tồn kho (trừ sách mất)', 52);
  const d3 = a.dec('ht', 776, 'Tiền phạt > 0?');
  const a4 = a.act('nv', 784, 'Thu tiền của độc giả, nhấn Thu phạt');
  const b4 = a.act('ht', 870, 'Ghi nhận đã thu phạt (DaThuPhat = 1)');
  const en2 = a.end('ht', 950);

  a.flow(st, a1); a.flow(a1, a2);
  a.flow(a2, d1, { from: 'r', to: 't', points: [[LANE.ht.cx, 199]] });
  a.flow(d1, err1, { from: 'l', to: 'r', label: '[không]', pos: 0, dy: -12 });
  a.flow(err1, a2, { from: 't', to: 'b' });
  a.flow(d1, b1, { label: '[có]', pos: 0, dx: 18 });
  a.flow(b1, a3, { from: 'l', to: 't', points: [[LANE.nv.cx, 372]] });
  a.flow(a3, d2, { from: 'r', to: 't', points: [[LANE.ht.cx, 456]] });
  a.flow(d2, err2, { from: 'l', to: 'r', label: '[không]', pos: 0, dy: -12 });
  a.flow(err2, en1);
  a.flow(d2, b2, { label: '[có]', pos: 0, dx: 18 });
  a.flow(b2, b3);
  a.flow(b3, d3);
  a.flow(d3, a4, { from: 'l', to: 'r', label: '[có]', pos: 0, dy: -12 });
  a.flow(a4, b4, { from: 'b', to: 'l', points: [[LANE.nv.cx, 893]] });
  a.flow(d3, en2, { from: 'r', to: 'r', points: [[740, 807], [740, 964]], label: '[không]', pos: -0.75, dy: -12 });
  a.flow(b4, en2);
  return a.finish(990, 'H2.9-act-trasach.drawio', out);
}

function actGiaHan(out) {
  const a = new Act('AD Gia hạn');
  const st = a.start('nv', 56);
  const a1 = a.act('nv', 100, 'Mở chi tiết phiếu mượn, chọn Gia hạn');
  const b1 = a.act('ht', 100, 'Hiển thị phiếu, hạn trả, số lần và lịch sử gia hạn');
  const d1 = a.dec('ht', 190, 'Đã trả hoặc quá hạn?');
  const bx = a.act('side', 196, 'Ẩn nút gia hạn, hiển thị thông báo', 50);
  const enx = a.end('side', 290);
  const a2 = a.act('nv', 330, 'Nhập ghi chú (tùy chọn), nhấn Xác nhận gia hạn', 52);
  const d2 = a.dec('ht', 410, 'Còn lượt gia hạn?');
  const err = a.act('nv', 418, 'Xem thông báo lỗi');
  const en1 = a.end('nv', 520, 50);
  const d3 = a.dec('ht', 510, 'Chưa trả và còn hạn?');
  const b2 = a.act('ht', 610, 'Thêm bản ghi GiaHan (lần, hạn cũ, hạn mới)');
  const b3 = a.act('ht', 690, 'Cập nhật NgayHanTra, TrangThai = Đã gia hạn', 52);
  const b4 = a.act('ht', 776, 'Thông báo thành công kèm hạn trả mới');
  const a3 = a.act('nv', 776, 'Xem hạn trả mới của phiếu');
  const en = a.end('nv', 862);

  a.flow(st, a1);
  a.flow(a1, b1, { from: 'r', to: 'l' });
  a.flow(b1, d1);
  a.flow(d1, bx, { from: 'r', to: 'l', label: '[có]', pos: -0.4 });
  a.flow(bx, enx);
  a.flow(d1, a2, { from: 'b', to: 't', points: [[LANE.ht.cx, 290], [LANE.nv.cx, 290]], label: '[không]', pos: -0.2 });
  a.flow(a2, d2, { from: 'r', to: 't', points: [[LANE.ht.cx, 356]] });
  a.flow(d2, err, { from: 'l', to: 'r', label: '[không]', pos: 0, dy: -12 });
  a.flow(d2, d3, { label: '[có]', pos: 0, dx: 18 });
  a.flow(d3, err, { from: 'l', to: 'b', points: [[LANE.nv.cx, 541]], label: '[không]', pos: -0.6, dy: -12 });
  a.flow(err, en1, { from: 'l', to: 't', points: [[50, 441]] });
  a.flow(d3, b2, { label: '[có]', pos: 0, dx: 18 });
  a.flow(b2, b3); a.flow(b3, b4);
  a.flow(b4, a3, { from: 'l', to: 'r' });
  a.flow(a3, en);
  return a.finish(900, 'H2.10-act-giahan.drawio', out);
}

module.exports = { actLogin, actMuon, actTra, actGiaHan };
