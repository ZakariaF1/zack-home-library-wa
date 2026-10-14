import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { Book } from './book';

export interface ImportResponse {
  queuedCount: number;
}

@Injectable({ providedIn: 'root' })
export class LibraryService {
  private readonly http = inject(HttpClient);

  listBooks(): Observable<Book[]> {
    return this.http.get<unknown>('/api/books').pipe(
      map((body) => {
        if (!Array.isArray(body)) {
          throw new Error('Books response was not an array');
        }
        return body.map((row) => this.normalizeBook(row));
      }),
    );
  }

  importCsv(file: File): Observable<ImportResponse> {
    const form = new FormData();
    form.append('file', file, file.name);

    return this.http
      .post<ImportResponse>('/api/imports', form, { observe: 'response' })
      .pipe(
        map((response) => {
          const body = response.body;
          if (body && typeof body.queuedCount === 'number') {
            return body;
          }
          return { queuedCount: 0 };
        }),
        catchError((err: unknown) => {
          // Some proxies/clients surface 202 Accepted as an HttpErrorResponse.
          if (err instanceof HttpErrorResponse && err.status === 202) {
            const queuedCount = this.readQueuedCount(err.error);
            if (queuedCount != null) {
              return of({ queuedCount });
            }
          }
          return throwError(() => err);
        }),
      );
  }

  private normalizeBook(row: unknown): Book {
    const r = row as Record<string, unknown>;
    return {
      id: Number(r['id'] ?? r['Id'] ?? 0),
      name: String(r['name'] ?? r['Name'] ?? ''),
      author: String(r['author'] ?? r['Author'] ?? ''),
      genre: String(r['genre'] ?? r['Genre'] ?? ''),
      importDate: String(r['importDate'] ?? r['ImportDate'] ?? ''),
    };
  }

  private readQueuedCount(errorBody: unknown): number | null {
    if (typeof errorBody === 'object' && errorBody != null && 'queuedCount' in errorBody) {
      const value = (errorBody as { queuedCount: unknown }).queuedCount;
      return typeof value === 'number' ? value : null;
    }
    return null;
  }
}
