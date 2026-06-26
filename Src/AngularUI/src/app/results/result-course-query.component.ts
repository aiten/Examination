import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseResultService } from '../services/course-result.service';
import { StudentCourseResultQuery } from '../models/course-result.model';

@Component({
  selector: 'app-result-course-query',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">
      <h2>Get Course Result</h2>
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
          <label>Course PIN *</label>
          <input type="text" name="pin" [(ngModel)]="pin" required maxlength="5" pattern="[0-9]{1,5}" class="form-control" />
        </div>
        <div class="form-group">
          <label>Registration Name *</label>
          <input name="registrationName" [(ngModel)]="registrationName" required minlength="5" class="form-control" />
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
export class ResultCourseQueryComponent implements OnInit {
  firstName = '';
  lastName = '';
  pin = '';
  registrationName = '';
  loading = signal(false);
  error = signal('');

  constructor(private service: CourseResultService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    const p = this.route.snapshot.queryParamMap;
    if (p.has('firstName'))        this.firstName = p.get('firstName')!;
    if (p.has('lastName'))         this.lastName = p.get('lastName')!;
    if (p.has('pin'))              this.pin = p.get('pin')!;
    if (p.has('registrationName')) this.registrationName = p.get('registrationName')!;
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    const query: StudentCourseResultQuery = {
      firstName: this.firstName,
      lastName: this.lastName,
      pin: this.pin,
      registrationName: this.registrationName
    };
    this.service.getResult(query).subscribe({
      next: result => {
        this.service.result.set(result);
        this.router.navigate(['/result/course/display']);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(err.error?.detail ?? 'Could not retrieve result.');
      }
    });
  }
}
