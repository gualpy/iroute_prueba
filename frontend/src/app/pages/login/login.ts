import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  template: `
    <form class="login" (ngSubmit)="submit()">
      <h2>Iniciar sesión</h2>
      <label>Usuario <input name="username" [(ngModel)]="username" autocomplete="username" required /></label>
      <label>Contraseña
        <input name="password" type="password" [(ngModel)]="password" autocomplete="current-password" required />
      </label>
      @if (error()) { <p class="msg err">{{ error() }}</p> }
      <button type="submit" [disabled]="loading() || !username || !password">
        {{ loading() ? 'Ingresando...' : 'Ingresar' }}
      </button>
    </form>
  `,
  styles: `
    .login { max-width: 320px; margin: 3rem auto; display: flex; flex-direction: column; gap: 1rem; }
    label { display: flex; flex-direction: column; gap: .25rem; }
    input { padding: .45rem; }
    button { margin-left: 0; }
  `,
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  username = '';
  password = '';
  readonly loading = signal(false);
  readonly error = signal('');

  submit() {
    this.loading.set(true); this.error.set('');
    this.auth.login(this.username, this.password).subscribe({
      next: () => this.router.navigate(['/upload']),
      error: (e: HttpErrorResponse) => {
        this.error.set(e.status === 401 ? 'Usuario o contraseña incorrectos.' : 'No se pudo conectar con el servidor.');
        this.loading.set(false);
      },
    });
  }
}
