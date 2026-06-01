import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Teacher } from '../models/teacher.model';
import { TeacherService } from '../services/teacher.service';

type SortCol = 'lastName' | 'firstName' | 'nickName' | 'abbreviation';

@Component({
  selector: 'app-teacher-list',
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
        <h2>Teachers</h2>
        <a routerLink="/teachers/new" class="btn btn-primary">+ New Teacher</a>
      </div>
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && sortedTeachers().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'lastName'" (click)="sort('lastName')">
                Last Name <span class="sort-icon">{{ sortIcon('lastName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'firstName'" (click)="sort('firstName')">
                First Name <span class="sort-icon">{{ sortIcon('firstName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'nickName'" (click)="sort('nickName')">
                Nick Name <span class="sort-icon">{{ sortIcon('nickName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'abbreviation'" (click)="sort('abbreviation')">
                Abbreviation <span class="sort-icon">{{ sortIcon('abbreviation') }}</span>
              </th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (t of sortedTeachers(); track t.id) {
              <tr>
                <td>{{ t.lastName }}</td>
                <td>{{ t.firstName }}</td>
                <td>{{ t.nickName }}</td>
                <td>{{ t.abbreviation }}</td>
                <td>
                  <a [routerLink]="['/teachers', t.id]" class="btn btn-sm">Edit</a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (!loading() && teachers().length === 0) {
        <p class="empty">No teachers found.</p>
      }
    </div>
  `
})
export class TeacherListComponent implements OnInit {
  teachers = signal<Teacher[]>([]);
  loading = signal(false);
  sortCol = signal<SortCol>('lastName');
  sortAsc = signal(true);

  sortedTeachers = computed(() => {
    const col = this.sortCol();
    const asc = this.sortAsc();
    return this.teachers().slice().sort((a, b) => {
      const va = (a[col] ?? '') as string;
      const vb = (b[col] ?? '') as string;
      const cmp = va.localeCompare(vb, undefined, { sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  constructor(private service: TeacherService) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: data => { this.teachers.set(data); this.loading.set(false); },
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
