import { Component, OnInit, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Student } from '../models/student.model';
import { StudentService } from '../services/student.service';
import { ClassService } from '../services/class.service';
import { SchoolClass } from '../models/class.model';

type SortCol = 'lastName' | 'firstName' | 'class';

@Component({
  selector: 'app-student-list',
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
        <h2>Students</h2>
        <a routerLink="/students/new" class="btn btn-primary">+ New Student</a>
        <a routerLink="/students/import" class="btn">Import</a>
      </div>
      <div class="form-group">
        <label for="classFilter">Class</label>
        <select id="classFilter" class="form-control" (change)="classFilter.set(+$any($event.target).value)">
          <option value="0" [selected]="classFilter() === 0">&lt;All&gt;</option>
          @for (c of sortedClasses(); track c.id) {
            <option [value]="c.id" [selected]="classFilter() === c.id">{{ c.description }} ({{ c.year }})</option>
          }
        </select>
      </div>
      @if (error()) {
        <p class="error">{{ error() }}</p>
      }
      @if (loading()) {
        <p class="empty">Loading...</p>
      }
      @if (!loading() && sortedStudents().length > 0) {
        <table class="table">
          <thead>
            <tr>
              <th class="sortable" [class.sort-active]="sortCol() === 'lastName'" (click)="sort('lastName')">
                Last Name <span class="sort-icon">{{ sortIcon('lastName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'firstName'" (click)="sort('firstName')">
                First Name <span class="sort-icon">{{ sortIcon('firstName') }}</span>
              </th>
              <th class="sortable" [class.sort-active]="sortCol() === 'class'" (click)="sort('class')">
                Classes <span class="sort-icon">{{ sortIcon('class') }}</span>
              </th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (s of sortedStudents(); track s.id) {
              <tr>
                <td>{{ s.lastName }}</td>
                <td>{{ s.firstName }}</td>
                <td>{{ classLabels(s) }}</td>
                <td>
                  <a [routerLink]="['/students', s.id]" class="btn btn-sm">Edit</a>
                  @if (s.classIds.length === 0) {
                    <button type="button" class="btn btn-sm btn-danger" (click)="delete(s)">Delete</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (!loading() && sortedStudents().length === 0) {
        <p class="empty">No students found.</p>
      }
    </div>
  `
})
export class StudentListComponent implements OnInit {
  students = signal<Student[]>([]);
  classes = signal<SchoolClass[]>([]);
  loading = signal(false);
  error = signal('');
  sortCol = signal<SortCol>('lastName');
  sortAsc = signal(true);
  classFilter = signal(0); // 0 = <All>

  sortedClasses = computed(() =>
    this.classes().slice().sort((a, b) =>
      b.year - a.year || a.description.localeCompare(b.description, undefined, { sensitivity: 'base' })));

  sortedStudents = computed(() => {
    const col = this.sortCol();
    const asc = this.sortAsc();
    const filter = this.classFilter();
    const classMap = new Map(this.classes().map(c => [c.id, c]));
    return this.students().filter(s => filter === 0 || s.classIds.includes(filter)).sort((a, b) => {
      let va: string, vb: string;
      switch (col) {
        case 'lastName':  va = a.lastName;  vb = b.lastName;  break;
        case 'firstName': va = a.firstName; vb = b.firstName; break;
        case 'class':     va = this.classLabelsFrom(a, classMap); vb = this.classLabelsFrom(b, classMap); break;
      }
      const cmp = va.localeCompare(vb, undefined, { sensitivity: 'base' });
      return asc ? cmp : -cmp;
    });
  });

  constructor(private service: StudentService, private classService: ClassService) {}

  ngOnInit(): void {
    this.classService.getAll().subscribe(data => this.classes.set(data));
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: data => { this.students.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  private classLabelsFrom(student: Student, classMap: Map<number, SchoolClass>): string {
    return student.classIds
      .map(id => classMap.get(id))
      .filter(c => c != null)
      .sort((a, b) => a!.year - b!.year)
      .map(c => `${c!.description}(${c!.year})`)
      .join(', ');
  }

  classLabels(student: Student): string {
    return this.classLabelsFrom(student, new Map(this.classes().map(c => [c.id, c])));
  }

  delete(student: Student): void {
    if (!confirm(`Delete student ${student.firstName} ${student.lastName}?`)) return;
    this.error.set('');
    this.service.delete(student.id).subscribe({
      next: () => this.students.update(list => list.filter(s => s.id !== student.id)),
      error: (err: any) => this.error.set(err.error?.detail ?? 'Delete failed.')
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
