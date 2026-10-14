import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CommerceService } from '../../core/commerce.service';
import { parseCsv } from '../../core/csv';

const FILE_NAME = /^commerce_\d{8}\.csv$/i;
const PREVIEW_LIMIT = 100;

@Component({
  selector: 'app-upload',
  template: `
    <h2>Cargar archivo</h2>
    <input type="file" accept=".csv" (change)="onFile($event)" />

    @if (error()) { <p class="msg err">{{ error() }}</p> }
    @if (message()) { <p class="msg ok">{{ message() }}</p> }

    @if (headers().length) {
      <p>Previsualización: {{ total() }} registros
        @if (total() > rows().length) { (mostrando {{ rows().length }}) }
      </p>
      <div class="scroll">
        <table>
          <thead><tr>@for (h of headers(); track h) { <th>{{ h }}</th> }</tr></thead>
          <tbody>
            @for (r of rows(); track $index) {
              <tr>@for (c of r; track $index) { <td>{{ c }}</td> }</tr>
            }
          </tbody>
        </table>
      </div>
      <button (click)="send()" [disabled]="sending() || !!error()">
        {{ sending() ? 'Enviando...' : 'Enviar al servidor' }}
      </button>
    }
  `,
})
export class Upload {
  private readonly api = inject(CommerceService);
  private file: File | null = null;

  readonly headers = signal<string[]>([]);
  readonly rows = signal<string[][]>([]);
  readonly total = signal(0);
  readonly error = signal('');
  readonly message = signal('');
  readonly sending = signal(false);

  async onFile(ev: Event) {
    this.reset();
    const file = (ev.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.file = file;

    if (!FILE_NAME.test(file.name)) this.error.set('El nombre debe ser commerce_DDMMYYYY.csv');
    if (file.size === 0) { this.error.set('El archivo está vacío.'); return; }

    const { headers, rows } = parseCsv(await file.text());
    if (!rows.length) { this.error.set('El archivo no contiene registros.'); return; }
    this.headers.set(headers);
    this.total.set(rows.length);
    this.rows.set(rows.slice(0, PREVIEW_LIMIT));
  }

  send() {
    if (!this.file) return;
    this.sending.set(true);
    this.api.upload(this.file).subscribe({
      next: r => { this.message.set(`${r.inserted} registros cargados.`); this.sending.set(false); },
      error: (e: HttpErrorResponse) => {
        this.error.set(e.error?.message ?? 'Error al enviar el archivo.');
        this.sending.set(false);
      },
    });
  }

  private reset() {
    this.file = null;
    this.headers.set([]); this.rows.set([]); this.total.set(0);
    this.error.set(''); this.message.set('');
  }
}
