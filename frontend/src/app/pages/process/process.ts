import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommerceService } from '../../core/commerce.service';

@Component({
  selector: 'app-process',
  imports: [FormsModule],
  template: `
    <h2>Procesar información</h2>
    <label>Fecha de proceso (pc_processdate)
      <input type="date" [(ngModel)]="date" />
    </label>
    <button (click)="run()" [disabled]="!date || running()">
      {{ running() ? 'Procesando...' : 'Procesar' }}
    </button>

    @if (error()) { <p class="msg err">{{ error() }}</p> }
    @if (result() !== null) {
      <p class="msg ok">Registros enviados a cuarentena: <strong>{{ result() }}</strong></p>
    }
  `,
})
export class Process {
  private readonly api = inject(CommerceService);
  date = '';
  readonly running = signal(false);
  readonly result = signal<number | null>(null);
  readonly error = signal('');

  run() {
    this.running.set(true); this.error.set(''); this.result.set(null);
    this.api.process(this.date).subscribe({
      next: r => { this.result.set(r.quarantined); this.running.set(false); },
      error: (e: HttpErrorResponse) => {
        this.error.set(e.error?.message ?? 'Error al procesar.');
        this.running.set(false);
      },
    });
  }
}
