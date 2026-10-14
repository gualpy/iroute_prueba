import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface CommerceQuarantine {
  id: number;
  pcNumdoc: string | null;
  pcTipodoc: string | null;
  pcNomcomred: string | null;
  pcRazonsocial: string | null;
  pcDireccion: string | null;
  pcTelefono: string | null;
  pcEmail: string | null;
  pcProcessdate: string;
  motivo: string;
}

@Injectable({ providedIn: 'root' })
export class CommerceService {
  private readonly http = inject(HttpClient);
  private readonly base = 'http://localhost:5009/api/commerce';

  upload(file: File): Observable<{ inserted: number }> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<{ inserted: number }>(`${this.base}/upload`, form);
  }

  process(processDate: string): Observable<{ quarantined: number }> {
    return this.http.post<{ quarantined: number }>(`${this.base}/process`, { processDate });
  }

  quarantine(): Observable<CommerceQuarantine[]> {
    return this.http.get<CommerceQuarantine[]>(`${this.base}/quarantine`);
  }
}
