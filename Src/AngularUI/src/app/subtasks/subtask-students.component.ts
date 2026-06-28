import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { SubtaskStudent, SubtaskStudentCreate } from '../models/subtask-student.model';
import { SubtaskStudentService } from '../services/subtask-student.service';
import { SubtaskService } from '../services/subtask.service';
import { StudentExamService } from '../services/student-exam.service';
import { ExamService } from '../services/exam.service';

interface StudentRow {
  studentExamId: number;
  lastName: string;
  firstName: string;
  id: number;
  result: number | null;
  comment: string | null;
  commentPrivate: string | null;
  date: string | null;
}

@Component({
  selector: 'app-subtask-students',
  standalone: true,
  imports: [FormsModule, RouterModule],
  styles: [`
    .input-narrow { width: 80px; padding: 4px 6px; border: 1px solid #ccc; border-radius: 4px; font-size: .95rem; }
    .input-wide { width: 100%; padding: 4px 6px; border: 1px solid #ccc; border-radius: 4px; font-size: .95rem; box-sizing: border-box; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>{{ title() }}</h2>
        <a [routerLink]="['/exams', examId, 'subtasks']" class="btn">Back</a>
      </div>

      @if (loading()) {
        <p class="empty">Loading...</p>
      }

      @if (!loading() && rows().length > 0) {
        <form (ngSubmit)="save()">
          <table class="table">
            <thead>
              <tr>
                <th>Last Name</th>
                <th>First Name</th>
                @if (isParticipation()) {
                  <th style="width:140px">Date</th>
                }
                <th style="width:100px">Result (%)</th>
                <th>Comment</th>
                <th>Private Comment</th>
              </tr>
            </thead>
            <tbody>
              @for (row of rows(); track row.studentExamId) {
                <tr>
                  <td>{{ row.lastName }}</td>
                  <td>{{ row.firstName }}</td>
                  @if (isParticipation()) {
                    <td>
                      <input type="date" [(ngModel)]="row.date" [name]="'date_' + row.studentExamId" class="input-narrow" />
                    </td>
                  }
                  <td>
                    <input type="number" [(ngModel)]="row.result" [name]="'result_' + row.studentExamId"
                           min="0" max="100" step="1" class="input-narrow" />
                  </td>
                  <td>
                    <input type="text" [(ngModel)]="row.comment" [name]="'comment_' + row.studentExamId" class="input-wide" />
                  </td>
                  <td>
                    <input type="text" [(ngModel)]="row.commentPrivate" [name]="'commentPrivate_' + row.studentExamId" class="input-wide" />
                  </td>
                </tr>
              }
            </tbody>
          </table>

          <div class="form-actions">
            <button type="submit" class="btn btn-primary">Save</button>
          </div>
        </form>
      }

      @if (!loading() && rows().length === 0) {
        <p class="empty">No students registered for this exam.</p>
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
export class SubtaskStudentsComponent implements OnInit {
  examId = 0;
  private subtaskId = 0;

  title = signal('');
  rows = signal<StudentRow[]>([]);
  isParticipation = signal(false);
  loading = signal(false);
  error = signal('');
  successMessage = signal('');

  constructor(
    private subtaskStudentService: SubtaskStudentService,
    private subtaskService: SubtaskService,
    private studentExamService: StudentExamService,
    private examService: ExamService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.examId = +this.route.snapshot.paramMap.get('examId')!;
    this.subtaskId = +this.route.snapshot.paramMap.get('subtaskId')!;
    this.load();
  }

  private load(): void {
    this.loading.set(true);

    forkJoin([
      this.studentExamService.getAll(this.examId),
      this.subtaskStudentService.getAll(this.examId, this.subtaskId),
      this.subtaskService.getAll(this.examId),
      this.examService.getById(this.examId)
    ]).subscribe({
      next: ([students, results, subtasks, exam]) => {
        const subtask = subtasks.find(s => s.id === this.subtaskId);
        this.title.set(`${subtask?.description ?? 'Subtask'} — ${exam.description}`);
        this.isParticipation.set(exam.examType === 1);

        const resultMap = new Map(results.map(r => [r.studentExamId, r]));
        const rows = students
          .slice()
          .sort((a, b) => a.lastName.localeCompare(b.lastName) || a.firstName.localeCompare(b.firstName))
          .map(s => {
            const existing = resultMap.get(s.id);
            return {
              studentExamId: s.id,
              lastName: s.lastName,
              firstName: s.firstName,
              id: existing?.id ?? 0,
              result: existing?.result ?? null,
              comment: existing?.comment ?? null,
              commentPrivate: existing?.commentPrivate ?? null,
              date: existing?.date ?? null
            } satisfies StudentRow;
          });

        this.rows.set(rows);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load data.');
        this.loading.set(false);
      }
    });
  }

  save(): void {
    this.error.set('');
    this.successMessage.set('');

    const ops: Observable<SubtaskStudent | void>[] = [];

    for (const row of this.rows()) {
      if (row.id) {
        ops.push(this.subtaskStudentService.update(this.examId, this.subtaskId, row as SubtaskStudent));
      } else if (row.result !== null || row.comment || row.commentPrivate || row.date) {
        const dto: SubtaskStudentCreate = {
          studentExamId: row.studentExamId,
          result: row.result,
          comment: row.comment,
          commentPrivate: row.commentPrivate,
          date: row.date
        };
        ops.push(this.subtaskStudentService.create(this.examId, this.subtaskId, dto));
      }
    }

    if (ops.length === 0) {
      this.successMessage.set('No changes to save.');
      return;
    }

    forkJoin(ops).subscribe({
      next: () => {
        this.successMessage.set('Saved successfully.');
        this.load();
      },
      error: err => this.error.set(err.error?.detail ?? 'Save failed.')
    });
  }
}
