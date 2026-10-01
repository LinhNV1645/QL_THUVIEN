const path = require('path');
const OUT = path.resolve(__dirname, '../../diagrams');

const modules = [
  require('./diagrams/usecase'),
  require('./diagrams/sequences'),
  require('./diagrams/activity'),
  require('./diagrams/design'),
];

for (const m of modules) {
  for (const fn of Object.values(m)) {
    if (typeof fn === 'function') console.log('wrote', path.basename(fn(OUT)));
  }
}
