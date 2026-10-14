/** Parser CSV mínimo (soporta comillas y comas escapadas) para la previsualización. */
export function parseCsv(text: string): { headers: string[]; rows: string[][] } {
  const lines: string[][] = [];
  let row: string[] = [];
  let cur = '';
  let quoted = false;

  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    if (quoted) {
      if (c === '"' && text[i + 1] === '"') { cur += '"'; i++; }
      else if (c === '"') quoted = false;
      else cur += c;
    } else if (c === '"') quoted = true;
    else if (c === ',') { row.push(cur.trim()); cur = ''; }
    else if (c === '\n' || c === '\r') {
      if (c === '\r' && text[i + 1] === '\n') i++;
      row.push(cur.trim()); cur = '';
      if (row.some(v => v !== '')) lines.push(row);
      row = [];
    } else cur += c;
  }
  row.push(cur.trim());
  if (row.some(v => v !== '')) lines.push(row);

  const [headers = [], ...rows] = lines;
  return { headers, rows };
}
