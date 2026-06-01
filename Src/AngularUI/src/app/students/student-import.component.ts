import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentService } from '../services/student.service';

@Component({
  selector: 'app-student-import',
  standalone: true,
  imports: [FormsModule, RouterModule],
  template: `
    <div class="page">
      <h2>Import Students</h2>
      <form (ngSubmit)="import()" class="form">
        <div class="form-group">
          <label>Students (one per line)</label>
          <textarea
            name="csvData"
            [(ngModel)]="csvData"
            rows="12"
            class="form-control"
            placeholder="Lastname;Firstname;ClassName(Year),ClassName2(Year2)&#10;e.g. Mustermann;Max;1ahif(2024),2ahif(2025)"
            style="font-family: monospace; resize: vertical;"
          ></textarea>
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="importing() || !csvData.trim()">
            {{ importing() ? 'Importing…' : 'Import' }}
          </button>
          <a routerLink="/students" class="btn">Cancel</a>
        </div>
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
      </form>
    </div>
  `
})
export class StudentImportComponent {
  csvData = '';
  importing = signal(false);
  error = signal('');

  constructor(private service: StudentService, private router: Router) {}

  import(): void {
    const lines = this.csvData
      .split('\n')
      .map(l => l.trim())
      .filter(l => l.length > 0);

    if (lines.length === 0) return;

    this.importing.set(true);
    this.error.set('');

    this.service.importStudents(lines).subscribe({
      next: () => this.router.navigate(['/students']),
      error: err => {
        this.error.set(err.error?.detail ?? 'Import failed.');
        this.importing.set(false);
      }
    });
  }
}
