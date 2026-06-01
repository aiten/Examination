import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RegistrationService } from '../services/registration.service';

@Component({
  selector: 'app-register-result',
  standalone: true,
  imports: [RouterModule],
  template: `
    @if (result()) {
      <div class="page">
        <h2>Registration Successful</h2>
        <div class="form">
          <div class="form-group">
            <label>Student</label>
            <p>{{ result()!.studentName }}</p>
          </div>
          <div class="form-group">
            <label>Exam</label>
            <p>{{ result()!.examDescription }}</p>
          </div>
          <div class="form-group">
            <label>Date</label>
            <p>{{ result()!.examDate }}</p>
          </div>
          <div class="form-group">
            <label>Registration Code</label>
            <p class="code-box">{{ result()!.registrationCode }}</p>
          </div>
          <div class="form-actions">
            <a routerLink="/registration" class="btn btn-primary">Register again</a>
          </div>
        </div>
      </div>
    } @else {
      <div class="page">
        <p class="empty">No registration result. <a routerLink="/registration">Go back</a>.</p>
      </div>
    }
  `
})
export class RegisterResultComponent {
  result;

  constructor(private service: RegistrationService) {
    this.result = service.result;
  }
}
