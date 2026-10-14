import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Book } from './book';

export interface ImportResponse {
  queuedCount: number;
}

@Injectable({ providedIn: 'root' })
export class LibraryService {
  private readonly http = inject(HttpClient);

  listBooks(): Observable<Book[]> {
    return this.http.get<Book[]>('/api/books');
  }

  importCsv(file: File): Observable<ImportResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<ImportResponse>('/api/imports', form);
  }
}
