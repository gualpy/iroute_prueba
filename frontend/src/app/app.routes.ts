import { Routes } from '@angular/router';
import { Upload } from './pages/upload/upload';
import { Process } from './pages/process/process';
import { Errors } from './pages/errors/errors';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'upload' },
  { path: 'upload', component: Upload },
  { path: 'process', component: Process },
  { path: 'errors', component: Errors },
];
