import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SchoolClass } from '../models/class.model';
import { ClassService } from '../services/class.service';
import { Teacher } from '../models/teacher.model';
import { TeacherService } from '../services/teacher.service';

@Component({
  selector: 'app-class-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Class' : 'Edit Class' }}</h2>
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Description *</label>
          <input name="description" [(ngModel)]="schoolClass().description" required class="form-control" />
        </div>
        <div class="form-group">
          <label>Year *</label>
          <input name="year" [(ngModel)]="schoolClass().year" type="number" required min="1980" max="2035" class="form-control" />
        </div>
        <div class="form-group">
          <label>Teacher</label>
          <select name="teacherId" [(ngModel)]="schoolClass().teacherId" class="form-control">
            <option [ngValue]="null">— none —</option>
            @for (t of teachers(); track t.id) {
              <option [ngValue]="t.id">
                {{ t.lastName }}{{ t.firstName ? ', ' + t.firstName : '' }}
              </option>
            }
          </select>
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Save</button>
          <a routerLink="/classes" class="btn">Cancel</a>
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
export class ClassFormComponent implements OnInit {
  schoolClass = signal<SchoolClass>({ id: 0, description: '', year: 2025, teacherId: null });
  teachers = signal<Teacher[]>([]);
  isNew = true;
  error = signal('');

  constructor(
    private service: ClassService,
    private teacherService: TeacherService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.teacherService.getAll().subscribe(t =>
      this.teachers.set(t.slice().sort((a, b) => a.lastName.localeCompare(b.lastName, undefined, { sensitivity: 'base' }) || (a.firstName ?? '').localeCompare(b.firstName ?? '', undefined, { sensitivity: 'base' })))
    );
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.service.getById(+id).subscribe(c => this.schoolClass.set(c));
    }
  }

  delete(): void {
    if (!confirm('Delete this class?')) return;
    this.service.delete(this.schoolClass().id).subscribe({
      next: () => this.router.navigate(['/classes']),
      error: (err: any) => this.error.set(err.error?.detail ?? 'Delete failed.')
    });
  }

  save(): void {
    const done = () => this.router.navigate(['/classes']);
    const fail = (err: any) => this.error.set(err.error?.detail ?? 'Save failed.');
    if (this.isNew) {
      this.service.create(this.schoolClass()).subscribe({ next: done, error: fail });
    } else {
      this.service.update(this.schoolClass().id, this.schoolClass()).subscribe({ next: done, error: fail });
    }
  }
}
