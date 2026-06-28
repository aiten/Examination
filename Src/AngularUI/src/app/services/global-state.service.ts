import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class GlobalStateService {
  lastPin    = '';
  lastCode   = '';
  filterYear = signal<number | null>(null);
}
