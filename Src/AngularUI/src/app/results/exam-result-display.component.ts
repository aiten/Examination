import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ExamResultService } from '../services/exam-result.service';
import { ExamResultDetailComponent } from './exam-result-detail.component';

@Component({
  selector: 'app-result-exam-display',
  standalone: true,
  imports: [RouterModule, ExamResultDetailComponent],
  template: `
    @if (result()) {
      <div class="page">
        <div class="page-header">
          <h2>Exam Result: {{ result()!.examDescription }} &nbsp;({{ result()!.examDate }}) {{ result()!.studentName }}</h2>
          <a routerLink="/result/exam" class="btn">New Query</a>
        </div>
        <app-exam-result-detail [exam]="result()!" />
      </div>
    } @else {
      <div class="page">
        <p class="empty">No result loaded. <a routerLink="/result/exam">Go back</a>.</p>
      </div>
    }
  `
})
export class ExamResultDisplayComponent {
  result;

  constructor(private service: ExamResultService) {
    this.result = service.result;
  }
}
