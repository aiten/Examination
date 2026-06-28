import { Component, OnInit, WritableSignal, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ExamOverview } from '../models/exam-overview.model';
import { ExamService } from '../services/exam.service';
import { ConfigService } from '../services/config.service';
import { GlobalStateService } from '../services/global-state.service';

type SortCol = 'description' | 'teacher' | 'date';

interface CourseGroup {
  course: string;
  exams: ExamOverview[];
}

@Component({
  selector: 'app-exam-list',
  standalone: true,
  imports: [FormsModule, RouterModule],
  styles: [`
    th.sortable { cursor: pointer; user-select: none; white-space: nowrap; }
    th.sortable:hover { background: #e2e8f0; }
    .sort-icon { margin-left: 4px; font-size: .8em; opacity: .5; }
    th.sort-active .sort-icon { opacity: 1; }
    .course-section { margin-top: 2rem; }
    .course-section h3 { margin-bottom: .5rem; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Exams</h2>
        <a routerLink="/exams/new" class="btn btn-primary">+ New Exam</a>
      </div>
      <div class="filter-bar">
        <input type="number" class="form-control" placeholder="School year (e.g. 2025)"
               [ngModel]="filterYear()"
               (ngModelChange)="onYearChange($event)" />
      </div>
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && exams().length === 0) {
        <p class="empty">No exams found.</p>
      }
      @for (group of groupedExams(); track group.course) {
        <div class="course-section">
          <h3>{{ group.course }}</h3>
          <table class="table">
            <thead>
              <tr>
                <th class="sortable" [class.sort-active]="sortCol() === 'description'" (click)="sort('description')">
                  Description <span class="sort-icon">{{ sortIcon('description') }}</span>
                </th>
                <th class="sortable" [class.sort-active]="sortCol() === 'teacher'" (click)="sort('teacher')">
                  Teacher <span class="sort-icon">{{ sortIcon('teacher') }}</span>
                </th>
                <th class="sortable" [class.sort-active]="sortCol() === 'date'" (click)="sort('date')">
                  Date <span class="sort-icon">{{ sortIcon('date') }}</span>
                </th>
                <th>Time</th>
                <th>PIN</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (e of group.exams; track e.id) {
                <tr>
                  <td>{{ e.description }}</td>
                  <td>{{ e.teacher }}</td>
                  <td>{{ formatDate(e.date) }}</td>
                  <td>{{ formatTime(e.from) }} – {{ formatTime(e.to) }}</td>
                  <td>{{ e.pin ?? '' }}</td>
                  <td>
                    <a [routerLink]="['/exams', e.id, 'students']" class="btn btn-sm">Students</a>
                    <a [routerLink]="['/exams', e.id, 'subtasks']" class="btn btn-sm">Subtasks</a>
                    <a [routerLink]="['/exams', e.id]" class="btn btn-sm">Edit</a>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `
})
export class ExamListComponent implements OnInit {
  exams = signal<ExamOverview[]>([]);
  loading = signal(false);

  filterYear: WritableSignal<number | null>;
  sortCol = signal<SortCol>('date');
  sortAsc = signal(false);

  groupedExams = computed((): CourseGroup[] => {
    const col = this.sortCol();
    const asc = this.sortAsc();

    const sorted = this.exams().slice().sort((a, b) => {
      let cmp: number;
      switch (col) {
        case 'description': cmp = a.description.localeCompare(b.description, undefined, { sensitivity: 'base' }); break;
        case 'teacher':     cmp = a.teacher.localeCompare(b.teacher, undefined, { sensitivity: 'base' }); break;
        case 'date':        cmp = (a.date ?? '').localeCompare(b.date ?? ''); break;
        default:            cmp = 0;
      }
      return asc ? cmp : -cmp;
    });

    const map = new Map<string, ExamOverview[]>();
    for (const e of sorted) {
      const group = map.get(e.course) ?? [];
      group.push(e);
      map.set(e.course, group);
    }

    return Array.from(map.entries())
      .sort(([a], [b]) => a.localeCompare(b, undefined, { sensitivity: 'base' }))
      .map(([course, exams]) => ({ course, exams }));
  });

  constructor(private service: ExamService, private configService: ConfigService, private globalState: GlobalStateService) {
    this.filterYear = globalState.filterYear;
  }

  ngOnInit(): void {
    if (this.filterYear() !== null) {
      this.loadExams();
    } else {
      this.configService.getConfig().subscribe({
        next: cfg => {
          this.filterYear.set(cfg.currentSchoolYear);
          this.loadExams();
        },
        error: () => this.loadExams()
      });
    }
  }

  private loadExams(): void {
    this.loading.set(true);
    this.service.getOverview(this.filterYear()).subscribe({
      next: data => { this.exams.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  onYearChange(value: number | null): void {
    this.filterYear.set(value || null);
    this.loadExams();
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

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '';
    const [y, m, d] = dateStr.split('-');
    return `${d}.${m}.${y}`;
  }

  formatTime(timeStr: string | null): string {
    return timeStr ? timeStr.slice(0, 5) : '';
  }
}
