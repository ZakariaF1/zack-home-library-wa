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
  private booksBeforeImport = 0;
  private awaitingWorker = false;

  readonly books = signal<Book[]>([]);
  readonly loading = signal(true);
  readonly uploading = signal(false);
  readonly dragging = signal(false);
  readonly error = signal<string | null>(null);
  readonly status = signal<string | null>(null);

  ngOnInit(): void {
    this.refreshBooks(false);
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
    this.booksBeforeImport = this.books().length;

    this.library.importCsv(file).subscribe({
      next: (result) => {
        this.uploading.set(false);
        this.awaitingWorker = true;
        this.status.set(
          `Queued ${result.queuedCount} books. Waiting for the worker to save them…`,
        );
        this.startPolling();
      },
      error: (err) => {
        this.uploading.set(false);
        this.awaitingWorker = false;
        this.status.set(null);
        this.error.set(this.readError(err) ?? this.readHttpMessage(err) ?? 'Import failed. Try again.');
      },
    });
  }

  private refreshBooks(isPoll: boolean): void {
    this.library.listBooks().subscribe({
      next: (books) => {
        this.books.set(books);
        this.loading.set(false);
        this.error.set(null);

        if (this.awaitingWorker && books.length > this.booksBeforeImport) {
          this.awaitingWorker = false;
          this.stopPolling();
          this.status.set(`Import complete — ${books.length} books on the shelf.`);
        }
      },
      error: () => {
        this.loading.set(false);
        // Don't paint a red error over a successful queue while we still expect the worker.
        if (this.awaitingWorker || isPoll) {
          this.status.set(
            'Queued books, but the list could not be refreshed yet. Is the API reachable at /api/books?',
          );
          return;
        }
        this.error.set('Could not load books. Is the API running on port 8080?');
      },
    });
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollTicks = 0;
    this.refreshBooks(true);
    this.pollHandle = setInterval(() => {
      this.pollTicks += 1;
      this.refreshBooks(true);
      if (this.pollTicks >= 20) {
        this.stopPolling();
        this.awaitingWorker = false;
        if (this.books().length <= this.booksBeforeImport) {
          this.error.set(
            'Books were queued, but none appeared. Make sure the Worker container/process is running.',
          );
          this.status.set(null);
        }
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

  private readHttpMessage(err: unknown): string | null {
    if (typeof err === 'object' && err !== null && 'message' in err) {
      const message = (err as { message?: unknown }).message;
      return typeof message === 'string' && message.length > 0 ? message : null;
    }
    return null;
  }
}

