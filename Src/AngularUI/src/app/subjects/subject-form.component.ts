import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject } from '../models/subject.model';
import { SubjectService } from '../services/subject.service';

@Component({
  selector: 'app-subject-form',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <h2>{{ isNew ? 'New Subject' : 'Edit Subject' }}</h2>
      <form (ngSubmit)="save()" #form="ngForm" class="form">
        <div class="form-group">
          <label>Name *</label>
          <input name="name" [(ngModel)]="subject().name" required minlength="2" class="form-control" />
        </div>
        <div class="form-group">
          <label>Comment</label>
          <textarea name="comment" [(ngModel)]="subject().comment" maxlength="256" class="form-control" rows="3"></textarea>
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Save</button>
          <a routerLink="/subjects" class="btn">Cancel</a>
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
export class SubjectFormComponent implements OnInit {
  subject = signal<Subject>({ id: 0, name: '', comment: null });
  isNew = true;
  error = signal('');

  constructor(
    private service: SubjectService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isNew = false;
      this.service.getById(+id).subscribe(s => this.subject.set(s));
    }
  }

  delete(): void {
    if (!confirm('Delete this subject?')) return;
    this.service.delete(this.subject().id).subscribe({
      next: () => this.router.navigate(['/subjects']),
      error: (err: any) => this.error.set(err.error?.detail ?? 'Delete failed.')
    });
  }

  save(): void {
    const done = () => this.router.navigate(['/subjects']);
    const fail = (err: any) => this.error.set(err.error?.detail ?? 'Save failed.');
    if (this.isNew) {
      this.service.create(this.subject()).subscribe({ next: done, error: fail });
    } else {
      this.service.update(this.subject().id, this.subject()).subscribe({ next: done, error: fail });
    }
  }
}
