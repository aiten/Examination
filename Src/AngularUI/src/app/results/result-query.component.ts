import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ResultService } from '../services/result.service';
import { StudentExamResultQuery } from '../models/exam-result.model';

@Component({
  selector: 'app-result-query',
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
          <input type="number" name="pin" [(ngModel)]="pin" required min="10000" max="99999" class="form-control" />
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
export class ResultQueryComponent {
  firstName = '';
  lastName = '';
  pin: number | null = null;
  registrationCode = '';
  loading = signal(false);
  error = signal('');

  constructor(private service: ResultService, private router: Router) {}

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    const query: StudentExamResultQuery = {
      firstName: this.firstName,
      lastName: this.lastName,
      pin: this.pin!,
      registrationCode: this.registrationCode
    };
    this.service.getResult(query).subscribe({
      next: result => {
        this.service.result.set(result);
        this.router.navigate(['/result/display']);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? 'Could not retrieve result.');
      }
    });
  }
}
