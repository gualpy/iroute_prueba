import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CommerceQuarantine, CommerceService } from '../../core/commerce.service';

@Component({
  selector: 'app-errors',
  imports: [DatePipe],
  template: `
    <h2>Registros con errores</h2>
    <button (click)="load()">Actualizar</button>
    @if (error()) { <p class="msg err">{{ error() }}</p> }
    @if (loading()) { <p>Cargando...</p> }
    @else if (!items().length) { <p>Sin registros en cuarentena.</p> }
    @else {
      <div class="scroll">
        <table>
          <thead><tr>
            <th>Fecha</th><th>Documento</th><th>Comercio</th><th>Razón social</th><th>Motivo</th>
          </tr></thead>
          <tbody>
            @for (i of items(); track i.id) {
              <tr>
                <td>{{ i.pcProcessdate | date: 'dd/MM/yyyy' }}</td>
                <td>{{ i.pcNumdoc }}</td>
                <td>{{ i.pcNomcomred }}</td>
                <td>{{ i.pcRazonsocial }}</td>
                <td>{{ i.motivo }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
})
export class Errors {
  private readonly api = inject(CommerceService);
  readonly items = signal<CommerceQuarantine[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');

  constructor() { this.load(); }

  load() {
    this.loading.set(true); this.error.set('');
    this.api.quarantine().subscribe({
      next: d => { this.items.set(d); this.loading.set(false); },
      error: () => { this.error.set('No se pudo obtener la lista.'); this.loading.set(false); },
    });
  }
}
