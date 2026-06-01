import { Component, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ExamOverview } from '../models/exam-overview.model';
import { ExamService } from '../services/exam.service';

type SortCol = 'description' | 'teacher' | 'course' | 'date';

function likeMatch(value: string, pattern: string): boolean {
  if (!pattern) return true;
  const regex = new RegExp('^' + pattern.replace(/%/g, '.*').replace(/_/g, '.') + '$', 'i');
  return regex.test(value);
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
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Exams</h2>
        <a routerLink="/exams/new" class="btn btn-primary">+ New Exam</a>
      </div>
      <div class="filter-bar">
        <input class="form-control" placeholder="Teacher (e.g. stein or %stein)" [ngModel]="filterTeacher()" (ngModelChange)="filterTeacher.set($event)" />
        <input class="form-control" placeholder="Course (e.g. pos or %htl)" [ngModel]="filterCourse()" (ngModelChange)="filterCourse.set($event)" />
      </div>
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && filteredExams().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'description'" (click)="sort('description')">
                Description <span class="sort-icon">{{ sortIcon('description') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'teacher'" (click)="sort('teacher')">
                Teacher <span class="sort-icon">{{ sortIcon('teacher') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'course'" (click)="sort('course')">
                Course <span class="sort-icon">{{ sortIcon('course') }}</span>
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
            @for (e of filteredExams(); track e.id) {
              <tr>
                <td>{{ e.description }}</td>
                <td>{{ e.teacher }}</td>
                <td>{{ e.course }}</td>
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
      }
      @if (!loading() && exams().length === 0) {
        <p class="empty">No exams found.</p>
      }
    </div>
  `
})
export class ExamListComponent implements OnInit {
  exams = signal<ExamOverview[]>([]);
  loading = signal(false);

  filterTeacher = signal('');
  filterCourse = signal('');
  sortCol = signal<SortCol>('date');
  sortAsc = signal(false);

  filteredExams = computed(() => {
    const col = this.sortCol();
    const asc = this.sortAsc();

    const filtered = this.exams().filter(e => {
      const tf = this.filterTeacher();
      const cf = this.filterCourse();
      const tp = tf && !tf.endsWith('%') ? tf + '%' : tf;
      const cp = cf && !cf.endsWith('%') ? cf + '%' : cf;
      return likeMatch(e.teacher, tp) && likeMatch(e.course, cp);
    });

    return filtered.slice().sort((a, b) => {
      let cmp: number;
      switch (col) {
        case 'description': cmp = a.description.localeCompare(b.description, undefined, { sensitivity: 'base' }); break;
        case 'teacher':     cmp = a.teacher.localeCompare(b.teacher, undefined, { sensitivity: 'base' }); break;
        case 'course':      cmp = a.course.localeCompare(b.course, undefined, { sensitivity: 'base' }); break;
        case 'date':        cmp = a.date.localeCompare(b.date); break;
        default:            cmp = 0;
      }
      return asc ? cmp : -cmp;
    });
  });

  constructor(private service: ExamService) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.service.getOverview().subscribe({
      next: data => { this.exams.set(data); this.loading.set(false); },
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

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const [y, m, d] = dateStr.split('-');
    return `${d}.${m}.${y}`;
  }

  formatTime(timeStr: string): string {
    return timeStr ? timeStr.slice(0, 5) : '';
  }
}
