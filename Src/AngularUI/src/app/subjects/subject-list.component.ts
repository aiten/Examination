import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Subject } from '../models/subject.model';
import { SubjectService } from '../services/subject.service';

type SortCol = 'name';

@Component({
  selector: 'app-subject-list',
  standalone: true,
  imports: [RouterModule],
  styles: [`
    th.sortable { cursor: pointer; user-select: none; white-space: nowrap; }
    th.sortable:hover { background: #e2e8f0; }
    .sort-icon { margin-left: 4px; font-size: .8em; opacity: .5; }
    th.sort-active .sort-icon { opacity: 1; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Subjects</h2>
        <a routerLink="/subjects/new" class="btn btn-primary">+ New Subject</a>
      </div>
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && sorted().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'name'" (click)="sort('name')">
                Name <span class="sort-icon">{{ sortIcon('name') }}</span>
              </th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (s of sorted(); track s.id) {
              <tr>
                <td>{{ s.name }}</td>
                <td>
                  <a [routerLink]="['/subjects', s.id]" class="btn btn-sm">Edit</a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (!loading() && subjects().length === 0) {
        <p class="empty">No subjects found.</p>
      }
    </div>
  `
})
export class SubjectListComponent implements OnInit {
  subjects = signal<Subject[]>([]);
  loading = signal(false);
  sortCol = signal<SortCol>('name');
  sortAsc = signal(true);

  sorted = computed(() => {
    const asc = this.sortAsc();
    return this.subjects().slice().sort((a, b) => {
      const cmp = a.name.localeCompare(b.name, undefined, { sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  constructor(private service: SubjectService) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: data => { this.subjects.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  sort(col: SortCol): void {
    if (this.sortCol() === col) {
      this.sortAsc.update(v => !v);
    } else {
      this.sortCol.set(col);
      this.sortAsc.set(true);
    }
  }

  sortIcon(col: SortCol): string {
    if (this.sortCol() !== col) return '↕';
    return this.sortAsc() ? '▲' : '▼';
  }
}
