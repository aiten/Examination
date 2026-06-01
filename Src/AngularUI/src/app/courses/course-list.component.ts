import { Component, OnInit, signal, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Course } from '../models/course.model';
import { CourseService } from '../services/course.service';
import { Subject } from '../models/subject.model';
import { SubjectService } from '../services/subject.service';

type SortCol = 'name' | 'year' | 'subject';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [RouterModule],
  styles: [`
    th.sortable { cursor: pointer; user-select: none; white-space: nowrap; }
    th.sortable:hover { background: #e2e8f0; }
    .sort-icon { margin-left: 4px; font-size: .8em; opacity: .5; }
    th.sort-active .sort-icon { opacity: 1; }
  `],
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Courses</h2>
        <a routerLink="/courses/new" class="btn btn-primary">+ New Course</a>
      </div>
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && sorted().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'name'" (click)="sort('name')">
                Name <span class="sort-icon">{{ sortIcon('name') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'year'" (click)="sort('year')">
                Year <span class="sort-icon">{{ sortIcon('year') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'subject'" (click)="sort('subject')">
                Subject <span class="sort-icon">{{ sortIcon('subject') }}</span>
              </th>
              <th>Classes</th>
              <th>Teachers</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (c of sorted(); track c.id) {
              <tr>
                <td>{{ c.name }}</td>
                <td>{{ c.year }}</td>
                <td>{{ subjectName(c.subjectId) }}</td>
                <td>{{ c.classIds.length }}</td>
                <td>{{ c.teacherIds.length }}</td>
                <td>
                  <a [routerLink]="['/courses', c.id]" class="btn btn-sm">Edit</a>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (!loading() && courses().length === 0) {
        <p class="empty">No courses found.</p>
      }
    </div>
  `
})
export class CourseListComponent implements OnInit {
  courses = signal<Course[]>([]);
  subjects = signal<Subject[]>([]);
  loading = signal(false);
  sortCol = signal<SortCol>('year');
  sortAsc = signal(false);

  private subjectMap = computed(() => new Map(this.subjects().map(s => [s.id, s.name])));

  sorted = computed(() => {
    const col = this.sortCol();
    const asc = this.sortAsc();
    const smap = this.subjectMap();
    return this.courses().slice().sort((a, b) => {
      let cmp: number;
      switch (col) {
        case 'name':    cmp = a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }); break;
        case 'year':    cmp = a.year - b.year; break;
        case 'subject': cmp = (smap.get(a.subjectId) ?? '').localeCompare(smap.get(b.subjectId) ?? '', undefined, { sensitivity: 'base' }); break;
        default:        cmp = 0;
      }
      return asc ? cmp : -cmp;
    });
  });

  constructor(
    private service: CourseService,
    private subjectService: SubjectService
  ) {}

  ngOnInit(): void {
    this.subjectService.getAll().subscribe(data => this.subjects.set(data));
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: data => { this.courses.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  subjectName(id: number): string {
    return this.subjectMap().get(id) ?? '';
  }

  sort(col: SortCol): void {
    if (this.sortCol() === col) {
      this.sortAsc.update(v => !v);
    } else {
      this.sortCol.set(col);
      this.sortAsc.set(col === 'year' ? false : true);
    }
  }

  sortIcon(col: SortCol): string {
    if (this.sortCol() !== col) return '↕';
    return this.sortAsc() ? '▲' : '▼';
  }
}
