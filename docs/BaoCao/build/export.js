// Xuất các file .drawio sang PNG (scale 2) bằng draw.io desktop CLI.
const { execFileSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const DRAWIO = path.join(process.env.LOCALAPPDATA, 'Programs', 'draw.io', 'draw.io.exe');
const SRC = path.resolve(__dirname, '../../diagrams');
const OUT = path.join(SRC, 'png');

const only = process.argv.slice(2);
fs.mkdirSync(OUT, { recursive: true });

for (const f of fs.readdirSync(SRC).filter((f) => f.endsWith('.drawio'))) {
  if (only.length && !only.some((o) => f.includes(o))) continue;
  const out = path.join(OUT, f.replace(/\.drawio$/, '.png'));
  execFileSync(DRAWIO, ['--export', '--format', 'png', '--scale', '2', '--border', '16',
    '--output', out, path.join(SRC, f)], { stdio: 'ignore' });
  console.log('exported', path.basename(out), fs.statSync(out).size);
}
