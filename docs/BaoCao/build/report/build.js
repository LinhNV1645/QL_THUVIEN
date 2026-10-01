const fs = require('fs');
const path = require('path');
const {
  Document, Packer, Paragraph, TextRun, AlignmentType, Footer, PageNumber, TableOfContents,
  LevelFormat, NumberFormat, SectionType,
} = require('docx');
const { CM } = require('./lib');

const OUT = path.resolve(__dirname, '../../MaSV_HoTen.docx');
const FONT = 'Times New Roman';

const chapters = (process.env.CHAPTERS || '1,2,3,4,ref').split(',');
const mods = { 1: './ch1', 2: './ch2', 3: './ch3', 4: './ch4', ref: './refs' };

const title = (text) => new Paragraph({
  alignment: AlignmentType.CENTER,
  spacing: { before: 0, after: 240 },
  children: [new TextRun({ text, bold: true, size: 28 })],
});

const body = [];
for (const c of chapters) body.push(...require(mods[c])());

const doc = new Document({
  creator: 'MaSV_HoTen',
  title: 'Báo cáo kết quả thực tập cơ sở – Quản lý thư viện cho Trường THCS Thanh Xuân',
  features: { updateFields: true },
  styles: {
    default: {
      document: {
        run: { font: FONT, size: 26 },
        paragraph: { spacing: { line: 360, before: 0, after: 120 } },
      },
    },
    paragraphStyles: [
      {
        id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 28, bold: true, color: '000000' },
        paragraph: { alignment: AlignmentType.CENTER, spacing: { before: 0, after: 240 }, keepNext: true, pageBreakBefore: true, outlineLevel: 0 },
      },
      {
        id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 26, bold: true, color: '000000' },
        paragraph: { spacing: { before: 200, after: 100 }, keepNext: true, outlineLevel: 1 },
      },
      {
        id: 'Heading3', name: 'Heading 3', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 26, bold: true, italics: true, color: '000000' },
        paragraph: { spacing: { before: 160, after: 80 }, keepNext: true, outlineLevel: 2 },
      },
      {
        id: 'Caption', name: 'caption', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 24, italics: true, color: '000000' },
        paragraph: { alignment: AlignmentType.CENTER, spacing: { before: 60, after: 160 } },
      },
    ],
  },
  numbering: {
    config: [
      {
        reference: 'bullet',
        levels: [
          { level: 0, format: LevelFormat.BULLET, text: '–', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: CM * 1.25, hanging: CM * 0.5 } } } },
          { level: 1, format: LevelFormat.BULLET, text: '+', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: CM * 1.9, hanging: CM * 0.5 } } } },
        ],
      },
      {
        reference: 'number',
        levels: [
          { level: 0, format: LevelFormat.DECIMAL, text: '%1.', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: CM * 1.25, hanging: CM * 0.6 } } } },
        ],
      },
    ],
  },
  sections: [
    {
      // Trang bìa: để trống, không đánh số trang.
      properties: {
        page: { size: { width: 11906, height: 16838 }, margin: { top: 2 * CM, bottom: 2 * CM, left: 3 * CM, right: 2 * CM } },
      },
      children: [new Paragraph({ children: [] })],
    },
    {
      properties: {
        type: SectionType.NEXT_PAGE,
        page: {
          size: { width: 11906, height: 16838 },
          margin: { top: 2 * CM, bottom: 2 * CM, left: 3 * CM, right: 2 * CM, footer: CM },
          pageNumbers: { start: 1, formatType: NumberFormat.DECIMAL },
        },
      },
      footers: {
        default: new Footer({
          children: [new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [new TextRun({ children: [PageNumber.CURRENT], size: 24 })],
          })],
        }),
      },
      children: [
        title('MỤC LỤC'),
        new TableOfContents('Mục lục', { hyperlink: true, headingStyleRange: '1-3' }),
        new Paragraph({ pageBreakBefore: true, alignment: AlignmentType.CENTER, spacing: { after: 240 }, children: [new TextRun({ text: 'DANH MỤC HÌNH VẼ', bold: true, size: 28 })] }),
        new TableOfContents('Danh mục hình vẽ', { hyperlink: true, captionLabelIncludingNumbers: 'Hình' }),
        new Paragraph({ pageBreakBefore: true, alignment: AlignmentType.CENTER, spacing: { after: 240 }, children: [new TextRun({ text: 'DANH MỤC BẢNG', bold: true, size: 28 })] }),
        new TableOfContents('Danh mục bảng', { hyperlink: true, captionLabelIncludingNumbers: 'Bảng' }),
        ...body,
      ],
    },
  ],
});

Packer.toBuffer(doc).then((buf) => {
  fs.writeFileSync(OUT, buf);
  console.log('wrote', OUT, buf.length);
});
