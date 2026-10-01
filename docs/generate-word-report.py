#!/usr/bin/env python3
"""
Generate BaoCao_QuanLyThuVien.docx - Báo cáo đồ án môn học
Hệ thống Quản lý Thư viện THCS Thanh Tuân
ASP.NET Core 8 - Clean Architecture
"""

from docx import Document
from docx.shared import Inches, Pt, RGBColor, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
import os

BASE = "/Users/sun/T_Private/PTIT/BTL/docs"
PNG = f"{BASE}/diagrams/png"
OUT = f"{BASE}/BaoCao_QuanLyThuVien.docx"

doc = Document()

# ─── Page layout: A4, margins 2.5cm all sides ─────────────────────────────────
from docx.shared import Cm
section = doc.sections[0]
section.page_height = Cm(29.7)
section.page_width  = Cm(21.0)
section.left_margin   = Cm(3.0)
section.right_margin  = Cm(2.0)
section.top_margin    = Cm(2.5)
section.bottom_margin = Cm(2.5)

# ─── Helpers ─────────────────────────────────────────────────────────────────
def h(level, text, bold=True):
    p = doc.add_heading(text, level=level)
    p.runs[0].bold = bold
    if level == 1:
        p.runs[0].font.size = Pt(16)
        p.runs[0].font.color.rgb = RGBColor(0x19, 0x87, 0x54)  # Bootstrap green
    elif level == 2:
        p.runs[0].font.size = Pt(14)
        p.runs[0].font.color.rgb = RGBColor(0x0d, 0x6e, 0xfd)  # Bootstrap blue
    elif level == 3:
        p.runs[0].font.size = Pt(12)
        p.runs[0].font.color.rgb = RGBColor(0x20, 0x20, 0x20)
    return p

def para(text, size=11, bold=False, italic=False, color=None, align=None):
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.font.size = Pt(size)
    run.bold = bold
    run.italic = italic
    if color:
        run.font.color.rgb = color
    if align:
        p.alignment = align
    return p

def bullet(items, level=0):
    for item in items:
        p = doc.add_paragraph(style='List Bullet')
        run = p.add_run(item)
        run.font.size = Pt(11)
        p.paragraph_format.left_indent = Cm(1.0 * (level + 1))

def add_table(headers, rows, col_widths=None):
    table = doc.add_table(rows=1 + len(rows), cols=len(headers))
    table.style = 'Table Grid'
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    # Header row
    hdr = table.rows[0]
    for i, h_text in enumerate(headers):
        cell = hdr.cells[i]
        cell.text = h_text
        cell.paragraphs[0].runs[0].bold = True
        cell.paragraphs[0].runs[0].font.size = Pt(10)
        cell.paragraphs[0].alignment = WD_ALIGN_PARAGRAPH.CENTER
        # Header background green
        tc = cell._tc
        tcPr = tc.get_or_add_tcPr()
        shd = OxmlElement('w:shd')
        shd.set(qn('w:val'), 'clear')
        shd.set(qn('w:color'), 'auto')
        shd.set(qn('w:fill'), '198754')
        tcPr.append(shd)
        cell.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
    # Data rows
    for r_idx, row_data in enumerate(rows):
        row = table.rows[r_idx + 1]
        for c_idx, val in enumerate(row_data):
            cell = row.cells[c_idx]
            cell.text = str(val)
            cell.paragraphs[0].runs[0].font.size = Pt(10)
            if r_idx % 2 == 1:
                tc = cell._tc
                tcPr = tc.get_or_add_tcPr()
                shd = OxmlElement('w:shd')
                shd.set(qn('w:val'), 'clear')
                shd.set(qn('w:color'), 'auto')
                shd.set(qn('w:fill'), 'F0FFF4')
                tcPr.append(shd)
    if col_widths:
        for row in table.rows:
            for i, w in enumerate(col_widths):
                row.cells[i].width = Cm(w)
    return table

def img(filename, caption, width=15):
    path = f"{PNG}/{filename}"
    if os.path.exists(path):
        p = doc.add_picture(path, width=Cm(width))
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        cap = doc.add_paragraph(caption)
        cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        cap.runs[0].italic = True
        cap.runs[0].font.size = Pt(10)
        cap.runs[0].font.color.rgb = RGBColor(0x66, 0x66, 0x66)
    else:
        doc.add_paragraph(f"[Hình: {caption} — file không tìm thấy: {path}]")

def page_break():
    doc.add_page_break()

def line():
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(4)
    p.paragraph_format.space_after = Pt(4)


# ════════════════════════════════════════════════════════════════════
# TRANG BÌA
# ════════════════════════════════════════════════════════════════════
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("HỌC VIỆN CÔNG NGHỆ BƯU CHÍNH VIỄN THÔNG")
run.bold = True
run.font.size = Pt(14)
run.font.color.rgb = RGBColor(0x00, 0x33, 0x99)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("KHOA CÔNG NGHỆ THÔNG TIN 1")
run.bold = True
run.font.size = Pt(13)
run.font.color.rgb = RGBColor(0x00, 0x33, 0x99)

doc.add_paragraph()
doc.add_paragraph()

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("BÁO CÁO ĐỒ ÁN MÔN HỌC")
run.bold = True
run.font.size = Pt(16)

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("LẬP TRÌNH ỨNG DỤNG WEB")
run.bold = True
run.font.size = Pt(14)
run.font.color.rgb = RGBColor(0x66, 0x66, 0x66)

doc.add_paragraph()
doc.add_paragraph()

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("HỆ THỐNG QUẢN LÝ THƯ VIỆN\nTHCS THANH TUÂN")
run.bold = True
run.font.size = Pt(22)
run.font.color.rgb = RGBColor(0x19, 0x87, 0x54)

doc.add_paragraph()

p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("ASP.NET Core 8 MVC  ·  Clean Architecture  ·  SQL Server")
run.italic = True
run.font.size = Pt(12)
run.font.color.rgb = RGBColor(0x44, 0x44, 0x44)

doc.add_paragraph()
doc.add_paragraph()

# Team table
team_table = doc.add_table(rows=4, cols=3)
team_table.style = 'Table Grid'
team_table.alignment = WD_TABLE_ALIGNMENT.CENTER

headers_team = ["Họ và tên", "MSSV", "Phân công"]
for i, hd in enumerate(headers_team):
    cell = team_table.rows[0].cells[i]
    run = cell.paragraphs[0].add_run(hd)
    run.bold = True
    run.font.size = Pt(11)
    cell.paragraphs[0].alignment = WD_ALIGN_PARAGRAPH.CENTER
    tc = cell._tc
    tcPr = tc.get_or_add_tcPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), '198754')
    tcPr.append(shd)
    run.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)

members = [
    ("Bùi Văn Cơ",       "B22DTCN001", "Sách, Độc giả, QR Code, Danh mục"),
    ("Tạ Văn Tuân",      "B22DTCN039", "Mượn sách, Gia hạn, Tìm kiếm, Thống kê, Export Excel"),
    ("Nguyễn Văn Linh\n(Nhóm trưởng)", "B22DTCN022", "Trả sách, Cấu hình, Tài khoản, Email Hangfire, Dashboard"),
]
for i, (name, mssv, task) in enumerate(members):
    row = team_table.rows[i + 1]
    row.cells[0].text = name
    row.cells[1].text = mssv
    row.cells[2].text = task
    for c in row.cells:
        c.paragraphs[0].runs[0].font.size = Pt(10.5)

doc.add_paragraph()
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("Hà Nội, tháng 10/2026")
run.italic = True
run.font.size = Pt(12)

page_break()

# ════════════════════════════════════════════════════════════════════
# I. GIỚI THIỆU
# ════════════════════════════════════════════════════════════════════
h(1, "I. GIỚI THIỆU")

h(2, "1.1. Bối cảnh và mục tiêu")
para(
    "Trường THCS Thanh Tuân hiện quản lý thư viện thủ công bằng sổ sách giấy, "
    "gây khó khăn trong việc tra cứu, thống kê và nhắc nhở hạn trả sách. "
    "Đồ án này xây dựng hệ thống quản lý thư viện web hiện đại, tự động hóa "
    "toàn bộ quy trình mượn — trả sách, cảnh báo quá hạn qua email, "
    "và cung cấp báo cáo thống kê trực quan cho Ban Giám hiệu."
)
doc.add_paragraph()

h(2, "1.2. Phạm vi hệ thống")
bullet([
    "Quản lý kho sách: thêm/sửa/xóa sách, thể loại, tác giả, NXB; sinh mã QR tự động",
    "Quản lý độc giả: đăng ký, phân loại theo lớp, lịch sử mượn-trả",
    "Mượn/Trả sách: lập phiếu mượn nhiều sách, gia hạn, tính tiền phạt trễ hạn",
    "Tìm kiếm sách: công khai (không cần đăng nhập), phân trang server-side",
    "Thống kê: Top sách mượn nhiều, Top độc giả tích cực, theo tháng/năm",
    "Xuất Excel: danh sách sách, lịch sử mượn-trả, báo cáo thống kê",
    "Email tự động: Hangfire job 7:00 AM mỗi ngày nhắc nhở 3 ngày trước hạn trả",
    "Dashboard: 7 KPI chỉ số thời gian thực",
])

h(2, "1.3. Công nghệ sử dụng")
add_table(
    ["Lớp", "Công nghệ", "Phiên bản", "Mục đích"],
    [
        ["Backend Framework",   "ASP.NET Core 8 MVC",       "8.0.x",       "Web framework chính"],
        ["Database",            "SQL Server",                "2019+",       "Lưu trữ dữ liệu"],
        ["ORM",                 "Entity Framework Core",     "8.0.13",      "Truy vấn LINQ / Fluent API"],
        ["Authentication",      "Cookie Authentication",     "Built-in",    "Session-based login"],
        ["Password Hashing",    "BCrypt.Net-Next",           "4.0.3",       "Mã hóa mật khẩu"],
        ["Background Job",      "Hangfire",                  "1.8.x",       "Email nhắc nhở định kỳ"],
        ["Email Client",        "MailKit",                   "4.7.x",       "Gửi SMTP qua Gmail"],
        ["QR Code Generator",   "QRCoder",                   "1.6.0",       "Sinh PNG mã QR sách"],
        ["Excel Export",        "ClosedXML",                 "0.104.x",     "Xuất file .xlsx"],
        ["UI Framework",        "Bootstrap 5 + BI Icons",    "5.3 / 1.11",  "Giao diện responsive"],
    ],
    col_widths=[4.5, 4.5, 2.5, 5]
)

page_break()

# ════════════════════════════════════════════════════════════════════
# II. PHÂN TÍCH YÊU CẦU
# ════════════════════════════════════════════════════════════════════
h(1, "II. PHÂN TÍCH YÊU CẦU")

h(2, "2.1. Sơ đồ Use Case tổng quan")
para("Hệ thống có 3 tác nhân chính: Admin, Nhân viên thư viện và Độc giả (học sinh). "
     "Mỗi vai trò được phân quyền rõ ràng qua Cookie Authentication của ASP.NET Core.")
doc.add_paragraph()
img("07-usecase-diagram.png", "Hình 1: Sơ đồ Use Case — Phân quyền 3 vai trò", width=15)

doc.add_paragraph()
h(2, "2.2. Danh sách chức năng")
add_table(
    ["#", "Chức năng", "Vai trò", "Độ ưu tiên", "Trạng thái"],
    [
        ["1",  "Đăng nhập / Đăng xuất",              "Tất cả",           "Cao",  "✅ Hoàn thành"],
        ["2",  "Dashboard 7 KPI",                      "Admin + NV",       "Cao",  "✅ Hoàn thành"],
        ["3",  "CRUD Sách + QR Code",                  "Admin + NV",       "Cao",  "✅ Hoàn thành"],
        ["4",  "CRUD Độc giả",                          "Admin + NV",       "Cao",  "✅ Hoàn thành"],
        ["5",  "CRUD Thể loại / Tác giả / NXB",        "Admin + NV",       "TB",   "✅ Hoàn thành"],
        ["6",  "Lập phiếu mượn (nhiều sách, 1 lần)",   "Admin + NV",       "Cao",  "✅ Hoàn thành"],
        ["7",  "Gia hạn phiếu mượn",                   "Admin + NV",       "TB",   "✅ Hoàn thành"],
        ["8",  "Trả sách + tính tiền phạt",             "Admin + NV",       "Cao",  "✅ Hoàn thành"],
        ["9",  "Thu tiền phạt",                         "Admin + NV",       "TB",   "✅ Hoàn thành"],
        ["10", "Tìm kiếm sách công khai",               "Tất cả (ẩn danh)", "Cao",  "✅ Hoàn thành"],
        ["11", "Thống kê sách / độc giả",               "Admin + NV",       "TB",   "✅ Hoàn thành"],
        ["12", "Xuất Excel báo cáo",                    "Admin + NV",       "Thấp", "✅ Hoàn thành"],
        ["13", "Quản lý tài khoản nhân viên",           "Admin",            "TB",   "✅ Hoàn thành"],
        ["14", "Cấu hình hệ thống (ngày mượn, phạt...)", "Admin",           "TB",   "✅ Hoàn thành"],
        ["15", "Email nhắc hạn trả (Hangfire job)",     "Hệ thống",         "TB",   "✅ Hoàn thành"],
    ],
    col_widths=[1, 5.5, 3.5, 2.5, 3.5]
)

page_break()

# ════════════════════════════════════════════════════════════════════
# III. KIẾN TRÚC HỆ THỐNG
# ════════════════════════════════════════════════════════════════════
h(1, "III. KIẾN TRÚC HỆ THỐNG")

h(2, "3.1. Sơ đồ ngữ cảnh hệ thống (C4 Level 1)")
para("Sơ đồ dưới đây mô tả hệ thống ở mức cao nhất — Context Level — "
     "thể hiện mối quan hệ giữa hệ thống với các tác nhân bên ngoài: "
     "người dùng, cơ sở dữ liệu SQL Server và dịch vụ Gmail SMTP.")
doc.add_paragraph()
img("01-system-context.png", "Hình 2: C4 Level 1 — System Context Diagram", width=15)

doc.add_paragraph()
h(2, "3.2. Kiến trúc Clean Architecture (4 tầng)")
para(
    "Dự án áp dụng Clean Architecture (Onion Architecture) với nguyên tắc "
    "\"Dependency Rule\": mọi dependency đều hướng vào trong (Domain). "
    "Tầng Domain không phụ thuộc bất kỳ tầng nào, tầng Infrastructure implement "
    "các interface mà Application định nghĩa."
)
doc.add_paragraph()
img("02-clean-architecture.png", "Hình 3: Clean Architecture — 4 tầng và dependency flow", width=16)

doc.add_paragraph()
h(3, "Chi tiết 4 tầng:")
add_table(
    ["Tầng", "Project", "Trách nhiệm", "Công nghệ chính"],
    [
        ["Domain",         "LibraryManagement.Domain",          "Entity classes, Domain exceptions\n12 entity, 3 exception types",              "Pure C# POCO"],
        ["Application",    "LibraryManagement.Application",     "Interfaces (10), DTOs (35+)\nPaginatedResult<T> generic",                    "C# Interfaces + Records"],
        ["Infrastructure", "LibraryManagement.Infrastructure",  "Repositories (7), Services (3)\nJobs (1), AppDbContext\nEF Core + ADO.NET",   "EF Core 8, MailKit,\nQRCoder, ClosedXML,\nHangfire"],
        ["Web",            "LibraryManagement.Web",             "Controllers (12), Views (41+)\nDI Container, Auth config",                  "ASP.NET Core 8 MVC\nBootstrap 5, Razor"],
    ],
    col_widths=[3, 4.5, 5.5, 3]
)

doc.add_paragraph()
h(2, "3.3. Luồng xử lý Authentication")
add_table(
    ["Bước", "Hành động", "Xử lý"],
    [
        ["1", "POST /Login",              "Controller nhận username + password"],
        ["2", "TaiKhoanRepository",       "Truy vấn DB lấy TaiKhoan theo TenDangNhap"],
        ["3", "BCrypt.Verify()",          "So sánh password với hash trong DB"],
        ["4", "SignInAsync()",            "Tạo Cookie với Claims: TenDangNhap, HoTen, MaVaiTro"],
        ["5", "ClaimsPrincipal",          "Middleware inject vào mọi request sau đó"],
        ["6", "[Authorize(Roles=\"Admin\")]", "Filter tự động từ chối nếu không đúng role"],
    ],
    col_widths=[1.5, 4.5, 10]
)

page_break()

# ════════════════════════════════════════════════════════════════════
# IV. THIẾT KẾ CƠ SỞ DỮ LIỆU
# ════════════════════════════════════════════════════════════════════
h(1, "IV. THIẾT KẾ CƠ SỞ DỮ LIỆU")

h(2, "4.1. Sơ đồ thực thể quan hệ (ER Diagram)")
para(
    "Cơ sở dữ liệu QuanLyThuVien gồm 12 bảng được thiết kế theo chuẩn 3NF "
    "(Third Normal Form), đảm bảo không dư thừa dữ liệu. "
    "Tất cả khóa chính là INT IDENTITY(1,1), khóa ngoại có ràng buộc FOREIGN KEY."
)
doc.add_paragraph()
img("03-er-diagram.png", "Hình 4: ER Diagram — 12 bảng với quan hệ", width=16)

doc.add_paragraph()
h(2, "4.2. Mô tả bảng chính")
add_table(
    ["Bảng", "Số cột", "Ghi chú"],
    [
        ["Sach",            "12",  "Sách trong thư viện. MaQR UNIQUE. SoLuongTon được cập nhật bởi SP"],
        ["DocGia",          "9",   "Học sinh mượn sách. TrangThai: 1=Hoạt động, 0=Khóa"],
        ["TaiKhoan",        "9",   "Tài khoản nhân viên/admin. MatKhau BCrypt hash. FK → VaiTro, DocGia"],
        ["PhieuMuon",       "7",   "Phiếu mượn sách. TrangThai: 0=Đang mượn, 1=Gia hạn, 2=Đã trả"],
        ["CTPhieuMuon",     "5",   "Chi tiết phiếu mượn (N sách/phiếu). FK → PhieuMuon, Sach"],
        ["GiaHan",          "7",   "Lịch sử gia hạn. LanGiaHan giới hạn bởi SP dựa vào CauHinh"],
        ["PhieuTra",        "9",   "Phiếu trả sách. Tính TienPhat và SoNgayTreHan. 1:1 với PhieuMuon"],
        ["CauHinhHeThong",  "4",   "Key-value cấu hình: SoNgayMuon, SoLanGiaHan, MucPhat, SoNgayNhacNho"],
    ],
    col_widths=[3.5, 2, 11]
)

doc.add_paragraph()
h(2, "4.3. Stored Procedures (8 SPs)")
para(
    "Toàn bộ logic nghiệp vụ phức tạp được đặt trong Stored Procedures "
    "để đảm bảo tính nguyên tử (ACID), bảo mật (không expose SQL vào code), "
    "và hiệu năng (execution plan cache)."
)
add_table(
    ["Stored Procedure", "Mục đích", "Điểm kỹ thuật nổi bật"],
    [
        ["sp_LapPhieuMuon",             "Tạo phiếu mượn + cập nhật tồn kho",  "JSON input, OUTPUT param, kiểm tra giới hạn, Transaction"],
        ["sp_TraSach",                  "Tạo phiếu trả + tính phạt",            "OUTPUT MaPhieuTra, TienPhat; tăng tồn kho nếu sách không mất"],
        ["sp_GiaHanPhieuMuon",          "Gia hạn phiếu mượn",                   "Kiểm tra SoLanGiaHan từ CauHinh, trả về HanTraMoi"],
        ["sp_TimKiemSach",              "Tìm kiếm toàn văn phân trang",         "CTE + ROW_NUMBER(), LIKE wildcard, COUNT tổng"],
        ["sp_ThongKeSachMuonNhieu",     "Top N sách được mượn nhiều nhất",      "GROUP BY, ORDER BY SoLanMuon DESC"],
        ["sp_ThongKeDocGiaMuonNhieu",   "Top N độc giả tích cực nhất",          "GROUP BY DocGia, LEFT JOIN để tính tổng"],
        ["sp_LayPhieuMuonSapDenHan",    "Lấy phiếu mượn sắp đến hạn N ngày",   "STRING_AGG tên sách, JOIN DocGia lấy Email"],
        ["sp_Dashboard",                "7 KPI Dashboard",                       "SUBQUERY đếm mỗi trạng thái trong 1 lần gọi"],
    ],
    col_widths=[4.5, 4.5, 7.5]
)

doc.add_paragraph()
h(2, "4.4. Views và Triggers (5 Views, 2 Triggers)")
add_table(
    ["Object", "Loại", "Mục đích"],
    [
        ["vw_SachDangMuon",      "VIEW",    "Danh sách sách đang được mượn + thông tin độc giả + ngày hạn"],
        ["vw_PhieuMuonDayDu",   "VIEW",    "JOIN 5 bảng: PhieuMuon + DocGia + CTPhieuMuon + Sach + PhieuTra"],
        ["vw_ThongKeSach",       "VIEW",    "Thống kê tổng hợp mỗi sách: số lần mượn, lần cuối mượn"],
        ["vw_DocGiaQuaHan",      "VIEW",    "Danh sách độc giả đang mượn sách quá hạn"],
        ["vw_TonKho",            "VIEW",    "Tồn kho hiện tại với % tỉ lệ mượn"],
        ["trg_KiemTraTonKho",    "TRIGGER", "INSTEAD OF INSERT trên CTPhieuMuon: rollback nếu tồn kho = 0"],
        ["trg_CapNhatTonKho",    "TRIGGER", "AFTER DELETE trên CTPhieuMuon: hoàn trả tồn kho"],
    ],
    col_widths=[4, 2.5, 10]
)

page_break()

# ════════════════════════════════════════════════════════════════════
# V. SƠ ĐỒ LUỒNG NGHIỆP VỤ
# ════════════════════════════════════════════════════════════════════
h(1, "V. SƠ ĐỒ LUỒNG NGHIỆP VỤ")

h(2, "5.1. Luồng Mượn Sách (sp_LapPhieuMuon)")
para(
    "Khi nhân viên lập phiếu mượn, hệ thống thực hiện 3 bước kiểm tra trước khi ghi dữ liệu: "
    "(1) Độc giả có đang hoạt động không? (2) Số sách đang mượn có vượt giới hạn không? "
    "(3) Tồn kho có đủ không? Nếu tất cả hợp lệ, SP dùng Transaction để đảm bảo atomic."
)
doc.add_paragraph()
img("04-sequence-muon-sach.png", "Hình 5: Sequence Diagram — Luồng lập phiếu mượn sách", width=16)

doc.add_paragraph()
h(2, "5.2. Luồng Trả Sách (sp_TraSach)")
para(
    "Khi trả sách, nhân viên chọn tình trạng sách (Tốt/Hỏng/Mất). "
    "Stored Procedure tự động tính tiền phạt theo công thức: "
    "TienPhat = SoNgayTreHan × MucPhatMoiNgay + Phat_HuHong (nếu có). "
    "Tồn kho chỉ được hoàn trả nếu sách không bị mất."
)
doc.add_paragraph()
img("05-sequence-tra-sach.png", "Hình 6: Sequence Diagram — Luồng trả sách và tính tiền phạt", width=16)

doc.add_paragraph()
h(2, "5.3. Luồng Email Nhắc Nhở Hàng Ngày (Hangfire Job)")
para(
    "NhacNhoHanTraJob được Hangfire scheduler kích hoạt lúc 7:00 AM mỗi ngày "
    "theo biểu thức Cron \"0 7 * * *\". Job gọi sp_LayPhieuMuonSapDenHan "
    "với tham số @SoNgayTruoc (lấy từ CauHinhHeThong), lấy danh sách phiếu "
    "mượn sắp đến hạn, sau đó gửi email HTML qua Gmail SMTP cho từng độc giả."
)
doc.add_paragraph()
img("06-sequence-email-job.png", "Hình 7: Sequence Diagram — Hangfire Email Job 7:00 AM", width=16)

page_break()

# ════════════════════════════════════════════════════════════════════
# VI. TRIỂN KHAI HỆ THỐNG
# ════════════════════════════════════════════════════════════════════
h(1, "VI. TRIỂN KHAI HỆ THỐNG")

h(2, "6.1. Sơ đồ triển khai (Deployment Diagram)")
para(
    "Hệ thống triển khai trong mạng nội bộ trường, truy cập qua WireGuard VPN. "
    "Máy chủ ứng dụng chạy Kestrel (ASP.NET Core built-in) và Hangfire Worker. "
    "SQL Server nằm trên máy chủ riêng (10.10.10.215) trong mạng LAN."
)
doc.add_paragraph()
img("08-deployment-diagram.png", "Hình 8: Deployment Diagram — Topology triển khai", width=15)

doc.add_paragraph()
h(2, "6.2. Cấu hình hệ thống")
add_table(
    ["Thành phần", "Giá trị / Cấu hình"],
    [
        ["SQL Server",        "10.10.10.215:1433 | Database: QuanLyThuVien | Auth: SQL Server"],
        ["Kestrel",           "Port 5000 (HTTP) — có thể dùng Nginx reverse proxy cho HTTPS"],
        ["Hangfire Storage",  "SQL Server (Hangfire tạo bảng riêng trong cùng DB)"],
        ["Email SMTP",        "smtp.gmail.com:587 | STARTTLS | Gmail App Password"],
        ["QR Code Storage",   "wwwroot/qr/{MaQR}.png — phục vụ tĩnh qua Kestrel"],
        ["Session / Cookie",  "HttpOnly Cookie, tên: LibraryAuth, ExpireTimeSpan: 8 giờ"],
        ["VPN",               "WireGuard tới 45.122.253.200:51820 → 10.60.10.x"],
    ],
    col_widths=[4, 12.5]
)

doc.add_paragraph()
h(2, "6.3. Hướng dẫn cài đặt")
add_table(
    ["Bước", "Lệnh / Hành động"],
    [
        ["1. Cài .NET 8 SDK",    "winget install Microsoft.DotNet.SDK.8"],
        ["2. Tạo Database",      "sqlcmd -S 10.10.10.215 -i sql/01-db-schema.sql"],
        ["3. Tạo SPs",           "sqlcmd -S 10.10.10.215 -i sql/02-stored-procedures.sql"],
        ["4. Tạo Views/Triggers","sqlcmd -S 10.10.10.215 -i sql/03-views-triggers.sql"],
        ["5. Cấu hình DB",       "Sửa appsettings.json → Password=<SA_PASSWORD>"],
        ["6. Cấu hình Email",    "Sửa EmailSettings → Password=<GMAIL_APP_PASSWORD>"],
        ["7. Restore packages",  "dotnet restore LibraryManagement.sln"],
        ["8. Build",             "dotnet build LibraryManagement.sln -c Release"],
        ["9. Chạy",              "dotnet run --project LibraryManagement.Web"],
        ["10. Truy cập",         "http://localhost:5000 — Login: admin / Admin@123"],
    ],
    col_widths=[4, 12.5]
)

page_break()

# ════════════════════════════════════════════════════════════════════
# VII. PHÂN CÔNG CÔNG VIỆC
# ════════════════════════════════════════════════════════════════════
h(1, "VII. PHÂN CÔNG CÔNG VIỆC CHI TIẾT")

h(2, "7.1. Bùi Văn Cơ — B22DTCN001")
para("Chịu trách nhiệm phần Sách, Độc giả, Danh mục và tích hợp QR Code.", bold=True)
add_table(
    ["Nhiệm vụ", "File chính", "Mô tả"],
    [
        ["CRUD Sách",         "SachController.cs\nSachRepository.cs",           "Thêm/sửa/xóa sách, phân trang, filter theo thể loại/tác giả"],
        ["Sinh QR Code",      "QrService.cs\nSachController.QrImage()",         "QRCoder tạo PNG lưu wwwroot/qr/{MaQR}.png, endpoint trả ảnh"],
        ["CRUD Độc giả",      "DocGiaController.cs\nDocGiaRepository.cs",       "Quản lý hồ sơ học sinh, lịch sử mượn-trả, khóa tài khoản"],
        ["Danh mục",          "DanhMucController.cs",                           "CRUD TheLoai, TacGia, NhaXuatBan — inline AJAX edit"],
        ["Domain entities",   "Domain/Entities/*.cs",                           "12 entity classes, clean POCO, DataAnnotations"],
        ["EF Core config",    "AppDbContext.cs",                                "Fluent API cho 12 DbSet, index, unique constraint"],
    ],
    col_widths=[3.5, 5, 8]
)

doc.add_paragraph()
h(2, "7.2. Tạ Văn Tuân — B22DTCN039")
para("Chịu trách nhiệm phần Mượn sách, Gia hạn, Tìm kiếm, Thống kê và Export.", bold=True)
add_table(
    ["Nhiệm vụ", "File chính", "Mô tả"],
    [
        ["Lập phiếu mượn",   "MuonSachController.cs\nPhieuMuonRepository.cs",  "AJAX tìm độc giả/sách, JSON → sp_LapPhieuMuon OUTPUT"],
        ["Gia hạn",          "GiaHanController.cs\nGiaHanRepository.cs",        "sp_GiaHanPhieuMuon, kiểm tra số lần, hiển thị hạn mới"],
        ["Tìm kiếm",         "TimKiemController.cs",                            "[AllowAnonymous], gọi sp_TimKiemSach CTE phân trang"],
        ["Thống kê",         "ThongKeController.cs",                            "ADO.NET SqlCommand sp_ThongKe*, chart data cho view"],
        ["Export Excel",     "ExportService.cs",                                "ClosedXML: sheet sách, sheet lịch sử, sheet thống kê"],
        ["Application DTOs", "Application/DTOs/*.cs",                           "35+ DTO records: SachDto, PhieuMuonDto, ThongKeDto..."],
    ],
    col_widths=[3.5, 5, 8]
)

doc.add_paragraph()
h(2, "7.3. Nguyễn Văn Linh — B22DTCN022 (Nhóm trưởng)")
para("Chịu trách nhiệm phần Trả sách, Cấu hình, Tài khoản, Email và Dashboard.", bold=True)
add_table(
    ["Nhiệm vụ", "File chính", "Mô tả"],
    [
        ["Trả sách",         "TraSachController.cs\nPhieuTraRepository.cs",    "sp_TraSach OUTPUT MaPhieuTra + TienPhat, tính phạt hỏng/mất"],
        ["Thu tiền phạt",    "TraSachController.ThuPhat()",                    "AJAX cập nhật DaThuPhat = 1 trong PhieuTra"],
        ["Email Hangfire",   "EmailService.cs\nNhacNhoHanTraJob.cs",           "MailKit SMTP, Hangfire recurring cron 0 7 * * *"],
        ["Dashboard",        "HomeController.cs\nIndex.cshtml",                "7 KPI LINQ query, top 5 quá hạn, progress bar"],
        ["Cấu hình HT",      "CauHinhController.cs\nCauHinhRepository.cs",    "Key-value config: ngày mượn, lần gia hạn, mức phạt, email"],
        ["Tài khoản",        "TaiKhoanController.cs\nTaiKhoanRepository.cs",  "Admin CRUD NV, BCrypt hash mật khẩu, reset password"],
        ["Program.cs + Auth","Program.cs",                                     "DI toàn bộ, Cookie Auth, Hangfire config, HangfireAuthFilter"],
        ["SQL scripts",      "sql/01,02,03-*.sql",                             "Thiết kế 12 bảng, 8 SPs, 5 Views, 2 Triggers, seed data"],
    ],
    col_widths=[3.5, 5, 8]
)

page_break()

# ════════════════════════════════════════════════════════════════════
# VIII. KẾT QUẢ VÀ ĐÁNH GIÁ
# ════════════════════════════════════════════════════════════════════
h(1, "VIII. KẾT QUẢ VÀ ĐÁNH GIÁ")

h(2, "8.1. Kết quả đạt được")
add_table(
    ["Tiêu chí", "Kết quả", "Ghi chú"],
    [
        ["Build Status",        "✅ 0 errors, 1 warning",   "Warning NU1902: MailKit vulnerability (non-blocking)"],
        ["Số chức năng",        "15/15 hoàn thành",          "Tất cả theo đặc tả"],
        ["Controllers",         "12 controllers",            "500+ dòng code xử lý nghiệp vụ"],
        ["Views",               "41+ Razor views",           "Bootstrap 5 responsive, dark sidebar"],
        ["Stored Procedures",   "8 SPs với Transaction",     "ACID-compliant, OUTPUT parameters"],
        ["Domain Exceptions",   "3 custom exceptions",       "TonKho, GioiHan, QuaHanGiaHan"],
        ["Background Jobs",     "1 Hangfire recurring job",  "Email nhắc nhở 7:00 AM daily"],
        ["Code Architecture",   "Clean Architecture 4L",     "Dependency Inversion đúng chuẩn"],
        ["Security",            "BCrypt + Cookie Auth",      "Mật khẩu hash, HTTPS-ready"],
    ],
    col_widths=[4, 4, 8.5]
)

doc.add_paragraph()
h(2, "8.2. Hạn chế và hướng phát triển")
bullet([
    "Chưa có giao diện mobile (responsive chỉ ở mức cơ bản) → Phát triển Flutter app kết nối API",
    "Tìm kiếm full-text chưa tối ưu cho kho lớn → Tích hợp Elasticsearch",
    "Email template đơn giản (plain HTML) → Sử dụng Razor Email Templates với Fluid",
    "Dashboard chưa có biểu đồ trực quan → Thêm Chart.js cho đồ thị thống kê",
    "Chưa có upload ảnh bìa sách → Tích hợp Azure Blob Storage hoặc MinIO",
    "Chưa có unit tests → Bổ sung xUnit + Moq cho Service layer",
    "Deployment thủ công → CI/CD pipeline với GitHub Actions",
])

doc.add_paragraph()
h(2, "8.3. Bài học kinh nghiệm")
bullet([
    "Clean Architecture giúp tách biệt rõ ràng concern, dễ test và maintain",
    "Stored Procedures với Transaction đảm bảo tính nhất quán dữ liệu tốt hơn code thuần EF Core",
    "Hangfire là giải pháp đơn giản và đáng tin cậy cho background jobs trong .NET",
    "Cookie Authentication đủ mạnh cho internal web app, không cần JWT cho trường hợp này",
    "Quan trọng: define interface trước → viết mock/test dễ hơn, phân công dễ hơn",
])

doc.add_paragraph()
doc.add_paragraph()
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
run = p.add_run("--- HẾT ---")
run.bold = True
run.font.size = Pt(12)
run.font.color.rgb = RGBColor(0x19, 0x87, 0x54)

# Save
doc.save(OUT)
print(f"✅ Saved: {OUT}")

import os
size = os.path.getsize(OUT)
print(f"   Size: {size/1024:.1f} KB")
