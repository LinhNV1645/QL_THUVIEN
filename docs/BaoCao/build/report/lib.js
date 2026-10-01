const fs = require('fs');
const path = require('path');
const {
  Paragraph, TextRun, HeadingLevel, AlignmentType, ImageRun, Table, TableRow, TableCell,
  WidthType, BorderStyle, ShadingType, SimpleField, VerticalAlign, PageBreak,
} = require('docx');

const ROOT = path.resolve(__dirname, '../../..');            // thư mục docs/
const DIAGRAM = (f) => path.join(ROOT, 'diagrams', 'png', f);
const SHOT = (f) => path.join(ROOT, 'ImageSystem', f);

const CM = 567;                 // twip / cm
const TEXT_W_CM = 16;           // A4 21cm - lề trái 3cm - lề phải 2cm
const PX_PER_CM = 96 / 2.54;    // docx tính kích thước ảnh theo pixel 96 dpi

// ---------- Đoạn văn ----------
// Hỗ trợ **đậm**, *nghiêng* và `mã` (in nghiêng) trong chuỗi.
function runs(text, base = {}) {
  const out = [];
  for (const part of String(text).split(/(\*\*[^*]+\*\*|\*[^*]+\*|`[^`]+`)/)) {
    if (!part) continue;
    if (part.startsWith('**')) out.push(new TextRun({ ...base, text: part.slice(2, -2), bold: true }));
    else if (part.startsWith('*')) out.push(new TextRun({ ...base, text: part.slice(1, -1), italics: true }));
    else if (part.startsWith('`')) out.push(new TextRun({ ...base, text: part.slice(1, -1), italics: true }));
    else out.push(new TextRun({ ...base, text: part }));
  }
  return out;
}

const p = (text, opts = {}) => new Paragraph({
  children: runs(text),
  alignment: AlignmentType.JUSTIFIED,
  indent: { firstLine: opts.noIndent ? 0 : CM },
  keepNext: opts.keepNext,
  spacing: opts.spacing,
});

const h1 = (text) => new Paragraph({ text, heading: HeadingLevel.HEADING_1 });
const h2 = (text) => new Paragraph({ text, heading: HeadingLevel.HEADING_2 });
const h3 = (text) => new Paragraph({ text, heading: HeadingLevel.HEADING_3 });

const bullets = (items, level = 0) => items.map((t) => new Paragraph({
  children: runs(t),
  alignment: AlignmentType.JUSTIFIED,
  numbering: { reference: 'bullet', level },
}));

let listInstance = 0;
const numbered = (items) => {
  listInstance += 1;
  const inst = listInstance;
  return items.map((t) => new Paragraph({
    children: runs(t),
    alignment: AlignmentType.JUSTIFIED,
    numbering: { reference: 'number', level: 0, instance: inst },
  }));
};

const pageBreak = () => new Paragraph({ children: [new PageBreak()] });

// ---------- Hình & chú thích ----------
function pngSize(file) {
  const b = fs.readFileSync(file);
  return { w: b.readUInt32BE(16), h: b.readUInt32BE(20), data: b };
}

// Chú thích "Hình c.n: ..." dùng trường SEQ để Word dựng danh mục hình (\c "Hình").
function captionPara(label, chapter, n, text, keepNext = false) {
  return new Paragraph({
    style: 'Caption',
    alignment: AlignmentType.CENTER,
    keepNext,
    children: [
      new TextRun(`${label} ${chapter}.`),
      new SimpleField(`SEQ ${label} \\* ARABIC \\s 1`, String(n)),
      new TextRun(`: ${text}`),
    ],
  });
}

class Figures {
  constructor(chapter) { this.chapter = chapter; this.n = 0; this.tn = 0; }

  // Trả về { label, nodes }; label dùng để dẫn chiếu trong đoạn giới thiệu.
  make(file, caption, { maxW = TEXT_W_CM, maxH = 21 } = {}) {
    this.n += 1;
    const { w, h, data } = pngSize(file);
    const scale = Math.min(maxW / (w / PX_PER_CM), maxH / (h / PX_PER_CM), 1e9);
    let wcm = (w / PX_PER_CM) * scale;
    let hcm = (h / PX_PER_CM) * scale;
    if (wcm > TEXT_W_CM) { hcm *= TEXT_W_CM / wcm; wcm = TEXT_W_CM; }
    const img = new Paragraph({
      alignment: AlignmentType.CENTER,
      keepNext: true,
      spacing: { before: 120, after: 0, line: 240 },
      children: [new ImageRun({
        type: 'png', data,
        transformation: { width: Math.round(wcm * PX_PER_CM), height: Math.round(hcm * PX_PER_CM) },
      })],
    });
    const label = `Hình ${this.chapter}.${this.n}`;
    return { label, nodes: [img, captionPara('Hình', this.chapter, this.n, caption)], wcm, hcm };
  }

  tableCaption(text) {
    this.tn += 1;
    const label = `Bảng ${this.chapter}.${this.tn}`;
    return { label, node: captionPara('Bảng', this.chapter, this.tn, text, true) };
  }
}

// ---------- Bảng ----------
const BORDER = { style: BorderStyle.SINGLE, size: 4, color: '808080' };
const BORDERS = { top: BORDER, bottom: BORDER, left: BORDER, right: BORDER, insideHorizontal: BORDER, insideVertical: BORDER };
const HEAD_FILL = 'D9E8DF';

function cellParas(content, { bold = false, size = 24, align = AlignmentType.LEFT } = {}) {
  const items = Array.isArray(content) ? content : [content];
  return items.map((t) => new Paragraph({
    alignment: align,
    spacing: { before: 20, after: 20, line: 260 },
    children: runs(t, { size, bold: bold || undefined }),
  }));
}

// widths: phần trăm theo chiều rộng vùng chữ.
function table(header, rows, widths, { size = 24, headAlign = AlignmentType.CENTER, center = [] } = {}) {
  const total = TEXT_W_CM * CM;
  const cols = widths.map((w) => Math.round((w / 100) * total));
  const mk = (cells, isHead) => new TableRow({
    tableHeader: isHead,
    cantSplit: true,
    children: cells.map((c, i) => new TableCell({
      width: { size: cols[i], type: WidthType.DXA },
      verticalAlign: VerticalAlign.CENTER,
      shading: isHead ? { type: ShadingType.CLEAR, color: 'auto', fill: HEAD_FILL } : undefined,
      margins: { left: 80, right: 80 },
      children: cellParas(c, {
        bold: isHead, size,
        align: isHead ? headAlign : (center.includes(i) ? AlignmentType.CENTER : AlignmentType.LEFT),
      }),
    })),
  });
  return new Table({
    width: { size: total, type: WidthType.DXA },
    columnWidths: cols,
    borders: BORDERS,
    rows: [...(header ? [mk(header, true)] : []), ...rows.map((r) => mk(r, false))],
  });
}

// Bảng kịch bản use case: 2 cột (mục | nội dung).
function scenario(rows) {
  const total = TEXT_W_CM * CM;
  const cols = [Math.round(total * 0.22), total - Math.round(total * 0.22)];
  return new Table({
    width: { size: total, type: WidthType.DXA },
    columnWidths: cols,
    borders: BORDERS,
    rows: rows.map(([k, v]) => new TableRow({
      cantSplit: false,
      children: [
        new TableCell({
          width: { size: cols[0], type: WidthType.DXA },
          shading: { type: ShadingType.CLEAR, color: 'auto', fill: HEAD_FILL },
          margins: { left: 80, right: 80 },
          children: cellParas(k, { bold: true, size: 24 }),
        }),
        new TableCell({
          width: { size: cols[1], type: WidthType.DXA },
          margins: { left: 80, right: 80 },
          children: cellParas(v, { size: 24 }),
        }),
      ],
    })),
  });
}

const spacer = () => new Paragraph({ spacing: { before: 0, after: 0, line: 240 }, children: [] });

module.exports = {
  p, h1, h2, h3, bullets, numbered, pageBreak, table, scenario, spacer, Figures, runs,
  DIAGRAM, SHOT, CM, TEXT_W_CM,
};
