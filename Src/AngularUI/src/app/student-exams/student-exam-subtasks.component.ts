import { Component, OnInit, computed, signal, ChangeDetectionStrategy } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { StudentSubtask } from '../models/student-subtask.model';
import { StudentSubtaskService } from '../services/student-subtask.service';
import { StudentExamService } from '../services/student-exam.service';
import { ExamService } from '../services/exam.service';

@Component({
  selector: 'app-student-exam-subtasks',
  standalone: true,
  imports: [RouterModule, FormsModule, DecimalPipe],
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `
    <div class="page">
      <div class="page-header">
        <h2>{{ title() }}</h2>
        <a [routerLink]="['/exams', examId, 'students']" class="btn">Back</a>
      </div>

      @if (loading()) {
        <p class="empty">Loading...</p>
      }

      @if (!loading() && subtasks().length > 0) {
        <form (ngSubmit)="save()">
          <table class="table">
            <thead>
              <tr>
                <th>Description</th>
                <th>Max Points</th>
                @if (examType() === 1) {
                  <th style="width:140px">Date</th>
                }
                <th>Result (%)</th>
                <th>Comment</th>
                <th>Private Comment</th>
              </tr>
            </thead>
            <tbody>
              @for (s of subtasks(); track s.id) {
                <tr>
                  <td>{{ s.description }}</td>
                  <td>{{ s.bonus ? '(' + s.points + ')' : s.points }}</td>
                  @if (examType() === 1) {
                    <td>
                      <input type="date" [(ngModel)]="s.date" [name]="'date_' + s.id" class="input-date" />
                    </td>
                  }
                  <td>
                    <input type="number" [(ngModel)]="s.result" [name]="'result_' + s.id"
                           (ngModelChange)="refresh()"
                           min="0" max="100" step="1" class="input-narrow" />
                  </td>
                  <td>
                    <input type="text" [(ngModel)]="s.comment" maxlength="2000" [name]="'comment_' + s.id" class="input-wide" />
                  </td>
                  <td>
                    <input type="text" [(ngModel)]="s.commentPrivate" maxlength="2000" [name]="'commentPrivate_' + s.id" class="input-wide" />
                  </td>
                </tr>
              }
            </tbody>
            <tfoot>
              <tr class="summary-row">
                <td><strong>Total</strong></td>
                <td><strong>{{ totalMaxPoints() }}</strong></td>
                @if (examType() === 1) {
                  <td></td>
                }
                <td><strong>{{ totalPercentage() | number:'1.2-2' }} %</strong></td>
                <td></td>
                <td></td>
              </tr>
            </tfoot>
          </table>

          <div class="form-actions">
            <button type="submit" class="btn btn-primary">Save</button>
          </div>
        </form>
      }

      @if (!loading() && subtasks().length > 0) {
        <details class="import-box">
          <summary>Import evaluation (JSON)</summary>
          <p>
            Paste the evaluation as JSON array, e.g.
            <code>[{{ '{' }}"name": "Description", "percent": 75, "reason": "Reason"{{ '}' }}]</code>.
            Subtasks are matched by description, <code>percent</code> is 0-100, other properties (e.g. points, max) are ignored.
            Existing results and comments of the matched subtasks are overwritten.
          </p>
          <textarea [(ngModel)]="importText" name="importText" rows="10" class="input-wide" style="width:100%"></textarea>
          <div class="form-actions">
            <button type="button" class="btn btn-primary" (click)="importResults()" [disabled]="!importText.trim() || importing()">Import</button>
          </div>
        </details>
      }

      @if (successMessage()) {
        <p class="success">{{ successMessage() }}</p>
      }

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
    </div>
  `
})
export class StudentExamSubtasksComponent implements OnInit {
  examId = 0;
  private studentExamId = 0;

  title = signal('');
  subtasks = signal<StudentSubtask[]>([]);
  examType = signal(0);
  loading = signal(false);
  importing = signal(false);
  importText = '';
  error = signal('');
  successMessage = signal('');

  totalMaxPoints = computed(() => {
    const isParticipation = this.examType() === 1;
    return this.subtasks().reduce((sum, s) => {
      if (s.bonus) return sum;
      if (isParticipation && s.result === null) return sum;
      return sum + s.points;
    }, 0);
  });
  totalPercentage = computed(() => {
    const max = this.totalMaxPoints();
    if (max === 0) return 0;
    return this.subtasks().reduce((sum, s) => sum + s.points * (s.result ?? 0), 0) / max;
  });

  constructor(
    private subtaskService: StudentSubtaskService,
    private studentExamService: StudentExamService,
    private examService: ExamService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.examId = +this.route.snapshot.paramMap.get('examId')!;
    this.studentExamId = +this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    forkJoin({
      exam: this.examService.getById(this.examId),
      detail: this.studentExamService.getById(this.examId, this.studentExamId),
      subtasks: this.subtaskService.getAll(this.examId, this.studentExamId)
    }).subscribe({
      next: ({ exam, detail, subtasks }) => {
        this.examType.set(exam.examType);
        this.title.set(`${detail.lastName}, ${detail.firstName} — Subtask Results`);
        this.subtasks.set(subtasks.slice().sort((a, b) => a.seqNo - b.seqNo));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load subtasks.');
        this.loading.set(false);
      }
    });
  }

  refresh(): void {
    this.subtasks.update(s => [...s]);
  }

  importResults(): void {
    this.error.set('');
    this.successMessage.set('');
    this.importing.set(true);
    this.subtaskService.import(this.examId, this.studentExamId, this.importText).subscribe({
      next: result => {
        this.importing.set(false);
        this.importText = '';
        this.load();
        this.successMessage.set(`Imported ${result.imported} subtask(s).`);
        if (result.notFound.length > 0) {
          this.error.set(`No matching subtask for: ${result.notFound.join('; ')}`);
        }
      },
      error: err => {
        this.importing.set(false);
        this.error.set(err.error?.detail ?? 'Import failed.');
      }
    });
  }

  save(): void {
    this.error.set('');
    this.successMessage.set('');
    const updates = this.subtasks().map(s => {
      const normalized = { ...s, comment: s.comment || null, commentPrivate: s.commentPrivate || null, date: s.date || null };
      return s.id ? this.subtaskService.update(this.examId, this.studentExamId, normalized) : this.subtaskService.create(this.examId, this.studentExamId, normalized);
    });
    forkJoin(updates).subscribe({
      next: () => {
        this.successMessage.set('Saved successfully.');
        this.load();
      },
      error: err => this.error.set(err.error?.detail ?? 'Save failed.')
    });
  }
}
