// Xuất các trang PDF thành PNG để kiểm tra bố cục: node preview.mjs <pdf> <outDir> [từ trang] [đến trang]
import { pdf } from 'pdf-to-img';
import fs from 'node:fs';
import path from 'node:path';

const [file, outDir, from = '1', to = '9999'] = process.argv.slice(2);
fs.mkdirSync(outDir, { recursive: true });
const doc = await pdf(file, { scale: 1 });
let i = 0;
for await (const img of doc) {
  i += 1;
  if (i < +from) continue;
  if (i > +to) break;
  fs.writeFileSync(path.join(outDir, `p${String(i).padStart(2, '0')}.png`), img);
}
console.log('pages', doc.length);
