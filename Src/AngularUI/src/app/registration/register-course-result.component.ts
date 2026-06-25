import { Component, computed, effect, ElementRef, ViewChild } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RegistrationCourseService } from '../services/registration-course.service';
import QRCode from 'qrcode';

@Component({
  selector: 'app-register-course-result',
  standalone: true,
  imports: [RouterModule],
  template: `
    @if (result()) {
      <div class="page">
        <h2>Registration Successful</h2>
        <div class="form">
          <div class="form-group">
            <label>Student</label>
            <p>{{ result()!.lastName }}, {{ result()!.firstName }}</p>
          </div>
          <div class="form-group">
            <label>Course</label>
            <p>{{ result()!.courseDescription }}</p>
          </div>
          <div class="form-group">
            <label>Registration Code</label>
            <p class="code-box">{{ result()!.registrationCode }}</p>
          </div>
          <div class="form-group">
            <label>Result QR Code</label>
            <canvas #qrCanvas></canvas>
          </div>
          <div class="form-actions">
            <a [routerLink]="['/result']" [queryParams]="resultQueryParams()" class="btn btn-primary">View My Result</a>
          </div>
        </div>
      </div>
    } @else {
      <div class="page">
        <p class="empty">No registration result. <a routerLink="/registration/course">Go back</a>.</p>
      </div>
    }
  `
})
export class RegisterCourseResultComponent {
  @ViewChild('qrCanvas') qrCanvas!: ElementRef<HTMLCanvasElement>;

  result;
  resultQueryParams;
  resultUrl;

  constructor(private service: RegistrationCourseService) {
    this.result = service.result;
    this.resultQueryParams = computed(() => {
      const r = this.result();
      if (!r) return {};
      return { firstName: r.firstName, lastName: r.lastName, pin: r.pin, registrationCode: r.registrationCode };
    });
    this.resultUrl = computed(() => {
      const r = this.result();
      if (!r) return '';
      const params = new URLSearchParams({
        firstName: r.firstName,
        lastName: r.lastName,
        pin: String(r.pin),
        registrationCode: r.registrationCode
      });
      return `${window.location.origin}/result?${params}`;
    });

    effect(() => {
      const url = this.resultUrl();
      if (!url) return;
      setTimeout(() => {
        if (this.qrCanvas?.nativeElement) {
          QRCode.toCanvas(this.qrCanvas.nativeElement, url, { width: 200 });
        }
      });
    });
  }
}
