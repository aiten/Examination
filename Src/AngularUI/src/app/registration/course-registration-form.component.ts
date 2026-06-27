import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CourseRegistrationService } from '../services/cource-registration.service';
import { RegistrationCourseRequest } from '../models/registration.model';

@Component({
  selector: 'app-register-course-form',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">
      <h2>Register for Course</h2>
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
export class CourseRegistrationFormComponent {
  firstName = '';
  lastName = '';
  pin = '';
  loading = signal(false);
  error = signal('');

  constructor(private service: CourseRegistrationService, private router: Router) {}

  register(): void {
    this.loading.set(true);
    this.error.set('');
    const req: RegistrationCourseRequest = {
      firstName: this.firstName,
      lastName: this.lastName,
      pin: this.pin
    };
    this.service.register(req).subscribe({
      next: result => {
        this.service.result.set(result);
        this.router.navigate(['/registration/course/result']);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? 'Registration failed.');
      }
    });
  }
}
