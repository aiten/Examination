import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RegistrationService } from '../services/registration.service';
import { ExamRegistrationRequest } from '../models/registration.model';

@Component({
  selector: 'app-register-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">
      <h2>Register for Exam</h2>
      <form (ngSubmit)="register()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Last Name *</label>
          <input name="lastName" [(ngModel)]="lastName" required class="form-control" />
        </div>
        <div class="form-group">
          <label>First Name *</label>
          <input name="firstName" [(ngModel)]="firstName" required class="form-control" />
        </div>
        <div class="form-group">
          <label>Login Name</label>
          <input name="loginName" [(ngModel)]="loginName" maxlength="32" class="form-control" />
          <small class="form-hint">The user-name used when logging into the school computer, e.g. Test9F01.</small>
        </div>
        <div class="form-group">
          <label>PIN * (5 digits)</label>
          <input type="text" name="pin" [(ngModel)]="pin" required pattern="[0-9]{5}" maxlength="5" class="form-control" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid || loading()">
            {{ loading() ? 'Registering…' : 'Register' }}
          </button>
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </div>
  `
})
export class RegisterFormComponent {
  firstName = '';
  lastName = '';
  loginName = '';
  pin = '';
  loading = signal(false);
  error = signal('');

  constructor(private service: RegistrationService, private router: Router) {}

  register(): void {
    this.loading.set(true);
    this.error.set('');
    const req: ExamRegistrationRequest = {
      firstName: this.firstName,
      lastName: this.lastName,
      loginName: this.loginName || null,
      pin: this.pin
    };
    this.service.register(req).subscribe({
      next: result => {
        this.service.result.set(result);
        this.router.navigate(['/registration/result']);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? 'Registration failed.');
      }
    });
  }
}
