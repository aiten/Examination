import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Student } from '../models/student.model';
import { StudentService } from '../services/student.service';
import { ClassService } from '../services/class.service';
import { SchoolClass } from '../models/class.model';

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  styles: [`
    .checkbox-list { display: flex; flex-direction: column; gap: 6px; margin-top: 4px; }
    .checkbox-item { display: flex; align-items: center; gap: 8px; font-weight: normal; cursor: pointer; }
  `],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Student' : 'Edit Student' }}</h2>
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Last Name *</label>
          <input name="lastName" [(ngModel)]="lastName" required minlength="2" class="form-control" />
        </div>
        <div class="form-group">
          <label>First Name *</label>
          <input name="firstName" [(ngModel)]="firstName" required minlength="2" class="form-control" />
        </div>
        <div class="form-group">
          <label>Classes</label>
          <div class="checkbox-list">
            @for (c of classes(); track c.id) {
              <label class="checkbox-item">
                <input type="checkbox"
                  [checked]="selectedClassIds().has(c.id)"
                  (change)="toggleClass(c.id, $any($event.target).checked)" />
                {{ c.description + ' (' + c.year + ')' }}
              </label>
            }
          </div>
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Save</button>
          <a routerLink="/students" class="btn">Cancel</a>
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
export class StudentFormComponent implements OnInit {
  firstName = '';
  lastName = '';
  isNew = true;
  studentId = 0;
  classes = signal<SchoolClass[]>([]);
  selectedClassIds = signal<Set<number>>(new Set());
  error = signal('');

  constructor(
    private service: StudentService,
    private classService: ClassService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.classService.getAll().subscribe(data => this.classes.set(data));

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.service.getById(+id).subscribe(s => {
        this.studentId = s.id;
        this.firstName = s.firstName;
        this.lastName = s.lastName;
        this.selectedClassIds.set(new Set(s.classIds));
      });
    }
  }

  toggleClass(id: number, checked: boolean): void {
    const s = new Set(this.selectedClassIds());
    if (checked) s.add(id); else s.delete(id);
    this.selectedClassIds.set(s);
  }

  delete(): void {
    if (!confirm('Delete this student?')) return;
    this.service.delete(this.studentId).subscribe({
      next: () => this.router.navigate(['/students']),
      error: (err: any) => this.error.set(err.error?.detail ?? 'Delete failed.')
    });
  }

  save(): void {
    const student: Student = {
      id: this.studentId,
      firstName: this.firstName,
      lastName: this.lastName,
      classIds: [...this.selectedClassIds()]
    };
    const done = () => this.router.navigate(['/students']);
    const fail = (err: any) => this.error.set(err.error?.detail ?? 'Save failed.');
    if (this.isNew) {
      this.service.create(student).subscribe({ next: done, error: fail });
    } else {
      this.service.update(this.studentId, student).subscribe({ next: done, error: fail });
    }
  }
}
