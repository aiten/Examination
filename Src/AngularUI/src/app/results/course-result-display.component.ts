import { Component, computed, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CourseResultService } from '../services/course-result.service';
import { StudentCourseResult } from '../models/course-result.model';

type SortColumn = 'seqNo' | 'description' | 'points' | 'percent' | 'comment';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-result-course-display',
  standalone: true,
  imports: [RouterModule],
  template: `
    @if (result()) {
      <div class="page">
        <div class="page-header">
          <h2>Course Result: {{ result()!.courseDescription }} &nbsp;({{ result()!.courseDescription }}) {{ result()!.studentName }}</h2>
          <a routerLink="/result/course" class="btn">New Query</a>
        </div>
    </div>
    } @else {
      <div class="page">
        <p class="empty">No result loaded. <a routerLink="/result/course">Go back</a>.</p>
      </div>
    }
  `,
  styles: [`
    .sortable { cursor: pointer; user-select: none; }
    .sortable:hover { background: #e4eaf0; }
    .bonus-row td { color: #1a73e8; }
  `]
})
export class CourseResultDisplayComponent {
  result;

  constructor(private service: CourseResultService) {
    this.result = service.result;
  }
}
