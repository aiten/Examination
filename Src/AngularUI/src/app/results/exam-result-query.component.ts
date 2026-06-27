import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ExamResultService } from '../services/exam-result.service';
import { StudentExamResultQuery } from '../models/exam-result.model';

@Component({
  selector: 'app-result-exam-query',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">
      <h2>Get Exam Result</h2>
      <form (ngSubmit)="submit()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Last Name *</label>
          <input name="lastName" [(ngModel)]="lastName" required class="form-control" />
        </div>
        <div class="form-group">
          <label>First Name *</label>
          <input name="firstName" [(ngModel)]="firstName" required class="form-control" />
        </div>
        <div class="form-group">
          <label>Exam PIN *</label>
          <input type="text" name="pin" [(ngModel)]="pin" required maxlength="5" pattern="[0-9]{1,5}" class="form-control" />
        </div>
        <div class="form-group">
          <label>Registration Code *</label>
          <input name="registrationCode" [(ngModel)]="registrationCode" required minlength="5" class="form-control" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid || loading()">
            {{ loading() ? 'Loading…' : 'Show Result' }}
          </button>
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </div>
  `
})
export class ExamResultQueryComponent implements OnInit {
  firstName = '';
  lastName = '';
  pin = '';
  registrationCode = '';
  loading = signal(false);
  error = signal('');

  constructor(private service: ExamResultService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const p = this.route.snapshot.queryParamMap;
    if (p.has('firstName'))       this.firstName = p.get('firstName')!;
    if (p.has('lastName'))        this.lastName = p.get('lastName')!;
    if (p.has('pin'))             this.pin = p.get('pin')!;
    if (p.has('registrationCode')) this.registrationCode = p.get('registrationCode')!;
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    const query: StudentExamResultQuery = {
      firstName: this.firstName,
      lastName: this.lastName,
      pin: this.pin,
      registrationCode: this.registrationCode
    };
    this.service.getResult(query).subscribe({
      next: result => {
        this.service.result.set(result);
        this.router.navigate(['/result/exam/display']);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? 'Could not retrieve result.');
      }
    });
  }
}
