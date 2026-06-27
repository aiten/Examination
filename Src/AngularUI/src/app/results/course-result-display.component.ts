import { Component, computed, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CourseResultService } from '../services/course-result.service';
import { StudentExamResult } from '../models/course-result.model';
import { ExamResultDetailComponent } from './exam-result-detail.component';

type SortColumn = 'examDescription' | 'examDate' | 'totalPoints' | 'percent' | 'grade';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-result-course-display',
  standalone: true,
  imports: [RouterModule, ExamResultDetailComponent],
  template: `
    @if (result()) {
      <div class="page">
        <div class="page-header">
          <h2>Course Result: {{ result()!.courseName }}</h2>
          <a routerLink="/result/course" class="btn">New Query</a>
        </div>

        <div class="form" style="max-width: 640px; margin-bottom: 24px;">
          <div class="form-group">
            <label>Student</label>
            <p>{{ result()!.studentName }}</p>
          </div>
        </div>

        @if (selectedExam()) {
          <button class="btn" style="margin-bottom: 16px;" (click)="selectedExam.set(null)">← Back to list</button>
          <app-exam-result-detail [exam]="selectedExam()!" />
        } @else {
          <table class="table">
            <thead>
              <tr>
                <th class="sortable" (click)="sort('examDescription')">Exam {{ sortIndicator('examDescription') }}</th>
                <th class="sortable" (click)="sort('examDate')">Date {{ sortIndicator('examDate') }}</th>
                <th class="sortable" (click)="sort('totalPoints')">Points {{ sortIndicator('totalPoints') }}</th>
                <th class="sortable" (click)="sort('percent')">% {{ sortIndicator('percent') }}</th>
                <th class="sortable" (click)="sort('grade')">Grade {{ sortIndicator('grade') }}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (exam of sortedExams(); track exam.examDescription) {
                <tr>
                  <td>{{ exam.examDescription }}</td>
                  <td>{{ exam.examDate }}</td>
                  <td>{{ exam.totalPoints != null ? exam.totalPoints : '—' }}</td>
                  <td>{{ exam.percent != null ? (exam.percent + ' %') : '—' }}</td>
                  <td>{{ exam.grade != null ? exam.grade : '—' }}</td>
                  <td><a class="link" (click)="selectedExam.set(exam)">Details</a></td>
                </tr>
              }
            </tbody>
          </table>
        }
      </div>
    } @else {
      <div class="page">
        <p class="empty">No result loaded. <a routerLink="/result/course">Go back</a>.</p>
      </div>
    }
  `,
  styles: [`
    .sortable { cursor: pointer; user-select: none; }
    .sortable:hover { background: #e4eaf0; }
    .link { cursor: pointer; color: #1a73e8; text-decoration: underline; }
  `]
})
export class CourseResultDisplayComponent {
  result;
  selectedExam = signal<StudentExamResult | null>(null);
  sortCol = signal<SortColumn>('examDate');
  sortDir = signal<SortDir>('asc');

  sortedExams = computed(() => {
    const r = this.result();
    if (!r) return [];
    return [...r.studentExams].sort((a, b) => this.compare(a, b));
  });

  constructor(private service: CourseResultService) {
    this.result = service.result;
  }

  sort(col: SortColumn): void {
    if (this.sortCol() === col) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortCol.set(col);
      this.sortDir.set('asc');
    }
  }

  sortIndicator(col: SortColumn): string {
    if (this.sortCol() !== col) return '';
    return this.sortDir() === 'asc' ? '▲' : '▼';
  }

  private compare(a: StudentExamResult, b: StudentExamResult): number {
    const dir = this.sortDir() === 'asc' ? 1 : -1;
    switch (this.sortCol()) {
      case 'examDescription': return dir * a.examDescription.localeCompare(b.examDescription);
      case 'examDate':        return dir * a.examDate.localeCompare(b.examDate);
      case 'totalPoints':     return dir * ((a.totalPoints ?? -1) - (b.totalPoints ?? -1));
      case 'percent':         return dir * ((a.percent ?? -1) - (b.percent ?? -1));
      case 'grade':           return dir * ((a.grade ?? 99) - (b.grade ?? 99));
    }
  }
}
