// Bộ dựng biểu đồ tuần tự UML trên nền Diagram (toạ độ tuyệt đối, lifeline vẽ tay).
const { S, C, FONT, base } = require('../dio');

const msgStyle = (size, ret) =>
  `html=1;verticalAlign=bottom;labelBackgroundColor=none;fontFamily=${FONT};fontSize=${size};fontColor=${C.text};` +
  `strokeColor=${C.line};strokeWidth=1.2;endSize=8;` +
  (ret ? 'dashed=1;endArrow=open;endFill=0;' : 'endArrow=block;endFill=1;');

const HEAD = {
  actor: { fill: '#FFFFFF', stroke: C.line },
  boundary: { fill: C.greenL, stroke: C.green },
  control: { fill: C.blueL, stroke: C.blue },
  object: { fill: C.blueL, stroke: C.blue },
  repo: { fill: C.purpleL, stroke: C.purple },
  db: { fill: C.orangeL, stroke: C.orange },
};

class Seq {
  constructor(d, parts, { top = 10, size = 13 } = {}) {
    this.d = d;
    this.size = size;
    this.top = top;
    this.p = {};
    this.back = [];
    for (const p of parts) this.p[p.key] = { ...p, minY: Infinity, maxY: -Infinity };
    for (const p of parts) this.drawHead(this.p[p.key]);
    this.insertAt = d.cells.length;
    this.start = top + 110;
  }

  drawHead(p) {
    const { d, top } = this;
    const h = HEAD[p.kind];
    const cx = p.cx;
    const lbl = (y, w = 170) => d.vertex(p.label, S.text(this.size, 'fontStyle=1;align=center;verticalAlign=top;'), cx - w / 2, y, w, 36);
    switch (p.kind) {
      case 'actor':
        d.vertex('', S.actor(this.size), cx - 15, top, 30, 50);
        lbl(top + 52);
        p.lifeTop = top + 90;
        break;
      case 'boundary':
        d.vertex('', `shape=umlBoundary;${base()}fillColor=${h.fill};strokeColor=${h.stroke};strokeWidth=1.4;`, cx - 26, top + 6, 52, 40);
        lbl(top + 52);
        p.lifeTop = top + 90;
        break;
      case 'control':
        d.vertex('', `shape=umlControl;${base()}fillColor=${h.fill};strokeColor=${h.stroke};strokeWidth=1.4;`, cx - 20, top + 4, 40, 44);
        lbl(top + 52);
        p.lifeTop = top + 90;
        break;
      case 'db':
        d.vertex(p.label, `shape=cylinder3;boundedLbl=1;size=9;${base(this.size)}fillColor=${h.fill};strokeColor=${h.stroke};strokeWidth=1.4;fontStyle=1;`, cx - (p.w || 110) / 2, top + 14, p.w || 110, 66);
        p.lifeTop = top + 80;
        break;
      default:
        d.vertex(p.label, S.rect(h.fill, h.stroke, this.size, 'fontStyle=1;'), cx - (p.w || 170) / 2, top + 20, p.w || 170, 56);
        p.lifeTop = top + 76;
    }
  }

  track(key, y1, y2 = y1) {
    const p = this.p[key];
    if (p.kind === 'actor') return;
    p.minY = Math.min(p.minY, y1);
    p.maxY = Math.max(p.maxY, y2);
  }

  off(key) { return this.p[key].kind === 'actor' ? 0 : 6; }

  msg(a, b, label, y, ret = false) {
    const pa = this.p[a], pb = this.p[b];
    const dir = pb.cx > pa.cx ? 1 : -1;
    const x1 = pa.cx + dir * this.off(a);
    const x2 = pb.cx - dir * this.off(b);
    this.d.line(x1, y, x2, y, label, msgStyle(this.size, ret));
    this.track(a, y); this.track(b, y);
  }

  ret(a, b, label, y) { this.msg(a, b, label, y, true); }

  self(key, label, y, h = 26, { left = false } = {}) {
    const p = this.p[key];
    const sx = left ? p.cx - 6 : p.cx + 6;
    const ox = left ? p.cx - 36 : p.cx + 36;
    const style = msgStyle(this.size, false) +
      (left ? 'align=right;spacingRight=4;' : 'align=left;spacingLeft=4;') +
      'verticalAlign=middle;labelBackgroundColor=#FFFFFF;labelPosition=' + (left ? 'left' : 'right') + ';';
    this.d.line(sx, y, sx, y + h, label, style, { points: [[ox, y], [ox, y + h]] });
    this.track(key, y, y + h);
  }

  // Khung tổ hợp (alt/opt/loop). sections: [{ y, guard }] là các đường phân cách.
  frame(op, guard, x1, y1, x2, y2, sections = []) {
    const d = this.d;
    const st = `shape=umlFrame;html=1;whiteSpace=wrap;pointerEvents=0;width=46;height=22;fontFamily=${FONT};fontSize=12;fontStyle=1;fontColor=${C.text};fillColor=none;strokeColor=${C.gray};strokeWidth=1.2;`;
    this.back.push(() => {
      d.vertex(op, st, x1, y1, x2 - x1, y2 - y1);
      d.vertex(guard, S.text(12, 'fontStyle=2;align=left;'), x1 + 52, y1 + 1, 420, 20);
      for (const s of sections) {
        d.line(x1, s.y, x2, s.y, '', `endArrow=none;dashed=1;dashPattern=6 4;html=1;strokeColor=${C.gray};strokeWidth=1.2;`);
        d.vertex(s.guard, S.text(12, 'fontStyle=2;align=left;'), x1 + 8, s.y + 1, 420, 20);
      }
    });
  }

  finish(bottom) {
    const d = this.d;
    const tail = d.cells.splice(this.insertAt);
    for (const p of Object.values(this.p)) {
      d.line(p.cx, p.lifeTop, p.cx, bottom, '', `endArrow=none;dashed=1;dashPattern=5 4;html=1;strokeColor=${C.gray};strokeWidth=1.2;`);
    }
    for (const p of Object.values(this.p)) {
      if (p.minY === Infinity) continue;
      const h = HEAD[p.kind];
      d.vertex('', S.rect('#FFFFFF', h.stroke, 12, 'strokeWidth=1.2;'), p.cx - 6, p.minY - 8, 12, p.maxY - p.minY + 16);
    }
    for (const f of this.back) f();
    d.cells.push(...tail);
  }
}

module.exports = { Seq };
