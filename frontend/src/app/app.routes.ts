import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { Login } from './pages/login/login';
import { Upload } from './pages/upload/upload';
import { Process } from './pages/process/process';
import { Errors } from './pages/errors/errors';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'upload' },
  { path: 'login', component: Login },
  { path: 'upload', component: Upload, canActivate: [authGuard] },
  { path: 'process', component: Process, canActivate: [authGuard] },
  { path: 'errors', component: Errors, canActivate: [authGuard] },
  { path: '**', redirectTo: 'upload' },
];
