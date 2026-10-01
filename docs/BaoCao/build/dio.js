// Thư viện nhỏ sinh file .drawio (mxGraph XML, không nén) với bảng màu và font thống nhất.
const fs = require('fs');
const path = require('path');

const FONT = 'Arial';

const C = {
  text: '#1F2937',
  line: '#374151',
  green: '#198754', greenL: '#E8F5EE', greenM: '#CDE9D9',
  blue: '#1F5FAD', blueL: '#E8F0FB', blueM: '#C9DCF5',
  orange: '#B45309', orangeL: '#FFF4E5', orangeM: '#FDE2BF',
  purple: '#6B3FA0', purpleL: '#F3ECFB',
  gray: '#6B7280', grayL: '#F5F6F8', grayM: '#E5E7EB',
  amber: '#9A6B00', amberL: '#FFF8DB',
  red: '#B42318', redL: '#FDECEA',
};

const base = (size = 13) =>
  `html=1;whiteSpace=wrap;fontFamily=${FONT};fontSize=${size};fontColor=${C.text};`;

const esc = (s) => String(s)
  .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

const S = {
  actor: (size = 13) => `shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;outlineConnect=0;html=1;fontFamily=${FONT};fontSize=${size};fontColor=${C.text};strokeColor=${C.line};fillColor=#FFFFFF;strokeWidth=1.5;fontStyle=1;`,
  usecase: (fill = C.greenL, stroke = C.green, size = 13) => `ellipse;${base(size)}fillColor=${fill};strokeColor=${stroke};strokeWidth=1.5;`,
  boundary: (size = 14) => `rounded=0;${base(size)}fillColor=#FCFDFC;strokeColor=${C.green};strokeWidth=1.5;verticalAlign=top;fontStyle=1;spacingTop=6;`,
  box: (fill, stroke, size = 13, extra = '') => `rounded=1;arcSize=12;${base(size)}fillColor=${fill};strokeColor=${stroke};strokeWidth=1.3;${extra}`,
  rect: (fill, stroke, size = 13, extra = '') => `rounded=0;${base(size)}fillColor=${fill};strokeColor=${stroke};strokeWidth=1.3;${extra}`,
  text: (size = 13, extra = '') => `text;${base(size)}strokeColor=none;fillColor=none;${extra}`,
  note: (size = 12) => `shape=note;size=14;${base(size)}fillColor=${C.amberL};strokeColor=${C.amber};align=left;spacingLeft=6;`,
  assoc: () => `endArrow=none;html=1;strokeColor=${C.line};strokeWidth=1.3;`,
  // Liên kết actor → use case: xuất phát từ vai actor, kết thúc ở mép trái ellipse để không cắt use case khác.
  assocLeft: () => `endArrow=none;html=1;strokeColor=${C.line};strokeWidth=1.3;exitX=1;exitY=0.3;exitDx=0;exitDy=0;entryX=0;entryY=0.5;entryDx=0;entryDy=0;`,
  dep: (size = 12) => `endArrow=open;endSize=10;dashed=1;html=1;strokeColor=${C.line};strokeWidth=1.2;fontFamily=${FONT};fontSize=${size};fontColor=${C.text};labelBackgroundColor=#FFFFFF;`,
  general: () => `endArrow=block;endFill=0;endSize=14;html=1;strokeColor=${C.line};strokeWidth=1.4;`,
};

class Diagram {
  constructor(name, { pageWidth = 827, pageHeight = 1169 } = {}) {
    this.name = name;
    this.cells = [];
    this.n = 2;
    this.pageWidth = pageWidth;
    this.pageHeight = pageHeight;
  }

  nextId(prefix = 'c') { return `${prefix}${this.n++}`; }

  vertex(value, style, x, y, w, h, { id, parent = '1' } = {}) {
    id = id || this.nextId();
    this.cells.push(
      `<mxCell id="${id}" value="${esc(value)}" style="${style}" vertex="1" parent="${parent}">` +
      `<mxGeometry x="${x}" y="${y}" width="${w}" height="${h}" as="geometry"/></mxCell>`);
    return id;
  }

  // Cạnh nối hai cell; points = [[x,y],...] điểm gấp tuyệt đối.
  edge(source, target, value, style, { points = [], id, labels = [] } = {}) {
    id = id || this.nextId('e');
    const pts = points.length
      ? `<Array as="points">${points.map(([x, y]) => `<mxPoint x="${x}" y="${y}"/>`).join('')}</Array>` : '';
    this.cells.push(
      `<mxCell id="${id}" value="${esc(value || '')}" style="${style}" edge="1" parent="1"` +
      `${source ? ` source="${source}"` : ''}${target ? ` target="${target}"` : ''}>` +
      `<mxGeometry relative="1" as="geometry">${pts}</mxGeometry></mxCell>`);
    for (const lb of labels) this.edgeLabel(id, lb.text, lb.pos, lb.dx || 0, lb.dy || 0, lb.size, lb.perp || 0);
    return id;
  }

  // Cạnh tự do theo toạ độ tuyệt đối (dùng cho thông điệp trong biểu đồ tuần tự).
  line(x1, y1, x2, y2, value, style, { points = [], id } = {}) {
    id = id || this.nextId('l');
    const pts = points.length
      ? `<Array as="points">${points.map(([x, y]) => `<mxPoint x="${x}" y="${y}"/>`).join('')}</Array>` : '';
    this.cells.push(
      `<mxCell id="${id}" value="${esc(value || '')}" style="${style}" edge="1" parent="1">` +
      `<mxGeometry relative="1" as="geometry"><mxPoint x="${x1}" y="${y1}" as="sourcePoint"/>` +
      `<mxPoint x="${x2}" y="${y2}" as="targetPoint"/>${pts}</mxGeometry></mxCell>`);
    return id;
  }

  // Nhãn gắn vào cạnh: pos từ -1 (đầu nguồn) đến 1 (đầu đích).
  edgeLabel(edgeId, text, pos, dx = 0, dy = 0, size = 12, perp = 0) {
    const id = this.nextId('lb');
    this.cells.push(
      `<mxCell id="${id}" value="${esc(text)}" style="edgeLabel;html=1;fontFamily=${FONT};fontSize=${size};fontColor=${C.text};labelBackgroundColor=#FFFFFF;resizable=0;" vertex="1" connectable="0" parent="${edgeId}">` +
      `<mxGeometry x="${pos}" y="${perp}" relative="1" as="geometry"><mxPoint x="${dx}" y="${dy}" as="offset"/></mxGeometry></mxCell>`);
    return id;
  }

  toXml() {
    return `<mxfile host="drawio" agent="LibraryManagement report generator">\n` +
      `<diagram name="${esc(this.name)}" id="${this.name.replace(/[^a-zA-Z0-9]/g, '')}">\n` +
      `<mxGraphModel dx="1200" dy="900" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="${this.pageWidth}" pageHeight="${this.pageHeight}" background="#FFFFFF" math="0" shadow="0">\n` +
      `<root>\n<mxCell id="0"/>\n<mxCell id="1" parent="0"/>\n${this.cells.join('\n')}\n</root>\n</mxGraphModel>\n</diagram>\n</mxfile>\n`;
  }

  save(dir, file) {
    fs.mkdirSync(dir, { recursive: true });
    const p = path.join(dir, file);
    fs.writeFileSync(p, this.toXml(), 'utf8');
    return p;
  }
}

module.exports = { Diagram, S, C, FONT, base, esc };
