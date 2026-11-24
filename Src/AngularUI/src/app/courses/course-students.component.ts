import { Component, OnInit, computed, signal, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { StudentCourse } from '../models/student-course.model';
import { StudentCourseService } from '../services/student-course.service';
import { CourseService } from '../services/course.service';

type SortCol = 'lastName' | 'firstName' | 'registrationCode';

@Component({
  selector: 'app-course-students',
  standalone: true,
  imports: [RouterModule],
  styles: [`
    th.sortable { cursor: pointer; user-select: none; white-space: nowrap; }
    th.sortable:hover { background: #e2e8f0; }
    .sort-icon { margin-left: 4px; font-size: .8em; opacity: .5; }
    th.sort-active .sort-icon { opacity: 1; }
  `],
  changeDetection: ChangeDetectionStrategy.Eager,
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Assigned Students{{ courseName() ? ' — ' + courseName() : '' }}</h2>
        <label>
          <input type="checkbox" [checked]="showCodes()" (change)="showCodes.set($any($event.target).checked)" />
          Show registration codes
        </label>
        <a routerLink="/courses" class="btn">Back to Courses</a>
      </div>

      @if (loading()) {
        <p class="empty">Loading...</p>
      }

      @if (!loading() && sorted().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'lastName'" (click)="sort('lastName')">
                Last Name <span class="sort-icon">{{ sortIcon('lastName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'firstName'" (click)="sort('firstName')">
                First Name <span class="sort-icon">{{ sortIcon('firstName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'registrationCode'" (click)="sort('registrationCode')">
                Reg. Code <span class="sort-icon">{{ sortIcon('registrationCode') }}</span>
              </th>
            </tr>
          </thead>
          <tbody>
            @for (s of sorted(); track s.id) {
              <tr>
                <td>{{ s.lastName }}</td>
                <td>{{ s.firstName }}</td>
                <td>{{ showCodes() ? s.registrationCode : '' }}</td>
              </tr>
            }
          </tbody>
        </table>
        <p class="count">Assigned students: {{ students().length }}</p>
      }

      @if (!loading() && students().length === 0 && !error()) {
        <p class="empty">No students assigned to this course.</p>
      }

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
    </div>
  `
})
export class CourseStudentsComponent implements OnInit {
  courseId = 0;

  students = signal<StudentCourse[]>([]);
  courseName = signal('');
  loading = signal(false);
  error = signal('');
  sortCol = signal<SortCol>('lastName');
  sortAsc = signal(true);
  showCodes = signal(false);

  sorted = computed(() => {
    const col = this.sortCol();
    const dir = this.sortAsc() ? 1 : -1;
    const hideCodes = !this.showCodes();
    const value = (s: StudentCourse) => (col === 'registrationCode' && hideCodes ? '' : s[col]) ?? '';
    return this.students().slice().sort((a, b) =>
      value(a).localeCompare(value(b), undefined, { sensitivity: 'base' }) * dir);
  });

  constructor(
    private service: StudentCourseService,
    private courseService: CourseService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.courseId = +this.route.snapshot.paramMap.get('courseId')!;
    this.courseService.getById(this.courseId).subscribe(c => this.courseName.set(c.name));
    this.loading.set(true);
    this.service.getAll(this.courseId).subscribe({
      next: data => { this.students.set(data); this.loading.set(false); },
      error: () => { this.error.set('Failed to load students.'); this.loading.set(false); }
    });
  }

  sort(col: SortCol): void {
    if (this.sortCol() === col) {
      this.sortAsc.update(v => !v);
    } else {
      this.sortCol.set(col);
      this.sortAsc.set(true);
    }
  }

  sortIcon(col: SortCol): string {
    if (this.sortCol() !== col) return '↕';
    return this.sortAsc() ? '▲' : '▼';
  }
}
