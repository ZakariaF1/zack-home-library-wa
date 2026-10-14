import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Book } from './book';
import { LibraryService } from './library.service';

@Component({
  selector: 'app-root',
  imports: [DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit, OnDestroy {
  private readonly library = inject(LibraryService);
  private pollHandle: ReturnType<typeof setInterval> | null = null;
  private pollTicks = 0;

  readonly books = signal<Book[]>([]);
  readonly loading = signal(true);
  readonly uploading = signal(false);
  readonly dragging = signal(false);
  readonly error = signal<string | null>(null);
  readonly status = signal<string | null>(null);

  ngOnInit(): void {
    this.refreshBooks();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  onBrowse(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.upload(file);
    }
    input.value = '';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (!file) {
      return;
    }
    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.error.set('Please drop a .csv file.');
      return;
    }
    this.upload(file);
  }

  private upload(file: File): void {
    this.error.set(null);
    this.status.set(null);
    this.uploading.set(true);

    this.library.importCsv(file).subscribe({
      next: (result) => {
        this.uploading.set(false);
        this.status.set(`Queued ${result.queuedCount} books. Refreshing the shelf…`);
        this.startPolling();
      },
      error: (err) => {
        this.uploading.set(false);
        this.error.set(this.readError(err) ?? 'Import failed. Try again.');
      },
    });
  }

  private refreshBooks(): void {
    this.library.listBooks().subscribe({
      next: (books) => {
        this.books.set(books);
        this.loading.set(false);
        this.error.set(null);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Could not load books. Is the API running?');
      },
    });
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollTicks = 0;
    this.refreshBooks();
    this.pollHandle = setInterval(() => {
      this.pollTicks += 1;
      this.refreshBooks();
      if (this.pollTicks >= 15) {
        this.stopPolling();
        this.status.set('Import finished polling. Newer rows appear when the worker catches up.');
      }
    }, 2000);
  }

  private stopPolling(): void {
    if (this.pollHandle) {
      clearInterval(this.pollHandle);
      this.pollHandle = null;
    }
  }

  private readError(err: unknown): string | null {
    if (
      typeof err === 'object' &&
      err !== null &&
      'error' in err &&
      typeof (err as { error: unknown }).error === 'object' &&
      (err as { error: { error?: string } }).error !== null
    ) {
      return (err as { error: { error?: string } }).error.error ?? null;
    }
    return null;
  }
}
