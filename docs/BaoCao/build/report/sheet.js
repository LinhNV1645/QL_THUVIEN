// Ghép các trang preview thành ảnh tổng hợp 3x2 để rà soát nhanh: node sheet.js <dir>
const fs = require('fs');
const path = require('path');
const { createCanvas, loadImage } = require('canvas');

(async () => {
  const dir = process.argv[2];
  const files = fs.readdirSync(dir).filter((f) => /^p\d+\.png$/.test(f)).sort();
  const per = 6;
  for (let s = 0; s < files.length; s += per) {
    const imgs = await Promise.all(files.slice(s, s + per).map((f) => loadImage(path.join(dir, f))));
    const w = imgs[0].width, h = imgs[0].height;
    const c = createCanvas(w * 3 + 8, h * 2 + 4);
    const x = c.getContext('2d');
    x.fillStyle = '#777';
    x.fillRect(0, 0, c.width, c.height);
    imgs.forEach((im, i) => x.drawImage(im, (i % 3) * (w + 4), Math.floor(i / 3) * (h + 4)));
    fs.writeFileSync(path.join(dir, `sheet${s / per + 1}.png`), c.toBuffer('image/png'));
  }
  console.log('sheets', Math.ceil(files.length / per));
})();
