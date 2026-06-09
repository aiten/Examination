import { Component, computed, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ResultService } from '../services/result.service';
import { StudentExamResultSubtask } from '../models/exam-result.model';

type SortColumn = 'seqNo' | 'description' | 'points' | 'percent' | 'comment';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-result-display',
  standalone: true,
  imports: [RouterModule],
  template: `
    @if (result()) {
      <div class="page">
        <div class="page-header">
          <h2>Exam Result: {{ result()!.examDescription }} &nbsp;({{ result()!.examDate }}) {{ result()!.studentName }}</h2>
          <a routerLink="/result" class="btn">New Query</a>
        </div>

        <table class="table" style="margin-bottom: 24px;">
          <thead>
            <tr>
              <th class="sortable" (click)="sort('seqNo')">No {{ sortIndicator('seqNo') }}</th>
              <th class="sortable" (click)="sort('description')">Task {{ sortIndicator('description') }}</th>
              <th class="sortable" (click)="sort('points')">Points {{ sortIndicator('points') }}</th>
              <th class="sortable" (click)="sort('percent')">Reached % {{ sortIndicator('percent') }}</th>
              <th class="sortable" (click)="sort('comment')">Comment {{ sortIndicator('comment') }}</th>
            </tr>
          </thead>
          <tbody>
            @for (row of sortedSubtasks(); track row.seqNo) {
              <tr [class.bonus-row]="row.bonus">
                <td>{{ row.seqNo }}{{ row.bonus ? ' ★' : '' }}</td>
                <td>{{ row.description }}</td>
                <td>{{ row.points }}</td>
                <td>{{ reachedPercent(row) }}</td>
                <td>{{ row.comment ?? '' }}</td>
              </tr>
            }
          </tbody>
        </table>

        <div class="form" style="max-width: 640px;">
          <h3 style="margin-bottom: 16px;">Summary</h3>
          <div class="form-group">
            <label>Total Points</label>
            <p>{{ result()!.totalPoints != null ? result()!.totalPoints : '—' }}</p>
          </div>
          <div class="form-group">
            <label>Percentage</label>
            <p>{{ result()!.percent != null ? (result()!.percent + ' %') : '—' }}</p>
          </div>
          <div class="form-group">
            <label>Grade</label>
            <p>{{ result()!.grade != null ? result()!.grade : '—' }}</p>
          </div>
        </div>
      </div>
    } @else {
      <div class="page">
        <p class="empty">No result loaded. <a routerLink="/result">Go back</a>.</p>
      </div>
    }
  `,
  styles: [`
    .sortable { cursor: pointer; user-select: none; }
    .sortable:hover { background: #e4eaf0; }
    .bonus-row td { color: #1a73e8; }
  `]
})
export class ResultDisplayComponent {
  result;
  sortCol = signal<SortColumn>('seqNo');
  sortDir = signal<SortDir>('asc');

  sortedSubtasks = computed(() => {
    const r = this.result();
    if (!r) return [];
    return [...r.subtasks].sort((a, b) => this.compare(a, b));
  });

  constructor(private service: ResultService) {
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

  reachedPercent(row: StudentExamResultSubtask): string {
    if (row.result == null) return '—';
    if (row.points === 0) return '—';
    return Math.round((row.result * 1000) / 10) + ' %';
  }

  private compare(a: StudentExamResultSubtask, b: StudentExamResultSubtask): number {
    const dir = this.sortDir() === 'asc' ? 1 : -1;
    switch (this.sortCol()) {
      case 'seqNo':      return dir * (a.seqNo - b.seqNo);
      case 'description': return dir * a.description.localeCompare(b.description);
      case 'points':     return dir * (a.points - b.points);
      case 'percent': {
        const pa = a.points > 0 && a.result != null ? a.result / a.points : -1;
        const pb = b.points > 0 && b.result != null ? b.result / b.points : -1;
        return dir * (pa - pb);
      }
      case 'comment':
        return dir * (a.comment ?? '').localeCompare(b.comment ?? '');
    }
  }
}
