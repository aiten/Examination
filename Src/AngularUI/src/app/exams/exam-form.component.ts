import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Exam } from '../models/exam.model';
import { ExamService } from '../services/exam.service';
import { Teacher } from '../models/teacher.model';
import { TeacherService } from '../services/teacher.service';
import { Course } from '../models/course.model';
import { CourseService } from '../services/course.service';

@Component({
  selector: 'app-exam-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Exam' : 'Edit Exam' }}</h2>
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Description *</label>
          <textarea name="description" [(ngModel)]="exam().description" required rows="3" class="form-control"></textarea>
        </div>
        <div class="form-group">
          <label>Type *</label>
          <select name="examType" [(ngModel)]="exam().examType" class="form-control">
            <option [ngValue]="0">Standard</option>
            <option [ngValue]="1">Repeated</option>
          </select>
        </div>
        <div class="form-group">
          <label>Teacher *</label>
          <select name="teacherId" [(ngModel)]="exam().teacherId" required class="form-control">
            <option [ngValue]="0" disabled>— select teacher —</option>
            @for (t of teachers(); track t.id) {
              <option [ngValue]="t.id">
                {{ t.lastName }}{{ t.firstName ? ', ' + t.firstName : '' }}
              </option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Course *</label>
          <select name="courseId" [(ngModel)]="exam().courseId" required class="form-control">
            <option [ngValue]="0" disabled>— select course —</option>
            @for (c of courses(); track c.id) {
              <option [ngValue]="c.id">{{ c.name }}</option>
            }
          </select>
        </div>
        <div class="form-group">
          <label>Date *</label>
          <input type="date" name="date" [(ngModel)]="exam().date" required class="form-control" />
        </div>
        <div class="form-group">
          <label>From *</label>
          <input type="time" name="from" [(ngModel)]="exam().from" required class="form-control" />
        </div>
        <div class="form-group">
          <label>To *</label>
          <input type="time" name="to" [(ngModel)]="exam().to" required class="form-control" />
        </div>
        @if (exam().from && exam().to && exam().from >= exam().to) {
          <p class="error">End time must be after start time.</p>
        }
        <div class="form-group">
          <label>PIN (10000–99999)</label>
          <input type="number" name="pin" [(ngModel)]="exam().pin" min="10000" max="99999" class="form-control" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid || (exam().from && exam().to && exam().from >= exam().to)">Save</button>
          <a routerLink="/exams" class="btn">Cancel</a>
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </div>
  `
})
export class ExamFormComponent implements OnInit {
  exam = signal<Exam>({ id: 0, description: '', examType: 0, teacherId: 0, courseId: 0, date: '2026-05-07', from: '08:00', to: '09:00', pin: null });
  teachers = signal<Teacher[]>([]);
  courses = signal<Course[]>([]);
  isNew = true;
  error = signal('');

  constructor(
    private service: ExamService,
    private teacherService: TeacherService,
    private courseService: CourseService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.teacherService.getAll().subscribe(t =>
      this.teachers.set(t.slice().sort((a, b) =>
        a.lastName.localeCompare(b.lastName, undefined, { sensitivity: 'base' }) ||
        (a.firstName ?? '').localeCompare(b.firstName ?? '', undefined, { sensitivity: 'base' })
      ))
    );
    this.courseService.getAll().subscribe(c => this.courses.set(c));
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.service.getById(+id).subscribe(e => this.exam.set({
        ...e,
        from: e.from.slice(0, 5),
        to: e.to.slice(0, 5),
      }));
    }
  }

  save(): void {
    const e = this.exam();
    if (e.from >= e.to) {
      this.error.set('End time must be after start time.');
      return;
    }
    const payload: Exam = {
      ...e,
      from: e.from.length === 5 ? e.from + ':00' : e.from,
      to:   e.to.length   === 5 ? e.to   + ':00' : e.to,
    };
    const done = () => this.router.navigate(['/exams']);
    const fail = (err: any) => this.error.set(err.error?.detail ?? 'Save failed.');
    if (this.isNew) {
      this.service.create(payload).subscribe({ next: done, error: fail });
    } else {
      this.service.update(payload.id, payload).subscribe({ next: done, error: fail });
    }
  }
}
