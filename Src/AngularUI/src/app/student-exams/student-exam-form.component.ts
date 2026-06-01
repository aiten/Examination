import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { StudentExamEdit } from '../models/student-exam.model';
import { StudentExamService } from '../services/student-exam.service';

@Component({
  selector: 'app-student-exam-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>{{ title() }}</h2>
        <a [routerLink]="['/exams', examId, 'students']" class="btn">Cancel</a>
      </div>

      @if (loading()) {
        <p class="empty">Loading...</p>
      }

      @if (!loading()) {
        <form (ngSubmit)="save()" #form="ngForm" class="form">
          <div class="form-group">
            <label>Login Name</label>
            <input name="loginName" [(ngModel)]="data().loginName" class="form-control" />
          </div>
          <div class="form-group">
            <label>Registration Code</label>
            <input name="registrationCode" [(ngModel)]="data().registrationCode" class="form-control" />
          </div>
          <div class="form-actions">
            <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Save</button>
            <a [routerLink]="['/exams', examId, 'students']" class="btn">Cancel</a>
          </div>
          @if (error()) {
            <p class="error">{{ error() }}</p>
          }
        </form>
      }
    </div>
  `
})
export class StudentExamFormComponent implements OnInit {
  examId = 0;
  studentExamId = 0;

  title = signal('');
  data = signal<StudentExamEdit>({ id: 0, loginName: '', registrationCode: '' });
  loading = signal(false);
  error = signal('');

  constructor(
    private service: StudentExamService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.examId = +this.route.snapshot.paramMap.get('examId')!;
    this.studentExamId = +this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.service.getById(this.examId, this.studentExamId).subscribe({
      next: detail => {
        this.title.set(`Edit — ${detail.lastName} ${detail.firstName}`);
        this.data.set({ id: detail.id, loginName: detail.loginName, registrationCode: detail.registrationCode });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load student exam.');
        this.loading.set(false);
      }
    });
  }

  save(): void {
    this.error.set('');
    this.service.updateStudentExam(this.examId, this.studentExamId, this.data()).subscribe({
      next: () => this.router.navigate(['/exams', this.examId, 'students']),
      error: err => this.error.set(err.error?.detail ?? 'Save failed.')
    });
  }
}
