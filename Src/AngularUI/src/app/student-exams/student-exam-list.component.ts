import { Component, OnInit, computed, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { GradeSummary, StudentExamOverview } from '../models/student-exam.model';
import { StudentExamService } from '../services/student-exam.service';
import { ExamService } from '../services/exam.service';

type SortCol = keyof Pick<StudentExamOverview, 'lastName' | 'firstName' | 'loginName' | 'registrationCode' | 'countRated' | 'points' | 'percent' | 'grade'>;

@Component({
  selector: 'app-student-exam-list',
  standalone: true,
  imports: [RouterModule],
  styles: [`
    th.sortable { cursor: pointer; user-select: none; white-space: nowrap; }
    th.sortable:hover { background: #e4eaf0; }
    .sort-icon { margin-left: 4px; font-size: .75rem; opacity: .5; }
    th.sort-active .sort-icon { opacity: 1; }
    .col-reg-code { width: 100px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .col-actions { white-space: nowrap; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Registered Students{{ examDescription() ? ' — ' + examDescription() : '' }}</h2>
        <a routerLink="/exams" class="btn">Back to Exams</a>
      </div>

      @if (loading()) {
        <p class="empty">Loading...</p>
      }

      @if (!loading() && students().length > 0) {
        <table class="table" style="table-layout: fixed">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'lastName'" (click)="sort('lastName')">Last Name <span class="sort-icon">{{ sortIcon('lastName') }}</span></th>
              <th class="sortable" [class.sort-active]="sortCol() === 'firstName'" (click)="sort('firstName')">First Name <span class="sort-icon">{{ sortIcon('firstName') }}</span></th>
              <th class="sortable" [class.sort-active]="sortCol() === 'loginName'" (click)="sort('loginName')">Login <span class="sort-icon">{{ sortIcon('loginName') }}</span></th>
              <th class="sortable col-reg-code" [class.sort-active]="sortCol() === 'registrationCode'" (click)="sort('registrationCode')">Reg. Code <span class="sort-icon">{{ sortIcon('registrationCode') }}</span></th>
              <th class="sortable" [class.sort-active]="sortCol() === 'countRated'" (click)="sort('countRated')">Rated <span class="sort-icon">{{ sortIcon('countRated') }}</span></th>
              <th class="sortable" [class.sort-active]="sortCol() === 'points'" (click)="sort('points')">Points <span class="sort-icon">{{ sortIcon('points') }}</span></th>
              <th class="sortable" [class.sort-active]="sortCol() === 'percent'" (click)="sort('percent')">Percentage <span class="sort-icon">{{ sortIcon('percent') }}</span></th>
              <th class="sortable" [class.sort-active]="sortCol() === 'grade'" (click)="sort('grade')">Grade <span class="sort-icon">{{ sortIcon('grade') }}</span></th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (s of sortedStudents(); track s.id) {
              <tr>
                <td>{{ s.lastName }}</td>
                <td>{{ s.firstName }}</td>
                <td>{{ s.loginName }}</td>
                <td class="col-reg-code" [title]="s.registrationCode">{{ s.registrationCode }}</td>
                <td>{{ s.countRated }}</td>
                <td>{{ s.points }}</td>
                <td>{{ s.percent }}</td>
                <td>{{ s.grade }}</td>
                <td class="col-actions">
                  <a [routerLink]="['/exams', examId, 'students', s.id, 'edit']" class="btn btn-sm">Edit</a>
                  <a [routerLink]="['/exams', examId, 'students', s.id, 'subtasks']" class="btn btn-sm">Results</a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }

      @if (!loading() && students().length === 0) {
        <p class="empty">No students registered for this exam.</p>
      }

      @if (gradeSummary().length > 0) {
        <div style="margin-top: 24px;">
          <h3 style="font-size: 1.1rem; margin-bottom: 8px;">Grade Summary</h3>
          <table class="table" style="max-width: 300px;">
            <thead>
              <tr>
                <th>Grade</th>
                <th>Students</th>
              </tr>
            </thead>
            <tbody>
              @for (g of gradeSummary(); track g.grade) {
                <tr>
                  <td>{{ g.grade }}</td>
                  <td>{{ g.count }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }

      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
    </div>
  `
})
export class StudentExamListComponent implements OnInit {
  examId = 0;

  students = signal<StudentExamOverview[]>([]);
  gradeSummary = signal<GradeSummary[]>([]);
  examDescription = signal('');
  loading = signal(false);
  error = signal('');

  sortCol = signal<SortCol>('lastName');
  sortDir = signal<1 | -1>(1);

  sortedStudents = computed(() => {
    const col = this.sortCol();
    const dir = this.sortDir();
    return this.students().slice().sort((a, b) => {
      const av = a[col] ?? '';
      const bv = b[col] ?? '';
      if (typeof av === 'string' && typeof bv === 'string') {
        return av.localeCompare(bv, undefined, { sensitivity: 'base' }) * dir;
      }
      return ((av as number) - (bv as number)) * dir;
    });
  });

  constructor(
    private service: StudentExamService,
    private examService: ExamService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.examId = +this.route.snapshot.paramMap.get('examId')!;
    this.examService.getById(this.examId).subscribe(e => this.examDescription.set(e.description));
    this.load();
  }

  sort(col: SortCol): void {
    if (this.sortCol() === col) {
      this.sortDir.update(d => d === 1 ? -1 : 1);
    } else {
      this.sortCol.set(col);
      this.sortDir.set(1);
    }
  }

  sortIcon(col: SortCol): string {
    if (this.sortCol() !== col) return '↕';
    return this.sortDir() === 1 ? '▲' : '▼';
  }

  private load(): void {
    this.loading.set(true);
    this.service.getAll(this.examId).subscribe({
      next: data => {
        this.students.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
    this.service.getSummary(this.examId).subscribe({
      next: data => this.gradeSummary.set(data.slice().sort((a, b) => a.grade - b.grade))
    });
  }
}
