import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SchoolClass } from '../models/class.model';
import { ClassService } from '../services/class.service';
import { TeacherService } from '../services/teacher.service';

type SortCol = 'description' | 'year' | 'teacher';

@Component({
  selector: 'app-class-list',
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
        <h2>Classes</h2>
        <a routerLink="/classes/new" class="btn btn-primary">+ New Class</a>
      </div>
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && sortedClasses().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'description'" (click)="sort('description')">
                Description <span class="sort-icon">{{ sortIcon('description') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'year'" (click)="sort('year')">
                Year <span class="sort-icon">{{ sortIcon('year') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'teacher'" (click)="sort('teacher')">
                Teacher <span class="sort-icon">{{ sortIcon('teacher') }}</span>
              </th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (c of sortedClasses(); track c.id) {
              <tr>
                <td>{{ c.description }}</td>
                <td>{{ c.year }}</td>
                <td>{{ teacherName(c.teacherId) }}</td>
                <td>
                  <a [routerLink]="['/classes', c.id]" class="btn btn-sm">Edit</a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (!loading() && classes().length === 0) {
        <p class="empty">No classes found.</p>
      }
    </div>
  `
})
export class ClassListComponent implements OnInit {
  classes = signal<SchoolClass[]>([]);
  loading = signal(false);
  sortCol = signal<SortCol>('description');
  sortAsc = signal(true);

  private teacherMap = signal<Map<number, string>>(new Map());

  sortedClasses = computed(() => {
    const col = this.sortCol();
    const asc = this.sortAsc();
    const tm = this.teacherMap();
    return this.classes().slice().sort((a, b) => {
      let va: string, vb: string;
      switch (col) {
        case 'description': va = a.description; vb = b.description; break;
        case 'year':        va = String(a.year); vb = String(b.year); break;
        case 'teacher':     va = a.teacherId != null ? (tm.get(a.teacherId) ?? '') : '';
                            vb = b.teacherId != null ? (tm.get(b.teacherId) ?? '') : ''; break;
      }
      const cmp = va.localeCompare(vb, undefined, { sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  constructor(private service: ClassService, private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.teacherService.getAll().subscribe(ts => {
      this.teacherMap.set(new Map(ts.map(t => [t.id, t.lastName + (t.firstName ? ', ' + t.firstName : '')])));
    });
    this.service.getAll().subscribe({
      next: data => { this.classes.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  teacherName(id: number | null): string {
    if (id == null) return '';
    return this.teacherMap().get(id) ?? String(id);
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
