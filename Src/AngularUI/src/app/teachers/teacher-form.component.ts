import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Teacher } from '../models/teacher.model';
import { TeacherService } from '../services/teacher.service';

@Component({
  selector: 'app-teacher-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Teacher' : 'Edit Teacher' }}</h2>
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Last Name *</label>
          <input name="lastName" [(ngModel)]="teacher().lastName" required minlength="3" class="form-control" />
        </div>
        <div class="form-group">
          <label>First Name</label>
          <input name="firstName" [(ngModel)]="teacher().firstName" class="form-control" />
        </div>
        <div class="form-group">
          <label>Nick Name</label>
          <input name="nickName" [(ngModel)]="teacher().nickName" class="form-control" />
        </div>
        <div class="form-group">
          <label>Abbreviation</label>
          <input name="abbreviation" [(ngModel)]="teacher().abbreviation" maxlength="10" class="form-control" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Save</button>
          <a routerLink="/teachers" class="btn">Cancel</a>
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
export class TeacherFormComponent implements OnInit {
  teacher = signal<Teacher>({ id: 0, firstName: null, lastName: '', nickName: null, abbreviation: null });
  isNew = true;
  error = signal('');

  constructor(private service: TeacherService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.service.getById(+id).subscribe(t => this.teacher.set(t));
    }
  }

  delete(): void {
    if (!confirm('Delete this teacher?')) return;
    this.service.delete(this.teacher().id).subscribe({
      next: () => this.router.navigate(['/teachers']),
      error: (err: any) => this.error.set(err.error?.detail ?? 'Delete failed.')
    });
  }

  save(): void {
    const done = () => this.router.navigate(['/teachers']);
    const fail = (err: any) => this.error.set(err.error?.detail ?? 'Save failed.');
    if (this.isNew) {
      this.service.create(this.teacher()).subscribe({ next: done, error: fail });
    } else {
      this.service.update(this.teacher().id, this.teacher()).subscribe({ next: done, error: fail });
    }
  }
}
