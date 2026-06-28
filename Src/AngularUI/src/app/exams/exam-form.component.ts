import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { Exam } from '../models/exam.model';
import { ExamService } from '../services/exam.service';
import { Teacher } from '../models/teacher.model';
import { TeacherService } from '../services/teacher.service';
import { Course } from '../models/course.model';
import { CourseService } from '../services/course.service';
import { SignalRService } from '../services/signalr.service';

@Component({
  selector: 'app-exam-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Exam' : 'Edit Exam' }}</h2>
      @if (reloadPending()) {
        <div class="reload-banner">
          This exam was updated by someone else.
          <button type="button" class="btn btn-primary" (click)="reloadExam()">Reload</button>
          <button type="button" class="btn" (click)="reloadPending.set(false)">Dismiss</button>
        </div>
      }
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Description *</label>
          <textarea name="description" [(ngModel)]="exam().description" required rows="3" class="form-control"></textarea>
        </div>
        <div class="form-group">
          <label>PIN (5 digits)</label>
          <input type="text" name="pin" [(ngModel)]="exam().pin" pattern="[0-9]{5}" maxlength="5" class="form-control" />
        </div>
        <div class="form-group">
          <label class="checkbox-label">
            <input type="checkbox" name="canRegister" [(ngModel)]="exam().canRegister" />
            Can Register
          </label>
        </div>
        <div class="form-group">
          <label class="checkbox-label">
            <input type="checkbox" name="canShowResults" [(ngModel)]="exam().canShowResults" />
            Can Show Results
          </label>
        </div>
        <div class="form-group">
          <label>Type *</label>
          <select name="examType" [(ngModel)]="exam().examType" class="form-control">
            <option [ngValue]="0">Standard</option>
            <option [ngValue]="1">Participation</option>
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
          <label>Date{{ exam().examType === 0 ? ' *' : '' }}</label>
          <input type="date" name="date" [(ngModel)]="exam().date" [required]="exam().examType === 0" class="form-control" />
        </div>
        <div class="form-group">
          <label>From{{ exam().examType === 0 ? ' *' : '' }}</label>
          <input type="time" name="from" [(ngModel)]="exam().from" [required]="exam().examType === 0" class="form-control" />
        </div>
        <div class="form-group">
          <label>To{{ exam().examType === 0 ? ' *' : '' }}</label>
          <input type="time" name="to" [(ngModel)]="exam().to" [required]="exam().examType === 0" class="form-control" />
        </div>
        @if (exam().from && exam().to && exam().from! >= exam().to!) {
          <p class="error">End time must be after start time.</p>
        }
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid || !!(exam().from && exam().to && exam().from! >= exam().to!)">Save</button>
          <a routerLink="/exams" class="btn">Cancel</a>
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </div>
  `
})
export class ExamFormComponent implements OnInit, OnDestroy {
  exam = signal<Exam>({ id: 0, description: '', examType: 0, teacherId: 0, courseId: 0, date: null, from: null, to: null, pin: null, canRegister: true, canShowResults: false });
  teachers = signal<Teacher[]>([]);
  courses = signal<Course[]>([]);
  isNew = true;
  error = signal('');
  reloadPending = signal(false);

  private examId = 0;
  private signalRSub?: Subscription;

  constructor(
    private service: ExamService,
    private teacherService: TeacherService,
    private courseService: CourseService,
    private route: ActivatedRoute,
    private router: Router,
    private signalR: SignalRService
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
      this.examId = +id;
      this.service.getById(this.examId).subscribe(e => this.exam.set({
        ...e,
        from: e.from ? e.from.slice(0, 5) : null,
        to: e.to ? e.to.slice(0, 5) : null,
      }));
      this.signalR.joinExamGroup(this.examId);
      this.signalRSub = this.signalR.examUpdated$.subscribe(msg => {
        if (msg.examId === this.examId) {
          this.reloadPending.set(true);
        }
      });
    }
  }

  reloadExam(): void {
    this.reloadPending.set(false);
    this.service.getById(this.examId).subscribe(e => this.exam.set({
      ...e,
      from: e.from ? e.from.slice(0, 5) : null,
      to: e.to ? e.to.slice(0, 5) : null,
    }));
  }

  ngOnDestroy(): void {
    if (this.examId) {
      this.signalR.leaveExamGroup(this.examId);
    }
    this.signalRSub?.unsubscribe();
  }

  save(): void {
    const e = this.exam();
    const from = e.from || null;
    const to   = e.to   || null;
    if (from && to && from >= to) {
      this.error.set('End time must be after start time.');
      return;
    }
    const payload: Exam = {
      ...e,
      date: e.date || null,
      from: from ? (from.length === 5 ? from + ':00' : from) : null,
      to:   to   ? (to.length   === 5 ? to   + ':00' : to)   : null,
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
