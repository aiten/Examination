import { Component, OnInit, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Course } from '../models/course.model';
import { CourseService } from '../services/course.service';
import { Subject } from '../models/subject.model';
import { SubjectService } from '../services/subject.service';
import { SchoolClass } from '../models/class.model';
import { ClassService } from '../services/class.service';
import { Teacher } from '../models/teacher.model';
import { TeacherService } from '../services/teacher.service';

@Component({
  selector: 'app-course-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  styles: [`
    .checkbox-list { display: flex; flex-direction: column; gap: 6px; margin-top: 4px; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; font-weight: normal; cursor: pointer; }
  `],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Course' : 'Edit Course' }}</h2>
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Name *</label>
          <input name="name" [(ngModel)]="course().name" required minlength="2" class="form-control" />
        </div>
        <div class="form-group">
          <label>Year *</label>
          <input type="number" name="year" [ngModel]="year()" (ngModelChange)="onYearChange($event)"
            required min="2020" max="2035" class="form-control" />
        </div>
        <div class="form-group">
          <label>Subject *</label>
          <select name="subjectId" [(ngModel)]="course().subjectId" required class="form-control">
            <option [ngValue]="0" disabled>— select subject —</option>
            @for (s of subjects(); track s.id) {
              <option [ngValue]="s.id">{{ s.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Classes {{ year() ? '(' + year() + ')' : '' }}</label>
          @if (!year()) {
            <p class="hint">Set a year first to see matching classes.</p>
          }
          <div class="checkbox-list">
            @for (c of filteredClasses(); track c.id) {
              <label class="checkbox-item">
                <input type="checkbox"
                  [checked]="selectedClassIds().has(c.id)"
                  (change)="toggleClass(c.id, $any($event.target).checked)" />
                {{ c.description + ' (' + c.year + ')' }}
              </label>
            }
          </div>
        </div>
        <div class="form-group">
          <label>Teachers</label>
          <div class="checkbox-list">
            @for (t of teachers(); track t.id) {
              <label class="checkbox-item">
                <input type="checkbox"
                  [checked]="selectedTeacherIds().has(t.id)"
                  (change)="toggleTeacher(t.id, $any($event.target).checked)" />
                {{ t.lastName }}{{ t.firstName ? ', ' + t.firstName : '' }}
              </label>
            }
          </div>
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Save</button>
          <a routerLink="/courses" class="btn">Cancel</a>
          @if (!isNew) {
            <button type="button" class="btn btn-danger" (click)="delete()">Delete</button>
          }
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </div>
  `
})
export class CourseFormComponent implements OnInit {
  course = signal<Course>({ id: 0, name: '', year: 0, subjectId: 0, classIds: [], teacherIds: [] });
  // Separate signal so filteredClasses computed reacts to year changes from ngModel
  year = signal(0);

  subjects = signal<Subject[]>([]);
  allClasses = signal<SchoolClass[]>([]);
  teachers = signal<Teacher[]>([]);
  selectedClassIds = signal<Set<number>>(new Set());
  selectedTeacherIds = signal<Set<number>>(new Set());
  isNew = true;
  error = signal('');

  filteredClasses = computed(() =>
    this.allClasses().filter(c => c.year === this.year())
  );

  constructor(
    private service: CourseService,
    private subjectService: SubjectService,
    private classService: ClassService,
    private teacherService: TeacherService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.subjectService.getAll().subscribe(data =>
      this.subjects.set(data.slice().sort((a, b) =>
        a.name.localeCompare(b.name, undefined, { sensitivity: 'base' })
      ))
    );
    this.classService.getAll().subscribe(data => this.allClasses.set(data));
    this.teacherService.getAll().subscribe(data =>
      this.teachers.set(data.slice().sort((a, b) =>
        a.lastName.localeCompare(b.lastName, undefined, { sensitivity: 'base' }) ||
        (a.firstName ?? '').localeCompare(b.firstName ?? '', undefined, { sensitivity: 'base' })
      ))
    );

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.service.getById(+id).subscribe(c => {
        this.course.set(c);
        this.year.set(c.year);
        this.selectedClassIds.set(new Set(c.classIds));
        this.selectedTeacherIds.set(new Set(c.teacherIds));
      });
    }
  }

  onYearChange(val: number): void {
    this.year.set(+val);
    // Deselect classes that no longer match the new year
    const validIds = new Set(this.filteredClasses().map(c => c.id));
    this.selectedClassIds.set(new Set([...this.selectedClassIds()].filter(id => validIds.has(id))));
  }

  toggleClass(id: number, checked: boolean): void {
    const s = new Set(this.selectedClassIds());
    if (checked) s.add(id); else s.delete(id);
    this.selectedClassIds.set(s);
  }

  toggleTeacher(id: number, checked: boolean): void {
    const s = new Set(this.selectedTeacherIds());
    if (checked) s.add(id); else s.delete(id);
    this.selectedTeacherIds.set(s);
  }

  delete(): void {
    if (!confirm('Delete this course?')) return;
    this.service.delete(this.course().id).subscribe({
      next: () => this.router.navigate(['/courses']),
      error: (err: any) => this.error.set(err.error?.detail ?? 'Delete failed.')
    });
  }

  save(): void {
    const payload: Course = {
      ...this.course(),
      year: this.year(),
      classIds: [...this.selectedClassIds()],
      teacherIds: [...this.selectedTeacherIds()]
    };
    const done = () => this.router.navigate(['/courses']);
    const fail = (err: any) => this.error.set(err.error?.detail ?? 'Save failed.');
    if (this.isNew) {
      this.service.create(payload).subscribe({ next: done, error: fail });
    } else {
      this.service.update(payload.id, payload).subscribe({ next: done, error: fail });
    }
  }
}
